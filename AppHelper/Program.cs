/*  AppHelper
 *  
 *  Program.cs
 *  Version: v1.0.0 (2023.04.11)
 *  
 *  Contributor
 *      Arime-chan
 */

// See https://aka.ms/new-console-template for more information

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AppHelper
{
    public class Program
    {
        public static void Main(string[] _args)
        {
            Console.WriteLine();

            RegisterArgument("--outputVerInfo", 2, WriteVersionInfo);

            var arguments = CommandLineArgsParser.Parse(_args);

            //PrintAllArguments(arguments);

            foreach (var arg in arguments)
            {
                if (arg.Option == null)
                {
                    // TODO: handle optionless param;
                    continue;
                }

                if (s_ArgumentCallback.ContainsKey(arg.Option))
                    s_ArgumentCallback[arg.Option](arg.Params);
            }

            Console.WriteLine();
        }

        #region Callbacks
        private static ErrorCode WriteVersionInfo(List<string> _params)
        {
            Console.WriteLine("\nBuild check: x.x." + s_Build + "." + s_Revision);

            if (_params == null || _params.Count < 2)
            {
                Console.WriteLine("WriteVersionInfo: App info is not supplied.");
                return ErrorCode.InsufficientParams;
            }

            AssemblyName assemblyName = AssemblyName.GetAssemblyName(_params[0] + _params[1]);
            if (assemblyName.Version == null)
                return ErrorCode.AssemblyIsNull;

            Version version = assemblyName.Version;
            if (version == null)
                return ErrorCode.AssemblyVersionIsNull;

            Console.WriteLine("Assembly version: " + version.ToString());

            StreamWriter writer = new(_params[0] + "version.dat");
            writer.Write(version.Major + "." + version.Minor + "." + version.Build + "." + version.Revision);
            writer.Close();

            Console.WriteLine("Version info file written.");

            return ErrorCode.NoError;
        }
        #endregion

        internal static bool RegisterArgument(string _optionName, int _paramCount, Func<List<string>, ErrorCode> _callback)
        {
            if (!CommandLineArgsParser.RegisterArgument(_optionName, _paramCount))
                return false;

            // TODO: register callback;
            if (s_ArgumentCallback.ContainsKey(_optionName))
            {
                Console.WriteLine("Option '" + _optionName + "' has been registered.");
                return false;
            }

            s_ArgumentCallback[_optionName] = _callback;
            return true;
        }

        internal static void PrintAllArguments(List<Argument> _arguments)
        {
            foreach (var arg in _arguments)
            {
                string indent = "  ";
                if (arg.Option != null)
                {
                    Console.WriteLine(indent + arg.Option);
                    indent = "    ";
                }

                if (arg.Params == null)
                    continue;

                foreach (var param in arg.Params)
                {
                    Console.WriteLine(indent + param);
                }
            }
        }

        private static int s_Build = (int)(DateTime.Now - new DateTime(2022, 8, 31, 2, 18, 37, 135)).TotalDays;
        private static int s_Revision = (int)(DateTime.Now.TimeOfDay.TotalSeconds * 0.5);

        private static Dictionary<string, Func<List<string>, ErrorCode>> s_ArgumentCallback = new();
    }

    internal enum ErrorCode
    {
        NoError = 0,

        InsufficientParams,

        AssemblyIsNull,
        AssemblyVersionIsNull,
    }

}

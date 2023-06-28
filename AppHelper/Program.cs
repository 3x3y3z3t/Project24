/*  AppHelper
 *  
 *  Program.cs
 *  Version: v1.0 (2023.06.26)
 *  
 *  Contributor
 *      Arime-chan
 */

// See https://aka.ms/new-console-template for more information

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace AppHelper
{
    public class Program
    {
        public static void Main(string[] _args)
        {
            Console.WriteLine("AppHelper started.");

            RegisterArgument("--setup", 0, SetUpAppEnvironment);
            RegisterArgument("--setupquiet", 0, SetUpAppEnvironmentQuiet);

            RegisterArgument("--outputVerInfo", 2, WriteVersionInfo);
            RegisterArgument("--runUpdate", 0, null);

            var arguments = CommandLineArgsParser.Parse(_args);

            //PrintAllArguments(arguments);

            foreach (var arg in arguments)
            {
                if (arg.Option == null)
                {
                    // NOTE: handle optionless param;
                    continue;
                }

                if (s_ArgumentCallback.ContainsKey(arg.Option))
                {
                    ErrorCode code = s_ArgumentCallback[arg.Option](arg.Params);
                    Console.WriteLine(code.ToString());
                }
            }

            Console.WriteLine("AppHelper exiting..");
        }

        #region Callbacks

        private static ErrorCode SetUpAppEnvironment(List<string> _param)
        {
            Console.Write("Running setup will regenerate all Service files. Your customizations (if has any) will be lost.\nAre you sure you want to run setup? (y/n) ");
            ConsoleKeyInfo keyInfo = Console.ReadKey();

            if (keyInfo.Key != ConsoleKey.Y)
            {
                Console.WriteLine("\nUser input is interpreted as 'no'. Please run setup again if you made mistake.");
                return ErrorCode.UserCancelled;
            }

            return SetUpAppEnvironmentQuiet(_param);
        }

        private static ErrorCode SetUpAppEnvironmentQuiet(List<string> _param)
        {
                    string ss = (string.Format(FileContents.Linux_App_ServiceFile, Directory.GetCurrentDirectory(), "Project24", "orangepi"));

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("Window is not supported yet.");
                return ErrorCode.PlatformNotSupported;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                #region Linux - Write Service File
                Console.WriteLine("Writing Service file..");
                try
                {
                    StreamWriter writer = new("/etc/systemd/system/kestrel-p24-core6.service", false, Encoding.UTF8);

                    writer.Write(string.Format(FileContents.Linux_App_ServiceFile, Directory.GetCurrentDirectory(), "Project24", "orangepi"));
                    writer.Flush();

                    writer.Close();
                }
                catch (Exception _e)
                {
                    Console.WriteLine("" + _e);
                    return ErrorCode.Exception;
                }
                #endregion

                #region Linux - Write Updater Script File
                Console.WriteLine("Writing Updater Script file..");
                try
                {
                    StreamWriter writer = new("", false, Encoding.UTF8);

                    writer.Flush();

                    writer.Close();
                }
                catch (Exception _e)
                {
                    Console.WriteLine("" + _e);
                    return ErrorCode.Exception;
                }
                #endregion

                return ErrorCode.NoError;
            }

            return ErrorCode.NotImplemented;

        }

        /// <summary> Write the version information for the app to .dat file. This function takes a list of 2 params.</summary>
        /// <param name="_params">
        ///     1. The full PATH to the assembly exclude the trailing slash.
        ///     2. The assembly NAME.
        /// </param>
        private static ErrorCode WriteVersionInfo(List<string> _params)
        {
            Console.WriteLine("\nBuild check: x.x." + s_Build + "." + s_Revision);

            if (_params == null || _params.Count < 2)
            {
                Console.WriteLine("WriteVersionInfo: App info is not supplied.");
                return ErrorCode.InsufficientParams;
            }

            AssemblyName assemblyName = AssemblyName.GetAssemblyName(_params[0] + "/" + _params[1]);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_params"></param>
        private static ErrorCode RunAppUpdate(List<string> _params)
        {
            return ErrorCode.NotImplemented;



        }

        #endregion

        internal static bool RegisterArgument(string _optionName, int _paramCount, Func<List<string>, ErrorCode> _callback)
        {
            if (!CommandLineArgsParser.RegisterArgument(_optionName, _paramCount))
                return false;

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

        private static readonly int s_Build = (int)(DateTime.Now - new DateTime(2022, 8, 31, 2, 18, 37, 135)).TotalDays;
        private static readonly int s_Revision = (int)(DateTime.Now.TimeOfDay.TotalSeconds * 0.5);

        private static Dictionary<string, Func<List<string>, ErrorCode>> s_ArgumentCallback = new();
    }

    internal enum ErrorCode
    {
        PlatformNotSupported = -2,

        NotImplemented = -1,

        NoError = 0,
        UserCancelled,

        InsufficientParams,

        AssemblyIsNull,
        AssemblyVersionIsNull,

        Exception,
    }

}

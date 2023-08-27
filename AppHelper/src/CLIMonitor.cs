/*  CLIMonitor.cs
 *  Version: v1.1 (2023.08.25)
 *  
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;

namespace AppHelper
{
    internal enum ErrorCode
    {

        PlatformNotSupported = -4,

        NotImplemented = -3,

        FunctionNotFound = -2,
        InternalError = -1,

        OK = 0,
        Exception,
        ObjectIsNull,

        UserCancelled,

        InsufficientParams,

        AssemblyIsNull,
        AssemblyVersionIsNull,

    }

    internal class CLIMonitor
    {
        public bool IsDebugMore { get; set; } = false;


        public CLIMonitor()
        { }


        public void ParseCLA(string[] _args)
        {
            string argument = null;
            List<string> parameters = new();

            foreach (string arg in _args)
            {
                if (arg.StartsWith('-') || arg.StartsWith("--"))
                {
                    if (argument != null)
                    {
                        m_Arguments[argument] = parameters;
                        parameters = new();
                    }

                    argument = arg;
                    continue;
                }

                parameters.Add(arg);
            }

            if (argument != null)
                m_Arguments[argument] = parameters;
        }

        public bool RegisterArguments(string _args, Func<List<string>, ErrorCode> _callback)
        {
            bool hasErr = false;

            string[] ss = _args.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (string arg in ss)
            {
                if (m_Callbacks.ContainsKey(arg))
                {
                    Console.WriteLine("Argument '" + arg + "' has been registered.");
                    hasErr = true;
                    continue;
                }

                m_Callbacks[arg] = _callback;
            }

            return hasErr;
        }

        public bool TryInvokeAll()
        {
            if (m_Arguments.Count <= 0)
                return false;

            foreach (var pair in m_Arguments)
            {
                ErrorCode errCode = TryInvoke(pair.Key, pair.Value);

                string msg = "(Arg = " + pair.Key;
                if (pair.Value.Count > 0)
                {
                    msg += ", Params = ";
                    foreach (string s in pair.Value)
                    {
                        msg += s + " ";
                    }
                    msg = msg.Trim();
                }

                msg += ", ErrCode = " + errCode + ")\r\n";

                Console.WriteLine(msg);
            }

            return true;
        }

        public ErrorCode TryInvoke(string _arg, List<string> _params)
        {
            if (!m_Callbacks.ContainsKey(_arg))
            { return ErrorCode.FunctionNotFound; }

            return m_Callbacks[_arg](_params);
        }

        private readonly Dictionary<string, List<string>> m_Arguments = new();
        private readonly Dictionary<string, Func<List<string>, ErrorCode>> m_Callbacks = new();
    }

}

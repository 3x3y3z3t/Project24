/*  CommandLineArgsParser.cs
 *  Version: v1.0.0 (2023.04.10)
 *  
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;

namespace AppHelper
{
    internal struct Argument
    {
        public string Option;
        public List<string> Params;
    }

    internal static class CommandLineArgsParser
    {
        public static List<Argument> Parse(string[] _args)
        {
            HashSet<string> parsedArg = new();

            List<Argument> result = new List<Argument>();

            string option = null;
            List<string> param = new();

            List<string> optionlessArgument = new();

            int consumeArgs = 0;

            foreach (string arg in _args)
            {
                if (arg.StartsWith("-") || arg.StartsWith("--"))
                {
                    if (parsedArg.Contains(arg))
                        continue;

                    if (consumeArgs > 0 && option != null)
                    {
                        Console.WriteLine("Option '" + option + "' is not supplied with enough parameter, and will be ignored.");

                        option = null;
                        param = new();
                        consumeArgs = 0;
                    }

                    if (s_ArgumentParamsCount.ContainsKey(arg))
                    {
                        option = arg;
                        consumeArgs = s_ArgumentParamsCount[arg];

                        if (consumeArgs == 0)
                        {
                            result.Add(new() { Option = option, Params = null });
                            parsedArg.Add(option);

                            option = null;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Option '" + arg + "' is not registered. All following arguments will be treated as optionless argument.");

                        option = null;
                        consumeArgs = -1;
                    }

                    continue;
                }

                if (consumeArgs <= 0)
                {
                    optionlessArgument.Add(arg);
                    continue;
                }

                param.Add(arg);
                --consumeArgs;

                if (consumeArgs == 0)
                {
                    result.Add(new() { Option = option, Params = param });
                    parsedArg.Add(option);

                    option = null;
                    param = new();
                }
            }

            if (option != null)
            {
                if (consumeArgs > 0)
                {
                    Console.WriteLine("Option '" + option + "' is not supplied with enough parameter, and will be ignored.");
                }
                else
                {
                    result.Add(new() { Option = option, Params = null });
                    parsedArg.Add(option);
                }
            }

            result.Add(new() { Option = null, Params = optionlessArgument });

            return result;
        }

        public static bool RegisterArgument(string _optionName, int _paramCount)
        {
            if (s_ArgumentParamsCount.ContainsKey(_optionName))
                return false;

            s_ArgumentParamsCount[_optionName] = _paramCount;
            return true;
        }

        private static Dictionary<string, int> s_ArgumentParamsCount = new Dictionary<string, int>();
    }

}

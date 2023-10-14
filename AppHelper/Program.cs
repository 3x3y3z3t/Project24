/*  AppHelper
 *  
 *  Program.cs
 *  Version: v1.1 (2023.10.13)
 *  
 *  Author
 *      Arime-chan
 */

// See https://aka.ms/new-console-template for more information

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace AppHelper
{
    public partial class Program
    {
        public static void Main(string[] _args)
        {
            Console.WriteLine("========== AppHelper started. ==========\r\n");

            if (_args.Length <= 0)
            {
                _ = Callback_PrintHelpPage(null);
            }
            else
            {
                m_Monitor.ParseCLA(_args);
                

                m_Monitor.RegisterArguments("--help -?", Callback_PrintHelpPage);
                m_Monitor.RegisterArguments("--printCurrentUser", Callback_PrintCurrentUserName);
                m_Monitor.RegisterArguments("--outputVerInfo", Callback_WriteVersionInfo);
                m_Monitor.RegisterArguments("--setup", Callback_SetUpAppEnvironment);
                m_Monitor.RegisterArguments("--validateLocaleFiles", Callback_ValidateLocaleFiles);
                //m_Monitor.RegisterArguments("--testSysCaller", TestSystemCaller);


                if (!m_Monitor.TryInvokeAll())
                    _ = Callback_PrintHelpPage(null);
            }

            Console.WriteLine("========== AppHelper exiting.. ==========");
        }
        private static KeyValuePair<string, string>? ParseSingleLine(string _locale, string _line, int _index)
        {
            int pos = _line.IndexOf('=');
            if (pos < 0)
            {
                return null;
            }

            string key = _line[..pos].Trim();
            string value = _line[(pos + 1)..].Trim();

            if (key == "" || value == "")
            {
                return null;
            }

            return new(key, value);
        }

        // ==================================================;
        // Callbacks
        // ==================================================;
        #region Callbacks

        private static ErrorCode Callback_PrintHelpPage(List<string> _param)
        {
            Console.WriteLine("// TODO: print help page;\r\n");

            string page = "AppHelper accepts the following arguments:\r\n" +
                "  -?                       Prints this help page.\r\n" +
                "  --help                   Prints this help page.\r\n" +
                "  --outputVerInfo          Writes Project24 app's version info to a file.\r\n" +
                "  --printCurrentUser       Prints the username of current user (that run this app).\r\n" +
                "  --setup [quiet]          Runs initial setup for Project24. AppHelper will write service files for Project24 on Linux and overwrite existing files. Use \"quiet\" parameter to run quietly (no prompt for overwritten).\r\n" +
                "  --validateLocaleFiles    Validates locale files against Project24's data, and writes any missing entries." +
                "" +
                "";

            Console.WriteLine(page);

            return ErrorCode.NoError;
        }

        private static ErrorCode Callback_PrintCurrentUserName(List<string> _param)
        {
            string username = Environment.UserName;

            Console.WriteLine("Current User Name: " + username);

            return ErrorCode.NoError;
        }

        private static ErrorCode Callback_SetUpAppEnvironment(List<string> _param)
        {
            if (_param.Count <= 0 || _param[0] != "quiet")
            {
                Console.Write("" +
                    "Running setup will regenerate all Service and Config files. Your customization on these files will be lost.\r\n" +
                    "appsettings.*.json will not be overwrited.\r\n" +
                    "Are you sure you want to run setup? (y/n) ");
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                if (keyInfo.Key != ConsoleKey.Y)
                {
                    Console.WriteLine("\nUser input is interpreted as 'no'. Please run setup again if you made mistake.");
                    return ErrorCode.UserCancelled;
                }
            }

            return SetUpAppEnvironmentQuiet(_param);
        }

        private static ErrorCode SetUpAppEnvironmentQuiet(List<string> _0)
        {
            Console.WriteLine("SetUpAppEnvironmentQuiet()..");
            //string ss = string.Format(FileContents.Linux_App_ServiceFile, Directory.GetCurrentDirectory(), "Project24", "orangepi");

            string username = Environment.UserName;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("  Window is not supported yet.");
                if (LinuxHelperFileWriter.WriteAppsettingsFile() != ErrorCode.NoError)
                {
                    return ErrorCode.InternalError;
                }
                return ErrorCode.PlatformNotSupported;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                const string svcTypeName = "kestrel";
                const string appName = "p24-core6";
                const string execName = "Project24";
                const string description = "Project24 - .NET 6.0";
                const string prevDir = "prev";
                const string mainDir = "main";

                const string svcName = svcTypeName + "-" + appName;


                Console.WriteLine("  Stopping Services..");
                SystemCaller.ExecUnixCommand("sudo systemctl disable --now " + svcName + "-" + prevDir);
                SystemCaller.ExecUnixCommand("sudo systemctl disable --now " + svcName + "-" + mainDir);

                Console.WriteLine("  Writing Service files..");
                {
                    string workingDir = Directory.GetCurrentDirectory();
                    if (LinuxHelperFileWriter.WriteServiceFile(svcName, mainDir, workingDir, execName, username, description + " (main app)") != ErrorCode.NoError)
                        return ErrorCode.InternalError;
                }
                {
                    string workingDir = Path.GetFullPath(Directory.GetCurrentDirectory() + "/../" + prevDir);
                    if (LinuxHelperFileWriter.WriteServiceFile(svcName, prevDir, workingDir, execName, username, description + " (prev app)") != ErrorCode.NoError)
                        return ErrorCode.InternalError;
                }

                Console.WriteLine("  Writing Nginx VHost Config files..");
                if (LinuxHelperFileWriter.WriteNginxVHostConfigFile(appName) != ErrorCode.NoError)
                    return ErrorCode.InternalError;

                Console.WriteLine("  Creating directories..");
                try
                {
                    _ = Directory.CreateDirectory("../" + prevDir);
                    _ = Directory.CreateDirectory("../" + mainDir);
                }
                catch (Exception _e)
                {
                    Console.WriteLine("Exception during constructing directory structure: " + _e);
                    return ErrorCode.Exception;
                }

                if (LinuxHelperFileWriter.WriteAppsettingsFile() != ErrorCode.NoError)
                {
                    return ErrorCode.InternalError;
                }

                //Console.WriteLine("  Writing Updater Script file..");
                //_ = LinuxHelperFileWriter.WriteUpdaterScriptFile();

                // setting permission;
                SystemCaller.ExecUnixCommand("sudo chmod 777 Project24");

                Console.WriteLine("  Restarting Services..");
                SystemCaller.ExecUnixCommand("sudo systemctl restart nginx");
                SystemCaller.ExecUnixCommand("sudo systemctl disable --now " + svcName + "-prev");
                SystemCaller.ExecUnixCommand("sudo systemctl enable --now " + svcName + "-main");

                Console.WriteLine("  Done");
                return ErrorCode.NoError;
            }

            Console.WriteLine("  Platform is not supported.");
            return ErrorCode.NotImplemented;
        }

        /// <summary> Writes the version information for the app to .dat file. This function takes a list of 2 params.</summary>
        /// <param name="_params">
        ///     <c>1</c> The full PATH to the assembly.<br />
        ///     <c>2</c> The assembly NAME.<br />
        /// </param>
        private static ErrorCode Callback_WriteVersionInfo(List<string> _params)
        {
            Console.WriteLine("Build check: x.x." + s_Build + "." + s_Revision);

            if (_params == null || _params.Count < 2)
            {
                Console.WriteLine("WriteVersionInfo: App info is not supplied.");
                return ErrorCode.InsufficientParams;
            }

            if (!_params[0].EndsWith('/'))
                _params[0] += "/";


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

#if false
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_params"></param>
        private static ErrorCode RunAppUpdate(List<string> _0)
        {
            return ErrorCode.NotImplemented;
        }
#endif

#if false
        private static ErrorCode TestSystemCaller(List<string> _params)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("  Window is not supported yet.");
                return ErrorCode.PlatformNotSupported;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string args = string.Join(' ', _params);

                SystemCaller.ExecUnixCommand(args);

                return ErrorCode.OK;
            }

            Console.WriteLine("  Platform is not supported.");
            return ErrorCode.NotImplemented;
        }
#endif

#endregion


        private static readonly CLIMonitor m_Monitor = new();
        private static readonly int s_Build = (int)(DateTime.Now - new DateTime(2022, 8, 31, 2, 18, 37, 135)).TotalDays;
        private static readonly int s_Revision = (int)(DateTime.Now.TimeOfDay.TotalSeconds * 0.5);
    }

}

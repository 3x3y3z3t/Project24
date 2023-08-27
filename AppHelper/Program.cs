/*  AppHelper
 *  
 *  Program.cs
 *  Version: v1.0 (2023.08.27)
 *  
 *  Contributor
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
    public class Program
    {
        public static void Main(string[] _args)
        {
            Console.WriteLine("========== AppHelper started. ==========\r\n");

            if (_args.Length <= 0)
            {
                _ = PrintHelpPage(null);
            }
            else
            {
                m_Monitor.ParseCLA(_args);
                

                m_Monitor.RegisterArguments("--help -?", PrintHelpPage);
                m_Monitor.RegisterArguments("--printCurrentUser", PrintCurrentUserName);
                m_Monitor.RegisterArguments("--outputVerInfo", WriteVersionInfo);
                m_Monitor.RegisterArguments("--setup", SetUpAppEnvironment);
                //m_Monitor.RegisterArguments("--testSysCaller", TestSystemCaller);


                if (!m_Monitor.TryInvokeAll())
                    _ = PrintHelpPage(null);
            }

            Console.WriteLine("========== AppHelper exiting.. ==========");
        }

        // ==================================================;
        // Callbacks
        // ==================================================;
        #region Callbacks

        private static ErrorCode PrintHelpPage(List<string> _param)
        {
            Console.WriteLine("// TODO: print help page;\r\n");

            string page = "AppHelper accepts the following arguments:\r\n" +
                "  -?                       Prints this help page.\r\n" +
                "  --help                   Prints this help page.\r\n" +
                "  --outputVerInfo          Writes Project24 app's version info to a file.\r\n" +
                "  --printCurrentUser       Prints the username of current user (that run this app).\r\n" +
                "  --setup [quiet]          Run initial setup for Project24. AppHelper will write service files for Project24 on Linux and overwrite existing files. Use \"quiet\" parameter to run quietly (no prompt for overwritten).\r\n" +
                "" +
                "";

            Console.WriteLine(page);

            return ErrorCode.OK;
        }

        private static ErrorCode PrintCurrentUserName(List<string> _param)
        {
            string username = Environment.UserName;

            Console.WriteLine("Current User Name: " + username);

            return ErrorCode.OK;
        }

        private static ErrorCode SetUpAppEnvironment(List<string> _param)
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
                if (LinuxHelperFileWriter.WriteAppsettingsFile() != ErrorCode.OK)
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
                    if (LinuxHelperFileWriter.WriteServiceFile(svcName, mainDir, workingDir, execName, username, description + " (main app)") != ErrorCode.OK)
                        return ErrorCode.InternalError;
                }
                {
                    string workingDir = Path.GetFullPath(Directory.GetCurrentDirectory() + "/../" + prevDir);
                    if (LinuxHelperFileWriter.WriteServiceFile(svcName, prevDir, workingDir, execName, username, description + " (prev app)") != ErrorCode.OK)
                        return ErrorCode.InternalError;
                }

                Console.WriteLine("  Writing Nginx VHost Config files..");
                if (LinuxHelperFileWriter.WriteNginxVHostConfigFile(appName) != ErrorCode.OK)
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

                if (LinuxHelperFileWriter.WriteAppsettingsFile() != ErrorCode.OK)
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
                return ErrorCode.OK;
            }

            Console.WriteLine("  Platform is not supported.");
            return ErrorCode.NotImplemented;
        }

        /// <summary> Write the version information for the app to .dat file. This function takes a list of 2 params.</summary>
        /// <param name="_params">
        ///     1. The full PATH to the assembly.
        ///     2. The assembly NAME.
        /// </param>
        private static ErrorCode WriteVersionInfo(List<string> _params)
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

            return ErrorCode.OK;
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

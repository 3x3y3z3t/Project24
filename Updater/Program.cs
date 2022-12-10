/*  Program.cs
 *  Version: 1.0 (2022.12.10)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Updater
{
    class Program
    {
        static void Main(string[] _args)
        {
            const string argFilename = "updater_args.cfg";
            const string logFilename = "../p24/updater.log";

            Logger logger = new Logger(logFilename);
            logger.WriteLine("Updater started.");

            if (!File.Exists(argFilename))
            {
                logger.WriteLine("Argument file missing.");
                logger.WriteLine("Update failed!");
                return;
            }

            int processId;
            string appAbsPath;
            string nextAbsPath;
            try
            {
                StreamReader reader = File.OpenText(argFilename);
                string pid = reader.ReadLine();
                appAbsPath = reader.ReadLine();
                nextAbsPath = reader.ReadLine();
                reader.Close();

                int.TryParse(pid, out processId);
            }
            catch (Exception _e)
            {
                logger.WriteLine("Exception during reading argument file: " + _e);
                logger.WriteLine("Update failed!");
                return;
            }

            if (processId == 0 || string.IsNullOrEmpty(appAbsPath) || string.IsNullOrEmpty(nextAbsPath))
            {
                logger.WriteLine("Invalid argument file content");
                logger.WriteLine("Update failed!");
                return;
            }

            logger.WriteLine("Args:");
            logger.WriteLine("    PID:      " + processId);
            logger.WriteLine("    appPath:  " + appAbsPath);
            logger.WriteLine("    nextPath: " + nextAbsPath);
            logger.Close();

            Task.Delay(4000).Wait(); // wait 4 seconds to make sure main app exited;

            /* From rsync's man page: 
             *      A trailing / on a source name means "copy the contents of this directory".
             *      Without a trailing slash it means "copy the directory".
             */
            string command = $"'{nextAbsPath}/update.sh' -main {processId} '{appAbsPath}/' '{nextAbsPath}/' >> '{appAbsPath}/../updater.log'";
            ExecUnixCommand(command, true);
            //ExecUnixCommand($"'{nextAbsPath}/update.sh' -launchApp", true);

            logger = new Logger(logFilename, true);
            logger.WriteLine("Exiting Updater..");
            logger.Close();
        }

        private static void ExecUnixCommand(string _command, bool _waitForExit = false)
        {
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = true,
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{_command}\""
                }
            };

            process.Start();

            if (_waitForExit)
                process.WaitForExit(120 * 1000); // wait 2 minutes;
        }

        private static void ExecUnixCommandNoHangup(string _command)
        {
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = true,
                    FileName = "/bin/nohup",
                    Arguments = _command
                }
            };

            process.Start();
            //return process.WaitForExit(5 * 1000); // wait 5 second;
        }
    }

}

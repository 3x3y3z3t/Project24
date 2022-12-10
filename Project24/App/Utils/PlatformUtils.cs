/*  PlatformUtils.cs
 *  Version: 1.0 (2022.12.06)
 *
 *  Contributor
 *      Arime-chan
 */

using System.Diagnostics;

namespace Project24.App.Utils
{
    public static class PlatformUtils
    {
        public static void ExecUnixCommand(string _command)
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
            //return process.WaitForExit(5 * 1000); // wait 5 second;
        }
    }
}

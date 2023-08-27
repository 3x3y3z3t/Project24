/*  SystemCaller.cs
 *  Version: v1.0 (2023.08.10)
 *  
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Diagnostics;

namespace AppHelper
{
    public static class SystemCaller
    {
        public static void ExecUnixCommand(string _command, bool _waitForExit = false)
        {
            Console.WriteLine("> " + _command);

            using Process process = new();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{_command}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;

            process.Start();

            string output = process.StandardOutput.ReadToEnd();

            Console.WriteLine("|> " + output);

            process.WaitForExit();


        }
    }

}

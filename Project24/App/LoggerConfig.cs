/*  App/LoggerConfig.cs
 *  Version: v1.0 (2023.09.21)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.IO;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;

namespace Project24.App
{
    public static class LoggerConfig
    {
        public static void ConfigureLogger(ILoggingBuilder _builder)
        {
            string filename = Path.GetFullPath(Program.WorkingDir + "/../logs/" + Program.AppSide + "_" + Program.CurrentSessionName + ".log");
            _builder.AddFile(filename, ConfigureFileLoggerOptions);
        }


        private static void ConfigureFileLoggerOptions(FileLoggerOptions _options)
        {
            _options.Append = false;
            _options.FileSizeLimitBytes = Constants.MaxLogFileSize;
            _options.MinLevel = LogLevel.Trace;
            _options.FormatLogEntry = FormatLogEntry;
            _options.HandleFileError = HandleFileError;
        }

        private static string FormatLogEntry(LogMessage _msg)
        {
            string res = ""
                + string.Format("[{0:yyyy.MM.dd HH:mm:ss.fff}]", DateTime.Now)
                + "[" + _msg.LogLevel + "]"
                + "[" + _msg.LogName + "]"
                + "[" + _msg.EventId + "]"
                + "\r\n"
                + "    " + _msg.Message.Replace("\n", "\n    ");

            if (_msg.Exception != null)
            {
                res += "\r\n    > Exception < " + _msg.Exception.ToString() + "\r\n";
            }

            return res;
        }

        private static void HandleFileError(FileLoggerProvider.FileError _error)
        {

        }
    }

}

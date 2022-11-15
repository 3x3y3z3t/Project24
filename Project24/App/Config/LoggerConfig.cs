/*  LoggerConfig.cs
 *  Version: 1.1 (2022.11.15)
 *
 *  Contributor
 *      Arime-chan
 */

using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using System;

namespace Project24.App
{
    public static class LoggerConfig
    {
        public static void ConfigureLogger(ILoggingBuilder _builder)
        {
            _builder.AddFile("../logs/" + AppUtils.CurrentSessionName + "-log.log", ConfigureFileLoggerOptions);
        }

        private static void ConfigureFileLoggerOptions(FileLoggerOptions _options)
        {
            _options.Append = false;
            _options.FileSizeLimitBytes = AppConfig.MaxLogFileSize;
            _options.MinLevel = LogLevel.Trace;
            _options.FormatLogEntry = FormatLogEntry;
            _options.HandleFileError = HandleFileError;
        }

        private static string FormatLogEntry(LogMessage _msg)
        {
            string res = "";
            res += string.Format("[{0:yyyy}.{0:MM}.{0:dd} {0:HH}:{0:mm}:{0:ss}.{0:fff}]", DateTime.Now);
            res += "[" + _msg.LogLevel + "]";
            res += "[" + _msg.LogName + "]";
            res += "[" + _msg.EventId + "]";
            res += "\r\n";
            res += "    " + _msg.Message.Replace("\n", "\n    ");

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
/*  Callbacks.cs
 *  Version: v1.0 (2023.10.13)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using AppHelper.Helpers;

namespace AppHelper
{
    public partial class Program
    {
        #region ValidateLocaleFiles
        /// <summary>
        ///     Validates the locale files against the main app's data and writes missing entries as well as put them in correct order.
        /// </summary>
        /// <param name="_params">
        ///     <c>0</c> The full PATH to the main app's assembly.<br />
        ///     <c>1</c> The fully qualified type name for <c>LOCL</c> class.<br />
        ///     <c>2</c> The full PATH to the locale directory.<br />
        ///     <c>3..</c> All the locale files to write.<br />
        /// </param>
        private static ErrorCode Callback_ValidateLocaleFiles(List<string> _params)
        {
            Stopwatch sw = Stopwatch.StartNew();

            if (_params == null || _params.Count < 4)
            {
                sw.Stop();
                Console.WriteLine("ValidateLocaleFiles: Missing required parameters (in order): [Assembly Path] [LOCL Full Name] [Locale Directory Path] [Locale Name]s");
                return ErrorCode.InsufficientParams;
            }

            string assemblyPath;
            string localePath;


            try
            {
                assemblyPath = Path.GetFullPath(_params[0]);
            }
            catch (Exception)
            {
                sw.Stop();
                Console.WriteLine("ValidateLocaleFiles: Invalid [Assembly Path]: " + _params[0]);
                return ErrorCode.InvalidParams;
            }

            try
            {
                localePath = Path.GetFullPath(_params[2]);
            }
            catch (Exception)
            {
                sw.Stop();
                Console.WriteLine("ValidateLocaleFiles: Invalid [Locale Directory Path]: " + _params[2]);
                return ErrorCode.InvalidParams;
            }

            string[] baseEntries = TryGetEntriesFromAssembly(assemblyPath, _params[1], out int longestEntryLength);
            if (baseEntries == null)
                return ErrorCode.InvalidParams;
            if (baseEntries.Length == 0)
                return ErrorCode.ReflectionFieldsNotFound;

            int remainder = 4 - (longestEntryLength % 4);
            int alignLength = longestEntryLength + remainder + 4;

            ErrorCode accumulatedErrorCode = ErrorCode.NoError;
            for (int i = 3; i < _params.Count; ++i)
            {
                if (_params[i].Contains("./"))
                {
                    Console.WriteLine("ValidateLocaleFiles: Invalid [Locale Name " + (i - 3) + "]: " + _params[i]);
                    accumulatedErrorCode = ErrorCode.InvalidParams;
                    continue;
                }

                string path;
                try
                {
                    path = Path.GetFullPath(localePath + "/" + _params[i] + ".ini");
                }
                catch (Exception _ex)
                {
                    Console.WriteLine("ValidateLocaleFiles: Invalid locale file.\n" + _ex);
                    accumulatedErrorCode = ErrorCode.InvalidParams;
                    continue;
                }

                // ==================== read ====================;
                var localeFile = LocaleFileUtils.ReadLocaleFile(path, _params[i]);
                if (localeFile.Error != null)
                {
                    accumulatedErrorCode = ErrorCode.InvalidParams;
                }

                // ==================== parse ====================;
                string lastPrefix = "";
                string writeEntries = "";
                foreach (string entry in baseEntries)
                {
                    int pos = entry.IndexOf('_');
                    string prefix = entry[..pos];
                    if (prefix != lastPrefix)
                    {
                        writeEntries += "\r\n; " + GetPrefixDescription(prefix) + "\r\n";
                        lastPrefix = prefix;
                    }

                    writeEntries += string.Format("{0,-" + alignLength + "}= ", entry);
                    if (localeFile.Entries.ContainsKey(entry))
                        writeEntries += localeFile.Entries[entry];
                    writeEntries += "\r\n";
                }

                writeEntries += "\r\n; Malformed entries\r\n";
                foreach (string key in localeFile.Entries.Keys)
                {
                    if (key.StartsWith("<>"))
                        writeEntries += localeFile.Entries[key] + "\r\n";
                }

                // ==================== write ====================;
                string content = localeFile.Header + writeEntries;
                try
                {
                    File.WriteAllText(path, content, Encoding.UTF8);
                }
                catch (Exception _ex)
                {
                    Console.WriteLine("ValidateLocaleFiles: Error during writing back locale file " + _params[1] + ":\n" + _ex);
                    if (accumulatedErrorCode == ErrorCode.NoError)
                        accumulatedErrorCode = ErrorCode.Exception;
                }
            }

            sw.Stop();
            Console.WriteLine("Validating Locale files took: " + sw.Elapsed.TotalMilliseconds.ToString("0.000") + " ms");

            return accumulatedErrorCode;
        }

        private static string[] TryGetEntriesFromAssembly(string _fullname, string _fullTypeName, out int _longestEntryLength)
        {
            List<string> entries;
            _longestEntryLength = 0;

            try
            {
                Assembly assembly = Assembly.LoadFrom(_fullname);
                var type = assembly.GetType(_fullTypeName);
                var fields = type.GetFields();

                entries = new List<string>();
                foreach (FieldInfo field in fields)
                {
                    entries.Add(field.Name);
                    if (_longestEntryLength < field.Name.Length)
                        _longestEntryLength = field.Name.Length;
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine("ValidateLocaleFiles: Could not get LOCL entries (exception).\n" + _ex);
                return null;
            }

            return entries.ToArray();
        }

        private static string GetPrefixDescription(string _prefix)
        {
            switch (_prefix)
            {
                case "BTN":
                    return "Button Labels";
                case "LBL":
                    return "Labels";
                case "PAGE":
                    return "Page Titles";
                case "STR":
                    return "Short Strings";
                case "DESC":
                    return "Long Strings (Descriptions)";
                case "SVRMSG":
                    return "";
            }

            return "";
        }
        #endregion
    }

}

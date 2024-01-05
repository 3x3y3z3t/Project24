/*  Callbacks.cs
 *  Version: v1.2 (2023.12.26)
 *  Spec:    v0.1
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
using System.Text.RegularExpressions;
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

            if (_params == null || _params.Count < 5)
            {
                sw.Stop();
                Console.WriteLine("ValidateLocaleFiles: Missing required parameters (in order):\n"
                    + "    0. [Assembly Path]\n"
                    + "    1. [LOCL Full Name]\n"
                    + "    2. [Locale Directory Path]\n"
                    + "    3. [js Localization Class Name]\n"
                    + "    4+ [Locale Name]s");
                return ErrorCode.InsufficientParams;
            }

            string assemblyPath;
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
            // TODO: validate assembly path (security issue);

            string localePath;
            try
            {
                localePath = Path.GetFullPath(_params[2]);
                if (!localePath.EndsWith("wwwroot"))
                    throw new ArgumentException(null);
            }
            catch (Exception)
            {
                sw.Stop();
                Console.WriteLine("ValidateLocaleFiles: Invalid [Locale Directory Path]: " + _params[2]);
                return ErrorCode.InvalidParams;
            }

            string loclTypeFullName = _params[1];

            List<string> requestedLocale = new();
            for (int i = 4; i < _params.Count; ++i)
                requestedLocale.Add(_params[i]);

            ErrorCode accumulatedErrorCode = ErrorCode.NoError;

            // ==================================================;

            ErrorCode errCode = ValidateBackEndLocaleFiles(assemblyPath, loclTypeFullName, localePath + "/res/locale", requestedLocale);
            if (errCode != ErrorCode.NoError)
                accumulatedErrorCode = errCode;

            errCode = ValidateFrontEndLocaleFiles(localePath + "/js/locale", requestedLocale, _params[3]);
            if (errCode != ErrorCode.NoError)
                accumulatedErrorCode = errCode;

            sw.Stop();
            Console.WriteLine("Validating Locale files took: " + sw.Elapsed.TotalMilliseconds.ToString("0.000") + " ms");

            return accumulatedErrorCode;
        }

        private static ErrorCode ValidateBackEndLocaleFiles(string _assemblyPath, string _loclTypeFullName, string _backEndLocaleFileDir, List<string> _requestedLocale)
        {
            string[] baseKeys = TryGetLocaleKeysFromAssembly(_assemblyPath, _loclTypeFullName, out int longestEntryLength);
            if (baseKeys == null)
                return ErrorCode.InvalidParams;
            if (baseKeys.Length == 0)
                return ErrorCode.ReflectionFieldsNotFound;

            int remainder = 4 - (longestEntryLength % 4);
            int alignLength = longestEntryLength + remainder + 4;

            ErrorCode accumulatedErrorCode = ErrorCode.NoError;
            for (int i = 0; i < _requestedLocale.Count; ++i)
            {
                if (_requestedLocale[i].Contains("./") || _requestedLocale[i].Contains("../"))
                {
                    Console.WriteLine("ValidateBackEndLocaleFiles: Invalid [Locale Name " + i + "]: " + _requestedLocale[i]);
                    accumulatedErrorCode = ErrorCode.InvalidParams;
                    continue;
                }

                string path;
                string fileName = _requestedLocale[i] + ".ini";
                try
                {
                    path = Path.GetFullPath(_backEndLocaleFileDir + "/" + fileName);
                }
                catch (Exception _ex)
                {
                    Console.WriteLine("ValidateBackEndLocaleFiles: Invalid locale file.\n" + _ex);
                    accumulatedErrorCode = ErrorCode.InvalidParams;
                    continue;
                }

                // ==================== read ====================;
                var localeFile = LocaleFileUtils.ReadLocaleFileIni(_backEndLocaleFileDir, fileName);
                if (localeFile.Error != null)
                {
                    accumulatedErrorCode = ErrorCode.InvalidParams;
                }

                // ==================== parse ====================;
                string lastPrefix = "";
                string writeEntries = "";
                foreach (string key in baseKeys)
                {
                    int pos = key.IndexOf('_');
                    string prefix = key[..pos];
                    if (prefix != lastPrefix)
                    {
                        writeEntries += "\r\n; " + GetPrefixDescription(prefix) + "\r\n";
                        lastPrefix = prefix;
                    }

                    if (localeFile.Entries.ContainsKey(key))
                        writeEntries += localeFile.Entries[key].ToString(-alignLength);
                    else
                        writeEntries += string.Format("{0,-" + alignLength + "} = ", key);

                    writeEntries += "\r\n";
                }

                if (localeFile.InvalidEntries.Count > 0)
                {
                    writeEntries += "\r\n; Invalid entries\r\n";
                    foreach (var entry in localeFile.InvalidEntries)
                    {
                        writeEntries += ";" + entry.RawString + "\r\n";
                    }
                }

                // ==================== write ====================;
                string content = localeFile.Header + writeEntries;
                try
                {
                    File.WriteAllText(path, content, Encoding.UTF8);
                }
                catch (Exception _ex)
                {
                    Console.WriteLine("ValidateBackEndLocaleFiles: Error during writing back locale file " + _requestedLocale[1] + ":\n" + _ex);
                    if (accumulatedErrorCode == ErrorCode.NoError)
                        accumulatedErrorCode = ErrorCode.Exception;
                }

                // ==================== done ====================;
            }

            return accumulatedErrorCode;
        }

        private static ErrorCode ValidateFrontEndLocaleFiles(string _frontEndLocaleDirectory, List<string> _requestedLocale, string _className)
        {
            string[] baseKeys;
            int longestEntryLength;
            {
                string keysFileFullname = _frontEndLocaleDirectory + "/" + LocaleFileUtils.FrontEndLocaleFileNamePrefix + "-keys.js";
                baseKeys = TryGetLocaleKeysFromJsFile(keysFileFullname, out longestEntryLength);
                if (baseKeys == null)
                    return ErrorCode.InvalidParams;
                if (baseKeys.Length == 0)
                    return ErrorCode.ReflectionFieldsNotFound;
            }

            int remainder = 4 - (longestEntryLength % 4);
            int alignLength = longestEntryLength + remainder + 4;

            ErrorCode accumulatedErrorCode = ErrorCode.NoError;
            for (int i = 0; i < _requestedLocale.Count; ++i)
            {
                if (_requestedLocale[i].Contains("./") || _requestedLocale[i].Contains("../"))
                {
                    Console.WriteLine("ValidateFrontEndLocaleFiles: Invalid [Locale Name " + i + "]: " + _requestedLocale[i]);
                    accumulatedErrorCode = ErrorCode.InvalidParams;
                    continue;
                }

                string path;
                string fileName = LocaleFileUtils.FrontEndLocaleFileNamePrefix + "-" + _requestedLocale[i].ToLower() + ".js";
                try
                {
                    path = Path.GetFullPath(_frontEndLocaleDirectory + "/" + fileName);
                }
                catch (Exception _ex)
                {
                    Console.WriteLine("ValidateFrontEndLocaleFiles: Invalid locale file.\n" + _ex);
                    accumulatedErrorCode = ErrorCode.InvalidParams;
                    continue;
                }

                // ==================== read ====================;
                var localeFile = LocaleFileUtils.ReadLocaleFileJs(_frontEndLocaleDirectory, fileName);
                if (localeFile.Error != null)
                {
                    accumulatedErrorCode = ErrorCode.InvalidParams;
                    continue;
                }

                // ==================== parse ====================;
                string writeEntries = "";
                foreach (string key in baseKeys)
                {
                    if (key == "")
                    {
                        writeEntries += "\r\n";
                        continue;
                    }

                    writeEntries += "    ";
                    if (localeFile.Entries.ContainsKey(key))
                        writeEntries += localeFile.Entries[key].ToString(-alignLength);
                    else
                        writeEntries += string.Format("{0,-" + alignLength + "}: \"\",", key);

                    writeEntries += "\r\n";
                }

                if (localeFile.InvalidEntries.Count > 0)
                {
                    writeEntries += "\r\n    // Invalid entries;\r\n";
                    foreach (var entry in localeFile.InvalidEntries)
                    {
                        writeEntries += "    //" + entry.RawString + "\r\n";
                    }
                }

                // ==================== write ====================;
                string content = localeFile.Header
                    + "\r\n"
                    + "window." + _className + " = {\r\n"
                    + "    get: function (_key) { if (this[_key] == null || this[_key].trim() == \"\") return \"<code>{\" + _key + \"}</code>\"; return this[_key]; },\r\n"
                    + writeEntries
                    + "\r\n};\r\n";

                try
                {
                    File.WriteAllText(path, content, Encoding.UTF8);
                }
                catch (Exception _ex)
                {
                    Console.WriteLine("ValidateFrontEndLocaleFiles: Error during writing back locale file " + _requestedLocale[1] + ":\n" + _ex);
                    if (accumulatedErrorCode == ErrorCode.NoError)
                        accumulatedErrorCode = ErrorCode.Exception;
                }

                // ==================== done ====================;
            }

            return accumulatedErrorCode;
        }

        private static string[] TryGetLocaleKeysFromAssembly(string _fullname, string _fullTypeName, out int _longestEntryLength)
        {
            List<string> entries;
            _longestEntryLength = 0;

            try
            {
                Assembly assembly = Assembly.LoadFrom(_fullname);
                var type = assembly.GetType(_fullTypeName);
                var fields = type.GetFields();

                entries = new List<string>(fields.Length);
                foreach (FieldInfo field in fields)
                {
                    entries.Add(field.Name);
                    if (_longestEntryLength < field.Name.Length)
                        _longestEntryLength = field.Name.Length;
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine("TryGetLocaleKeysFromAssembly: Exception during reading keys from assembly:\n" + _ex);
                return null;
            }

            return entries.ToArray();
        }

        private static string[] TryGetLocaleKeysFromJsFile(string _fullname, out int _longestEntryLength)
        {
            List<string> entries;
            _longestEntryLength = 0;

            string[] lines;
            try
            {
                lines = File.ReadAllLines(_fullname);
            }
            catch (Exception _ex)
            {
                Console.WriteLine("TryGetLocaleKeysFromJsFile: Could not read lines (exception).\n" + _ex);
                return null;
            }

            string keyValueMismatch = "";
            bool isInsideCommentBlock = false;

            entries = new List<string>(lines.Length);
            for (int i = 0; i < lines.Length; ++i)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("*/"))
                {
                    isInsideCommentBlock = false;
                    continue;
                }

                if (isInsideCommentBlock)
                    continue;

                if (line == "")
                {
                    entries.Add("");
                    continue;
                }

                if (line.StartsWith("//"))
                    continue;

                if (line.StartsWith("/*"))
                {
                    isInsideCommentBlock = true;
                    continue;
                }

                const string pattern = @"\s*([A-Z0-9_]+)\s*=\s*""(.+)"";\s*(//.*)*";
                var match = Regex.Match(line, pattern);

                if (!match.Success)
                    continue;

                string key = match.Groups[1].Value.Trim();
                string value = match.Groups[2].Value.Trim();

                if (key != value)
                {
                    keyValueMismatch += "    (" + (i + 1) + ") " + key + " = " + value + "\n";
                    //key = "<>" + key;
                }

                if (_longestEntryLength < key.Length)
                    _longestEntryLength = key.Length;

                entries.Add(key);
            }

            if (keyValueMismatch != "")
                Console.WriteLine("TryGetLocaleKeysFromJsFile: Key is malformed (key and key name mismatch):\n" + keyValueMismatch);

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

/*  Helpers/LocaleFileUtils.cs
 *  Version: v1.2 (2023.12.26)
 *  Spec:    v0.1
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace AppHelper.Helpers
{
    public static class LocaleFileUtils
    {
        public const string FrontEndLocaleFileNamePrefix = "p24-localization";


        public class LocaleFile
        {
            //public string LocaleName { get; set; }
            public string Error { get; set; } = null;

            public string Header { get; set; } = null;
            public Dictionary<string, LocaleEntry> Entries { get; set; } = null;
            public List<LocaleEntry> InvalidEntries { get; set; } = null;

            public int TotalEntriesCount
            {
                get
                {
                    int count = 0;
                    if (Entries != null)
                        count += Entries.Count;
                    if (InvalidEntries != null)
                        count += InvalidEntries.Count;
                    return count;
                }
            }
        }

        /// <summary>
        ///     Stores the information of each locale entry (key-value pair)
        ///     along with its line number in locale file and validity status.
        /// </summary>
        public abstract class LocaleEntry
        {
            public bool IsValid { get; set; } = false;
            public string RawString { get; private set; } = null;
            public int LineNumber { get; set; } = 0;

            public string Key { get; set; } = null;
            public string Value { get; set; } = null;
            public string Comment { get; set; } = null;


            protected LocaleEntry(string _rawString, int _line, string _pattern)
            {
                RawString = _rawString;
                LineNumber = _line;

                var match = Regex.Match(_rawString, _pattern);

                if (!match.Success)
                    return;

                IsValid = true;
                Key = match.Groups[1].Value;
                Value = match.Groups[2].Value;
                if (match.Groups.Count == 4)
                    Comment = match.Groups[3].Value;
            }

            public abstract override string ToString();

            public abstract string ToString(int _alignLength = 0);
        }

        public class LocaleEntryIni : LocaleEntry
        {
            public LocaleEntryIni(string _rawString, int _line)
                : base(_rawString, _line, @"([A-Z0-9_]+)\s*=\s*(.*)")
            {
                if (!IsValid || string.IsNullOrWhiteSpace(Value))
                    return;

                int pos = Value.IndexOf(';');
                if (pos < 0)
                    return;

                Comment = Value[pos..].Trim();
                Value = Value[..pos].Trim();
            }


            public override string ToString() => ToString(0);

            public override string ToString(int _alignLength = 0)
            {
                if (!IsValid)
                    return "Invalid " + nameof(LocaleEntryIni);

                string res = "";
                if (_alignLength == 0)
                    res = Key;
                else
                    res = string.Format("{0," + _alignLength + "}", Key);

                res += " = " + Value;
                if (Comment != null)
                    res += " " + Comment;

                return res;
            }
        }

        public class LocaleEntryJs : LocaleEntry
        {
            public LocaleEntryJs(string _rawString, int _line)
                : base(_rawString, _line, @"([A-Z0-9_]+)\s*:\s*""(.*)"",\s*(//.*)*")
            { }


            public override string ToString() => ToString(0);

            public override string ToString(int _alignLength = 0)
            {
                if (!IsValid)
                    return "Invalid " + nameof(LocaleEntryIni);

                string res = "";
                if (_alignLength == 0)
                    res = Key;
                else
                    res = string.Format("{0," + _alignLength + "}", Key);

                res += ": \"" + Value + "\",";
                if (Comment != null)
                    res += " " + Comment;

                return res;
            }
        }


        public static LocaleFile ReadLocaleFileIni(string _filePath, string _fileName, bool _writeErrorToConsole = true)
        {
            string[] lines;
            try
            {
                lines = File.ReadAllLines(_filePath + "/" + _fileName, Encoding.UTF8);
            }
            catch (Exception _ex)
            {
                string msg = "ReadLocaleFileIni: Exception during reading locale file " + _fileName + ":\n" + _ex;
                if (_writeErrorToConsole)
                    Console.WriteLine(msg);

                return new LocaleFile() { Error = msg };
            }

            if (lines.Length == 0)
            {
                string msg = "Locale file " + _fileName + " is empty.";
                if (_writeErrorToConsole)
                    Console.WriteLine("Locale file is empty.");

                return new LocaleFile() { Error = msg };
            }

            string localeFileHeader = ReadLocaleFileIniHeader(lines, out int contentStartsPos);

            Dictionary<string, LocaleEntry> entries = new(lines.Length);
            List<LocaleEntry> invalidEntries = new(lines.Length);

            for (int i = contentStartsPos; i < lines.Length; ++i)
            {
                string line = lines[i].Trim();
                if (line.Length == 0 || line.StartsWith(';'))
                    continue;

                int lineNumber = i - contentStartsPos + 1;
                LocaleEntry entry = new LocaleEntryIni(line, lineNumber);

                if (entry.IsValid)
                    entries[entry.Key] = entry;
                else
                    invalidEntries.Add(entry);
            }

            return new LocaleFile()
            {
                Header = localeFileHeader,
                Entries = entries,
                InvalidEntries = invalidEntries
            };
        }

        public static LocaleFile ReadLocaleFileJs(string _filePath, string _fileName, bool _writeErrorToConsole = true)
        {
            string[] lines;
            try
            {
                lines = File.ReadAllLines(_filePath + "/" + _fileName, Encoding.UTF8);
            }
            catch (Exception _ex)
            {
                string msg = "ReadLocaleFileJs: Exception during reading locale file " + _fileName + ":\n" + _ex;
                if (_writeErrorToConsole)
                    Console.WriteLine(msg);

                return new LocaleFile() { Error = msg };
            }

            if (lines.Length == 0)
            {
                string msg = "Locale file " + _fileName + " is empty.";
                if (_writeErrorToConsole)
                    Console.WriteLine("Locale file is empty.");

                return new LocaleFile() { Error = msg };
            }

            string localeFileHeader = ReadLocaleFileJsHeader(lines, out int contentStartsPos);

            bool isInsideCommentBlock = false;

            Dictionary<string, LocaleEntry> entries = new(lines.Length);
            List<LocaleEntry> invalidEntries = new(lines.Length);

            for (int i = contentStartsPos; i < lines.Length; ++i)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("*/"))
                {
                    isInsideCommentBlock = false;
                    continue;
                }

                if (isInsideCommentBlock)
                    continue;

                if (line.Length == 0 || line.StartsWith("//") || line.StartsWith("window.") || line.StartsWith('}'))
                    continue;

                if (line.StartsWith("/*"))
                {
                    isInsideCommentBlock = true;
                    continue;
                }

                int lineNumber = i - contentStartsPos + 1;
                LocaleEntry entry = new LocaleEntryJs(line, lineNumber);

                if (entry.IsValid)
                    entries[entry.Key] = entry;
                else
                {
                    if (!entry.RawString.StartsWith("get"))
                        invalidEntries.Add(entry);
                }
            }

            return new LocaleFile()
            {
                Header = localeFileHeader,
                Entries = entries,
                InvalidEntries = invalidEntries
            };
        }

        private static string ReadLocaleFileIniHeader(string[] _lines, out int _contentStartPos)
        {
            _contentStartPos = 0;

            string header = "";

            for (int i = 0; i < _lines.Length; ++i)
            {
                if (string.IsNullOrWhiteSpace(_lines[i]) || !_lines[i].StartsWith(';'))
                {
                    _contentStartPos = i;
                    break;
                }

                header += _lines[i] + "\r\n";
            }

            return header;
        }

        private static string ReadLocaleFileJsHeader(string[] _lines, out int _contentStartPos)
        {
            _contentStartPos = 0;
            bool headerStarted = false;

            string header = "";

            for (int i = 0; i < _lines.Length; ++i)
            {
                if (_lines[i].Trim().StartsWith("/*"))
                {
                    headerStarted = true;
                }
                else if (_lines[i].Trim().StartsWith("*/"))
                {
                    if (!headerStarted)
                        Console.WriteLine("Header is not started, \"/*\" missing.");

                    header += _lines[i] + "\r\n";
                    _contentStartPos = i;

                    return header;
                }

                header += _lines[i] + "\r\n";
            }

            return header;
        }
    }

}

/*  Helpers/LocaleFileUtils.cs
 *  Version: v1.0 (2023.10.13)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AppHelper.Helpers
{
    public static class LocaleFileUtils
    {
        public  class LocaleFile
        {
            //public string LocaleName { get; set; }
            public string Error { get; set; } = null;

            public string Header { get; set; } = null;
            public Dictionary<string, string> Entries { get; set; } = null;
        }


        public static LocaleFile ReadLocaleFile(string _fullname, string _locale, bool _readHeader = true, bool _readFullEntries = true, bool _writeErrorToConsole = true)
        {
            string[] lines;
            try
            {
                lines = File.ReadAllLines(_fullname, Encoding.UTF8);
            }
            catch (Exception _ex)
            {
                string msg = "ReadLocaleFile: Exception during reading locale file " + _locale + ":\n" + _ex;
                if (_writeErrorToConsole)
                    Console.WriteLine(msg);

                return new LocaleFile() { Error = msg };
            }

            if (lines.Length == 0)
            {
                string msg = "Locale file " + _locale + " is empty.";
                if (_writeErrorToConsole)
                    Console.WriteLine("Locale file is empty.");

                return new LocaleFile() { Error = msg };
            }

            string localeFileHeader = null;
            int contentStarts = 0;
            if (_readHeader)
            {
                int startPos = _fullname.IndexOf("wwwroot");
                int endPos = _fullname.LastIndexOfAny(new[] { '/', '\\' });
                string localePath = _fullname[(startPos + 8)..(endPos + 1)].Replace('\\', '/');

                localeFileHeader = ReadLocaleFileHeader(lines, localePath, out contentStarts);
            }

            Dictionary<string, string> entries = new();
            for (int i = contentStarts; i < lines.Length; ++i)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith(';'))
                    continue;

                string key = null;
                string value = null;

                // separate key/value;
                int pos = line.IndexOf('=');
                if (pos <= 0)
                    key = line.Trim();
                else
                {
                    key = line[0..pos].Trim();
                    value = line[(pos + 1)..].Trim();
                }

                // check for comment character;
                pos = key.IndexOf(';');
                if (pos > 0)
                {
                    // this line is malformed;
                    key = "<>" + i;
                    value = lines[i];
                }

                if (!_readFullEntries)
                {
                    pos = value.IndexOf(';');
                    if (pos > 0)
                        value = value[0..pos];
                }

                entries[key] = value;
            }

            return new LocaleFile()
            {
                Header = localeFileHeader,
                Entries = entries,
            };
        }

        private static string ReadLocaleFileHeader(string[] _lines, string _localePath, out int _contentStart)
        {
            _contentStart = _lines.Length;

#if true
            string header = "";

            for(int i = 0; i < _lines.Length; ++i)
            {
                if (string.IsNullOrWhiteSpace(_lines[i]) || !_lines[i].StartsWith(';'))
                {
                    _contentStart = i;
                    break;
                }

                header += _lines[i] + "\r\n";
            }

            return header;

#else
            string versionLine = _lines[1];

            string header = ";   " + _localePath + "vi-VN.ini\r\n;   Version: v";

            Match matches = Regex.Match(versionLine, @"Version: v(\d+).(\d+)");
            if (!matches.Success || matches.Groups.Count != 3)
            {
                header += "1.0";
            }
            else
            {
                if (!int.TryParse(matches.Groups[2].Value, out int minorVer))
                    minorVer = 0;

                header += matches.Groups[1].Value + "." + (minorVer + 1);
            }

            header += " (" + DateTime.Now.ToString("yyyy.MM.dd") + ")\r\n;\r\n;   Author\r\n;       Arime-chan\r\n;";

            return header;
#endif
        }
    }

}

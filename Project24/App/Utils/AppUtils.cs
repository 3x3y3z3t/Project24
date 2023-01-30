/*  AppUtils.cs
 *  Version: 1.6 (2023.01.27)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Project24.App.Utils;

namespace Project24.App
{
    public struct VersionInfo
    {
        public int Major;
        public int Minor;
        public int Build;
        public int Revision;

        public override string ToString()
        {
            return Major + "." + Minor + "." + Build + "." + Revision;
        }

        public int CompareTo(VersionInfo _other)
        {
            if (Major > _other.Major)
                return 1;
            if (Major < _other.Major)
                return -1;

            // Major equals;
            if (Minor > _other.Minor)
                return 1;
            if (Minor < _other.Minor)
                return -1;

            // Major, Minor equals;
            if (Build > _other.Build)
                return 1;
            if (Build < _other.Build)
                return -1;

            // Major, Minor, Build equals;
            if (Revision > _other.Revision)
                return 1;
            if (Revision < _other.Revision)
                return -1;

            // Major, Minor, Build, Revision equals;
            return 0;
        }

        public int CompareTo(Version _other)
        {
            return CompareTo(new VersionInfo()
            {
                Major = _other.Major,
                Minor = _other.Minor,
                Build = _other.Build,
                Revision = _other.Revision
            });
        }
    }

    public static class BackingObject
    {
        public static bool IsP24StorageValidationInProgress { get; set; } = false;
    }

    public static class AppUtils
    {
        public static int ProcessId { get; private set; } = Process.GetCurrentProcess().Id;
        public static string AppRoot { get; private set; } = System.IO.Directory.GetCurrentDirectory();
        public static string Today { get { return DateTime.Now.ToString("yyyy-MM-dd"); } }

        public static string CurrentSessionName { get; private set; } = string.Format("{0:yyyy}-{0:MM}-{0:dd}_{0:HH}-{0:mm}-{0:ss}", DateTime.Now);

        public static string CurrentVersion { get; set; }

        public static UpdaterStats UpdaterStats { get; set; } = new UpdaterStats();

        public static JavaScriptEncoder FullUnicodeRangeJsonEncoder { get; set; } = JavaScriptEncoder.Create(UnicodeRanges.All);

        private static char[] s_InvalidFilenameChars = new char[]
        {
            '\u0000', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007', '\u0008', '\u0009', '\u000a', '\u000b', '\u000c', '\u000d', '\u000e', '\u000f',
            '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017', '\u0018', '\u0019', '\u001a', '\u001b', '\u001c', '\u001d', '\u001e', '\u001f',
            '\\', '/', ':', '*', '?', '\"', '<', '>', '|'
        };

        private static string[] s_InvalidFileNameString = new string[]
        {
            "CON", "PRN", "AUX", "NUL",
            "COM0", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT0", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };


        public static bool IsFileNameValid(string _filename, bool _isFolder = false)
        {
            /* Reference:
             * https://learn.microsoft.com/en-us/windows/win32/fileio/naming-a-file#naming-conventions
             */

            // file name cannot be all white spaces;
            if (string.IsNullOrWhiteSpace(_filename))
                return false;

            // file name cannot be "." (Current Directory) or ".." (Parent Directory);
            if (_filename == "." || _filename == "..")
                return false;

            // file name cannot end with white space or period character;
            if (_filename.EndsWith(' ') || _filename.EndsWith('.'))
                return false;

            // file name cannot contains any of invalid characters;
            if (_filename.IndexOfAny(s_InvalidFilenameChars) >= 0)
                return false;

            // file name cannot be any of reserved names;
            foreach (string name in s_InvalidFileNameString)
            {
                if (_filename.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return false;

                if (!_isFolder && _filename.StartsWith(name + ".", StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        public static string FormatDataSize(long _size)
        {
            const long oneKiB = 1024L;
            const long oneMiB = 1024L * oneKiB;
            const long oneGiB = 1024L * oneMiB;
            const long oneTiB = 1024L * oneGiB;

            if (_size >= oneTiB)
            {
                return string.Format("{0:##0.00} TB", (float)_size / oneTiB);
            }

            if (_size >= oneGiB)
            {
                return string.Format("{0:##0.00} GB", (float)_size / oneGiB);
            }

            if (_size >= oneMiB)
            {
                return string.Format("{0:##0.00} MB", (float)_size / oneMiB);
            }

            if (_size >= oneKiB)
            {
                return string.Format("{0:##0.00} KB", (float)_size / oneKiB);
            }

            return string.Format("{0:##0.00}  B", (float)_size);
        }

        public static string FormatDateTimeString_EndsWithMinute(DateTime _dateTime)
        {
            return string.Format("{0:yyyy}/{0:MM}/{0:dd} {0:HH}:{0:mm}", _dateTime);
        }

        public static HtmlString FormatHtmlDisplayForMultilineText(string _multilineText)
        {
            if (_multilineText == null)
                return new HtmlString("");

            string res = _multilineText.Replace("\\r", "\r").Replace("\\n", "\n");
            res = _multilineText.Replace("\r\n", "</div><div class=\"rev-indent mt-1\">");
            res = "<div class=\"rev-indent\">" + res + "</div>";

            return new HtmlString(res);
        }

        public static string NormalizeGenderString(char _gender)
        {
            switch (_gender)
            {
                case 'M':
                    return P24Constants.GenderMale;
                case 'm':
                    return P24Constants.GenderMale;

                case 'F':
                    return P24Constants.GenderFemale;
                case 'f':
                    return P24Constants.GenderFemale;
            }

            return "?";
        }

        public static async Task UpdateCurrentVersion()
        {
            string webRootPath = AppRoot + "/" + "wwwroot";

            string markdown = await System.IO.File.ReadAllTextAsync(webRootPath + "/ReleaseNote.md", Encoding.UTF8);

            Regex regex = new Regex(@"#+ v[0-9]+\.[0-9]+\.[0-9]+-*([a-z0-9])*");

            var match = regex.Match(markdown);
            if (match.Success)
            {
                string v = match.Value[5..];
                CurrentVersion = v; // equivalent to .Substring(4);
            }
            else
            {
                CurrentVersion = "Unknown";
            }
        }

        public static string ComputeCyrb53HashCode(string _string, int _seed = 0)
        {
            /*
                cyrb53 (c) 2018 bryc (github.com/bryc)
                A fast and simple hash function with decent collision resistance.
                Largely inspired by MurmurHash2/3, but with a focus on speed/simplicity.
                Public domain. Attribution appreciated.
            */

            int h1 = (int)(0xdeadbeef ^ _seed);
            int h2 = (int)0x41c6ce57 ^ _seed;
            
            for (int i = 0; i < _string.Length; i++)
            {
                int ch = _string[i];
                h1 = (int)((h1 ^ ch) * 2654435761);
                h2 = (int)((h2 ^ ch) * 1597334677L);
            }

            h1 = (int)((h1 ^ (int)((uint)h1 >> 16)) * 2246822507U) ^ (int)((h2 ^ (int)((uint)h2 >> 13)) * 3266489909U);
            h2 = (int)((h2 ^ (int)((uint)h2 >> 16)) * 2246822507U) ^ (int)((h1 ^ (int)((uint)h1 >> 13)) * 3266489909U);

            return "" + (4294967296 * (2097151 & h2) + (uint)h1);
        }

    }

}

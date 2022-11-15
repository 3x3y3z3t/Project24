/*  AppUtils.cs
 *  Version: 1.0 (2022.11.13)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

    public class AppUtils
    {
        public static string AppRoot { get; private set; } = System.IO.Directory.GetCurrentDirectory();

        public static string CurrentSessionName { get; private set; } = string.Format("{0:yyyy}-{0:MM}-{0:dd}_{0:HH}-{0:mm}-{0:ss}", DateTime.Now);


        public static string CurrentVersion { get; set; }

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

            return string.Format("{0:##0.00}   B", (float)_size);
        }

        public static string FormatDateTimetring_EndsWithMinute(DateTime _dateTime)
        {
            return string.Format("{0:yyyy}/{0:MM}/{0:dd} {0:HH}:{0:mm}", _dateTime);
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

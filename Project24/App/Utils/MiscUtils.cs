/*  App/Utils/MiscUtils.cs
 *  Version: v1.1 (2023.06.28)
 *  
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Project24.App.BackendData;

namespace Project24.App
{
    public static class Utils
    {
        public static string WebRootPath = null;

        public static readonly JavaScriptEncoder FullUnicodeRangeJsonEncoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        public static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            Encoder = FullUnicodeRangeJsonEncoder
        };

        public static bool IsFlagSet(int _flags, int _flag) => (_flags & _flag) != 0;

        #region DateTime
        public static string FormatDateTimeString_EndsWithMinute(DateTime _dateTime) => string.Format("{0:yyyy}/{0:MM}/{0:dd} {0:HH}:{0:mm}", _dateTime);
        public static string FormatDateTimeString_EndsWithSecond(DateTime _dateTime) => string.Format("{0:yyyy}/{0:MM}/{0:dd} {0:HH}:{0:mm}:{0:ss}", _dateTime);
        #endregion


        #region Version Info
        public static int CompareVersion(VersionInfo _ver0,VersionInfo _ver1)
        {
            if (_ver0.Major > _ver1.Major)
                return 1;
            if (_ver0.Major < _ver1.Major)
                return -1;

            if (_ver0.Minor> _ver1.Minor)
                return 1;
            if (_ver0.Minor < _ver1.Minor)
                return -1;

            if (_ver0.Build > _ver1.Build)
                return 1;
            if (_ver0.Build < _ver1.Build)
                return -1;

            if (_ver0.Revision > _ver1.Revision)
                return 1;
            if (_ver0.Revision < _ver1.Revision)
                return -1;

            return 0;
        }

        public static string ConstructVersionInfoString(VersionInfo _versionInfo) => (_versionInfo.Major + "." + _versionInfo.Minor + "." + _versionInfo.Build + "." + _versionInfo.Revision);

        public static VersionInfo ParseVersionInfo(string _verInfoString)
        {
            string[] arr = _verInfoString.Split('.');
            if (arr.Length < 4)
                return null;

            if (!int.TryParse(arr[0], out int major))
                return null;
            if (!int.TryParse(arr[1], out int minor))
                return null;
            if (!int.TryParse(arr[2], out int build))
                return null;
            if (!int.TryParse(arr[3], out int revision))
                return null;

            return new VersionInfo()
            {
                Major = major,
                Minor = minor,
                Build = build,
                Revision = revision
            };
        }
        #endregion


        // https://github.com/bryc/code/blob/master/jshash/experimental/cyrb53.js
        public static long ComputeCyrb53HashCode(string _string, int _seed = 0)
        {
            /*
                cyrb53 (c) 2018 bryc (github.com/bryc)
                A fast and simple hash function with decent collision resistance.
                Largely inspired by MurmurHash2/3, but with a focus on speed/simplicity.
                Public domain. Attribution appreciated.
            */

            int h1 = (int)(0xdeadbeef ^ _seed);
            int h2 = (int)(0x41c6ce57 ^ _seed);

            for (int i = 0; i < _string.Length; i++)
            {
                int ch = _string[i];
                h1 = (int)((h1 ^ ch) * 2654435761);
                h2 = (int)((h2 ^ ch) * 1597334677L);
            }

            h1 = (int)((h1 ^ (int)((uint)h1 >> 16)) * 2246822507U) ^ (int)((h2 ^ (int)((uint)h2 >> 13)) * 3266489909U);
            h2 = (int)((h2 ^ (int)((uint)h2 >> 16)) * 2246822507U) ^ (int)((h1 ^ (int)((uint)h1 >> 13)) * 3266489909U);

            return 4294967296 * (2097151 & h2) + (uint)h1;
        }

    }

}

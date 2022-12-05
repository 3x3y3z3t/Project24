/*  StringUtils.cs
 *  Version: 1.0 (2022.12.04)
 *
 *  Contributor
 *      Arime-chan
 */

using System.Globalization;

namespace Project24.App.Utils
{
    public static class StringUtils
    {
        public static readonly CultureInfo CultureInfo;

        static StringUtils()
        {
            CultureInfo = CultureInfo.CreateSpecificCulture("vi-VN");
        }

        public static string ToTitleCase(string _original)
        {
            return CultureInfo.TextInfo.ToTitleCase(_original);
        }

    }
}

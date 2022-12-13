/*  P24Utils.cs
 *  Version: 1.1 (2022.12.13)
 *
 *  Contributor
 *      Arime-chan
 */

using System;

namespace Project24.Utils.ClinicManager
{
    public static class P24Utils
    {
        public static Tuple<string, string> SplitFirstLastName(string _fullName)
        {
            if (string.IsNullOrEmpty(_fullName))
                return null;

            string[] tokens = _fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 1)
                return new Tuple<string, string>(null, tokens[0].Trim());

            string middlename = "";
            for (int i = 0; i < tokens.Length - 1; ++i)
            {
                middlename += tokens[i].Trim() + " ";
            }

            return new Tuple<string, string>(middlename.Trim(), tokens[^1].Trim());
        }
    }

}

/*  App/Utils/P24DesignMap.cs
 *  Version: v1.0 (2023.10.30)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.Collections.Generic;

namespace Project24.App.Utils
{
    public partial class P24DesignMap
    {
        public class ProgressInfo : Tuple<int, int>
        {
            public float Percent { get { if (Item2 == 0) return 0; return Item1 * 100.0f / Item2; } }
            public float Ratio { get { if (Item2 == 0) return 0; return (float)Item1 / Item2; } }


            public ProgressInfo(int _item1, int _item2)
                : base(_item1, _item2)
            { }
        }

        public static ProgressInfo GetProgressForCMModule() => GetProgressForModule("CM_");
        public static ProgressInfo GetProgressForNasModule() => GetProgressForModule("Nas_");
        public static ProgressInfo GetProgressForHomeModule() => GetProgressForModule("Home_");

        private static ProgressInfo GetProgressForModule(string _modulePrefix)
        {
            if (string.IsNullOrWhiteSpace(_modulePrefix))
                return new(0, 1);

            if (m_PageProgresses == null || m_PageProgresses.Count <= 0)
                return new(0, 1);

            int accumulate = 0;
            int count = 0;
            foreach (var pair in m_PageProgresses)
            {
                if (pair.Key.StartsWith(_modulePrefix))
                {
                    accumulate += pair.Value.Item1;
                    count += pair.Value.Item2;
                }
            }

            return new(accumulate, count);
        }


        private static readonly Dictionary<string, ProgressInfo> m_PageProgresses = new();
    }

}

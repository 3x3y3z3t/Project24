/*  App/Utils/P24Stopwatch.cs
 *  Version: v1.0 (2023.10.02)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Project24.App.Utils
{
    public class P24Stopwatch : Stopwatch
    {
        public string GetLapsesAsLogString()
        {
            int remainder = 4 - (m_LongestLapName % 4);
            int alignLength = m_LongestLapName + remainder;

            string s = "";
            foreach (var lap in m_Lapses)
            {
                //s += string.Format("{0,-" + alignLength + "}: {1,9:0.000} ms\n", lap.Item1, lap.Item2.TotalMilliseconds);
                s += string.Format("{0,10:0.000} ms : {1}\n", lap.Item2.TotalMilliseconds, lap.Item1);
            }

            return s;
        }

        public TimeSpan Lap()
        {
            TimeSpan lap = Elapsed - m_LastLap;
            m_LastLap = Elapsed;

            return lap;
        }

        public void Lap(string _lapName)
        {
            TimeSpan lap = Elapsed - m_LastLap;
            m_LastLap = Elapsed;

            if (m_LongestLapName < _lapName.Length)
                m_LongestLapName = (ushort)_lapName.Length;

            m_Lapses.Add(new Tuple<string, TimeSpan>(_lapName, lap));
        }


        public static new P24Stopwatch StartNew()
        {
            P24Stopwatch sw = new();
            sw.Start();
            return sw;
        }


        private TimeSpan m_LastLap = TimeSpan.Zero;
        private ushort m_LongestLapName = 0;
        private List<Tuple<string, TimeSpan>> m_Lapses = new();
    }

}

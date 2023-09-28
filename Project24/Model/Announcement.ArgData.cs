/*  Model/Announcement.ArgData.cs
 *  Version: v1.0 (2023.09.28)
 *  
 *  Author
 *      Arime-chan
 */

using System;

namespace Project24.Model
{
    internal class AnnouncementArgDataJson
    {
        public string Type { get; set; } = null;
        public string Value { get; set; } = null;
        public string CountsToward { get; set; } = null;
        public bool ShouldUpdate { get; set; } = false;
        public short CountsPerMillis { get; set; } = 0;
        public bool InfiniteCount { get; set; } = false;

        public AnnouncementArgDataJson()
        { }
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class AnnouncementArgData
    {
        public enum CountDirection : sbyte
        {
            Down = -1,
            Up = 1
        }


        public object Type => GetTypeNameStatic();
        public object Value => GetValue();
        public object CountToward => GetCountToward();

        public bool ShouldUpdate => m_ShouldUpdate;
        public short CountsPerMillis => m_CountsPerMillis;
        public bool InfiniteCount => m_InfiniteCount;


        //protected AnnouncementArgsData(short _countPerMillisecond = 0, bool _infiniteCount = false, bool _isComplexObject = false, bool _shouldUpdate = false)
        protected AnnouncementArgData(short _countsPerMillis = 0, bool _infiniteCount = false, bool _shouldUpdate = false)
        {
            //m_IsComplexObject = _isComplexObject;
            m_ShouldUpdate = _shouldUpdate;

            m_CountsPerMillis = _countsPerMillis;
            m_InfiniteCount = _infiniteCount;
        }


        public abstract void Update(TimeSpan _deltaTime);
        //public abstract string ToDbString();


        protected abstract bool IsDoneUpdating();
        protected abstract string GetTypeNameStatic();
        protected abstract object GetValue();
        protected abstract object GetCountToward();


        //protected readonly bool m_IsComplexObject = false;
        protected readonly bool m_ShouldUpdate = false;

        protected readonly short m_CountsPerMillis = 0;
        protected readonly bool m_InfiniteCount = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AnnouncementArgData<T> : AnnouncementArgData where T : IComparable
    {
        //protected AnnouncementArgsData(T _value, T _countToward = default, short _countPerMillisecond = 0, bool _infiniteCount = false, bool _isComplexObject = false, bool _shouldUpdate = false)
        protected AnnouncementArgData(T _value, T _countToward = default, short _countPerMillisecond = 0, bool _infiniteCount = false, bool _shouldUpdate = false)
            //: base(_countPerMillisecond, _infiniteCount, _isComplexObject, _shouldUpdate)
            : base(_countPerMillisecond, _infiniteCount, _shouldUpdate)
        {
            m_Value = _value;
            m_CountToward = _countToward;
        }


        public override void Update(TimeSpan _deltaTime)
        {
            if (IsDoneUpdating())
                return;

            UpdateInternal(_deltaTime);

            if (m_InfiniteCount)
                return;

            if (IsValueSurpassedThreshold())
                m_Value = m_CountToward;
        }


        protected override bool IsDoneUpdating()
        {
            if (!m_ShouldUpdate)
                return true;

            if (m_InfiniteCount)
                return false;

            return IsValueSurpassedThreshold();
        }

        protected override object GetValue() => m_Value;

        protected override object GetCountToward() => m_CountToward;

        protected abstract void UpdateInternal(TimeSpan _deltaTime);


        private bool IsValueSurpassedThreshold()
        {
            int compareResult = m_Value.CompareTo(m_CountToward);
            if (m_CountsPerMillis > 0 && compareResult >= 0)
                return true;
            else if (m_CountsPerMillis < 0 && compareResult < 0)
                return true;

            return false;
        }


        protected T m_Value = default;
        protected readonly T m_CountToward = default;
    }

    /// <summary>
    /// 
    /// </summary>
    public class AnnouncementArgDataString : AnnouncementArgData<string>
    {
        public AnnouncementArgDataString(string _value)
            : base(_value)
        { }


        protected override void UpdateInternal(TimeSpan _deltaTime)
        { }

        protected override string GetTypeNameStatic() => nameof(AnnouncementArgDataString);
    }

    /// <summary>
    /// 
    /// </summary>
    public class AnnouncementArgDataDateTime : AnnouncementArgData<DateTime>
    {
        public AnnouncementArgDataDateTime(DateTime _value, DateTime _countToward = default, CountDirection _countDirection = CountDirection.Up, bool _infiniteCount = false, bool _shouldUpdate = false)
            : base(_value, _countToward, (short)_countDirection, _infiniteCount, _shouldUpdate)
        { }


        protected override void UpdateInternal(TimeSpan _deltaTime)
        {
            double millis = m_CountsPerMillis * _deltaTime.TotalMilliseconds;
            m_Value = m_Value.AddMilliseconds(millis);
        }

        protected override string GetTypeNameStatic() => nameof(AnnouncementArgDataDateTime);
    }

    /// <summary>
    /// 
    /// </summary>
    public class AnnouncementArgDataTimeSpan : AnnouncementArgData<TimeSpan>
    {
        public AnnouncementArgDataTimeSpan(TimeSpan _value, TimeSpan _countToward = default, CountDirection _countDirection = CountDirection.Up, bool _infiniteCount = false, bool _shouldUpdate = false)
            : base(_value, _countToward, (short)_countDirection, _infiniteCount, _shouldUpdate)
        { }


        protected override void UpdateInternal(TimeSpan _deltaTime)
        {
            if (m_CountsPerMillis == 0)
                return;

            if (m_CountsPerMillis > 0)
                m_Value = m_Value.Add(_deltaTime);
            else
                m_Value = m_Value.Subtract(_deltaTime);
        }

        protected override string GetTypeNameStatic() => nameof(AnnouncementArgDataTimeSpan);
    }

}

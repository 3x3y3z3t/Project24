/*  Model/Announcement.cs
 *  Version: v1.0 (2023.09.18)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using Project24.App.Services;
using Project24.App.Services.ServerAnnouncement;

namespace Project24.Model
{
    public class Announcement
    {
        public static class ExpirationRule_
        {
            public const string NO_EXPIRE = nameof(NO_EXPIRE);

            public const string BY_DATE = nameof(BY_DATE);

            //public const string WHEN_EITHER_ARG_DONE_COUNTING = nameof(WHEN_EITHER_ARG_DONE_COUNTING);
            //public const string WHEN_ALL_ARGS_DONE_COUNTING = nameof(WHEN_ALL_ARGS_DONE_COUNTING);

            //public const string BY_DATE_OR_EITHER_ARG_DONE_COUNTING = nameof(BY_DATE_OR_EITHER_ARG_DONE_COUNTING);
            //public const string BY_DATE_OR_ALL_ARGS_DONE_COUNTING = nameof(BY_DATE_OR_ALL_ARGS_DONE_COUNTING);
        }

        public static class Severity_
        {
            public const string PRIMARY = "primary";
            public const string SECONDARY = "secondary";
            public const string SUCCESS = "success";
            public const string WARNING = "warning";
            public const string DANGER = "danger";
            public const string INFO = "info";
        }


        [Key]
        public long Id { get; protected set; }
        [StringLength(100)]
        public string Tag { get; set; }

        public DateTime AddedDate { get; protected set; } = DateTime.Now;
        public DateTime ExpireDate { get; protected set; } = DateTime.MaxValue;
        [StringLength(10)]
        public string Severity { get; protected set; } = Severity_.INFO;
        [StringLength(100)]
        public string ExpirationRule { get; protected set; } = ExpirationRule_.NO_EXPIRE;

        [JsonIgnore]
        public string RawFormatString { get; protected set; } = null;
        [JsonIgnore]
        public string ArgumentsAsJsonString { get; protected set; } = null;

        [NotMapped]
        public string FormatString { get; protected set; } = null;
        [NotMapped]
        public List<AnnouncementArgData> Arguments { get; protected set; } = new();
        [NotMapped]
        public bool IsExpired => IsExpiredInternal();


        public Announcement()
        { }

        public Announcement(string _messageFormatString, AnnouncementArgData _arg = null, string _severity = Severity_.INFO, string _expirationRule = ExpirationRule_.NO_EXPIRE, DateTime _expireDate = default)
            : this(_messageFormatString, new AnnouncementArgData[] { _arg }, _severity, _expirationRule, _expireDate)
        { }

        public Announcement(string _messageFormatString, IList<AnnouncementArgData> _args = null, string _severity = Severity_.INFO, string _expirationRule = ExpirationRule_.NO_EXPIRE, DateTime _expireDate = default)
        {
            RawFormatString = _messageFormatString;
            ExpirationRule = _expirationRule;
            Severity = _severity;

            if (_args != null && _args.Count > 0)
                Arguments.AddRange(_args);

            if (_expireDate != default)
                ExpireDate = _expireDate;

            if (ArgumentsAsJsonString == null)
            {
                ArgumentsAsJsonString = JsonSerializer.Serialize(Arguments);
            }
            else
            {
                DeserializeArgsDbString();
            }
        }


        public void DeserializeArgsDbString()
        {
            AnnouncementArgDataJson[] args = JsonSerializer.Deserialize<AnnouncementArgDataJson[]>(ArgumentsAsJsonString);
            if (args == null || args.Length <= 0)
                return;

            foreach (AnnouncementArgDataJson arg in args)
            {
                switch (arg.Type)
                {
                    case nameof(AnnouncementArgDataString):
                        Arguments.Add(ConstructArgDataString(arg));
                        break;
                    case nameof(AnnouncementArgDataDateTime):
                        Arguments.Add(ConstructArgDataDateTime(arg));
                        break;
                    case nameof(AnnouncementArgDataTimeSpan):
                        Arguments.Add(ConstructArgDataTimeSpan(arg));
                        break;


                    default:
                        continue;
                }


                // TODO: initialize this arg object;



            }
        }

        /// <summary>
        /// Resolves this Argument's format string and writes it to FormatString.<br />
        /// If the format string is a placeholder string (<c>{FORMAT_STRING}</c>), it will be replaced with the localized string.
        /// If the format string is a regular string, it will be kept as-is 
        /// </summary>
        public void ResolveFormatString(LocalizationSvc _localization)
        {
            if (RawFormatString.StartsWith('{') && RawFormatString.EndsWith('}'))
            {
                string key = RawFormatString.TrimStart('{').TrimEnd('}');
                if (_localization.ContainsKey(key))
                {
                    FormatString = _localization[key];
                    return;
                }
            }

            FormatString = RawFormatString;
        }

        /// <summary> Forces this Announcement to expire. </summary>
        public void ForceExpire()
        {
            /*  Currently we force expiring this annoucement by setting expire date to the past
             *  AND setting expiration rule to "by date".
             */
            ExpireDate = DateTime.MinValue;
            ExpirationRule = ExpirationRule_.BY_DATE;
        }

        public void Update(TimeSpan _deltaTime)
        {
            foreach (var arg in Arguments)
            {
                if (arg.ShouldUpdate)
                    arg.Update(_deltaTime);
            }
        }


        private bool IsExpiredInternal()
        {
            switch (ExpirationRule)
            {
                case ExpirationRule_.NO_EXPIRE:
                    return false;

                case ExpirationRule_.BY_DATE:
                    return DateTime.Now > ExpireDate;
            }

            return false;
        }


        #region AnnouncementArgData Factory
        private static AnnouncementArgDataString ConstructArgDataString(AnnouncementArgDataJson _json)
        {
            return new(_json.Value);
        }

        private static AnnouncementArgDataDateTime ConstructArgDataDateTime(AnnouncementArgDataJson _json)
        {
            DateTime value = DateTime.Parse(_json.Value);
            _ = DateTime.TryParse(_json.CountsToward, out DateTime countsToward); // DateTime.TryParse does initialize object with default;

            AnnouncementArgData.CountDirection countDirection = AnnouncementArgData.CountDirection.Up;
            if (_json.CountsPerMillis < 0)
                countDirection = AnnouncementArgData.CountDirection.Down;

            return new(value, countsToward, countDirection, _json.InfiniteCount, _json.ShouldUpdate);
        }

        private static AnnouncementArgDataTimeSpan ConstructArgDataTimeSpan(AnnouncementArgDataJson _json)
        {
            TimeSpan value = TimeSpan.Parse(_json.Value);
            _ = TimeSpan.TryParse(_json.CountsToward, out TimeSpan countsToward);

            AnnouncementArgData.CountDirection countDirection = AnnouncementArgData.CountDirection.Up;
            if (_json.CountsPerMillis < 0)
                countDirection = AnnouncementArgData.CountDirection.Down;

            return new(value, countsToward, countDirection, _json.InfiniteCount, _json.ShouldUpdate);
        }



        #endregion
    }

}

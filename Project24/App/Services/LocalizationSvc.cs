/*  App/Services/LocalizationSvc.cs
 *  Version: v1.2 (2023.09.15)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Project24.App.Services
{
    public class LocalizationSvc : IProject24HostedService
    {
        public static class SupportedLocale
        {
            public const string EN_US = "en-US";
            public const string JA_JP = "ja-JP";
            public const string VI_VN = "vi-VN";
        }
 

        public string this[string _key]
        {
            get
            {
                if (!ContainsKey(_key))
                    return "<code>" + _key + "</code>";

                return m_Locales[CurrentLocale][_key];
            }
        }

        public string CurrentLocale { get => m_TrackerSvc[InternalTrackedKeys.CONFIG_GLOBAL_LOCALIZATION]; }


        public LocalizationSvc(InternalTrackerSvc _trackerSvc, ILogger<LocalizationSvc> _logger)
        {
            m_TrackerSvc = _trackerSvc;
            m_Logger = _logger;

            m_Locales = new();

        }


        public async Task StartAsync(CancellationToken _cancellationToken = default)
        {
            m_Locales[SupportedLocale.EN_US] = await LoadLocalizedStringAsync(SupportedLocale.EN_US, _cancellationToken);
            m_Locales[SupportedLocale.JA_JP] = await LoadLocalizedStringAsync(SupportedLocale.JA_JP, _cancellationToken);
            m_Locales[SupportedLocale.VI_VN] = await LoadLocalizedStringAsync(SupportedLocale.VI_VN, _cancellationToken);

        }

        public bool ContainsKey(string _key) => m_Locales[CurrentLocale].ContainsKey(_key);


        private async Task<Dictionary<string, string>> LoadLocalizedStringAsync(string _locale, CancellationToken _cancellationToken = default)
        {
            if (string.IsNullOrEmpty(_locale))
                return new();

            Dictionary<string, string> dict = new();

            string localeFileName = _locale + ".ini";
            string path = Path.GetFullPath(Program.WorkingDir + "/wwwroot/res/locale/" + localeFileName);
            if (!File.Exists(path))
            {
                m_Logger.LogWarning("Locale file {_localeFileName} does not exist.", localeFileName);
                return dict;
            }

            // ==================== read ====================;
            string[] lines;
            try
            {
                lines = await File.ReadAllLinesAsync(path);
            }
            catch (Exception _ex)
            {
                m_Logger.LogWarning("Exception during loading locale {_locale}: {_ex}", _locale, _ex);
                return dict;
            }

            if (lines == null || lines.Length == 0)
            {
                m_Logger.LogWarning("Locale file {_localeFileName} is empty.", localeFileName);
                return dict;
            }

            // ==================== parse ====================;
            for ( int i = 0; i < lines.Length; ++i)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith(';'))
                    continue;

                var pair = ParseSingleLine(_locale, line, i);
                if (pair == null)
                    continue;

                dict[pair.Value.Key] = pair.Value.Value;
            }

            if (dict.Count <= 0)
            {
                m_Logger.LogWarning("Locale file {_localeFileName} is empty.", localeFileName);
                return dict;
            }

            return dict;
        }

        private KeyValuePair<string, string>? ParseSingleLine(string _locale, string _line, int _index)
        {
            int pos = _line.IndexOf('=');
            if (pos < 0)
            {
                m_Logger.LogWarning("[{_locale}]: Malformed record: ({_line})[{_lineText}]", _locale, _index + 1, _line);
                return null;
            }

            string key = _line[..pos].Trim();
            string value = _line[(pos + 1)..].Trim();

            if (key == "" || value == "")
            {
                m_Logger.LogWarning("[{_locale}]: Malformed record: ({_line})[{_lineText}]", _locale, _index + 1, _line);
                return null;
            }

            return new(key, value);
        }


        private readonly Dictionary<string, Dictionary<string, string>> m_Locales;

        private readonly InternalTrackerSvc m_TrackerSvc;
        private readonly ILogger<LocalizationSvc> m_Logger;
    }

}

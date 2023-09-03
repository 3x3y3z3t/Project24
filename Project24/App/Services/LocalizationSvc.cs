/*  App/Services/LocalizationSvc.cs
 *  Version: v1.0 (2023.09.02)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.IO;
using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Logging;

namespace Project24.App.Services
{
    public class LocalizationSvc
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
                if (!m_Locales[CurrentLocale].ContainsKey(_key))
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


        public void StartService()
        {
            LoadEN_US();
            LoadJA_JP();
            LoadVI_VN();

            // TODO: load from file;

        }


        private void LoadEN_US() => m_Locales[SupportedLocale.EN_US] = LoadLocalizedString(SupportedLocale.EN_US);


        private void LoadJA_JP() => m_Locales[SupportedLocale.JA_JP] = LoadLocalizedString(SupportedLocale.JA_JP);


        private void LoadVI_VN() => m_Locales[SupportedLocale.VI_VN] = LoadLocalizedString(SupportedLocale.VI_VN);

        private Dictionary<string, string> LoadLocalizedString(string _locale)
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
                lines = File.ReadAllLines(path);
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

                int pos = line.IndexOf('=');
                if (pos < 0)
                {
                    m_Logger.LogWarning("[{_locale}]: Malformed record: ({_line})[{_lineText}]", _locale, i + 1, line);
                    continue;
                }

                string key = line[..pos].Trim();
                string value = line[(pos + 1)..].Trim();

                if (key == "" || value == "")
                {
                    m_Logger.LogWarning("[{_locale}]: Malformed record: ({_line})[{_lineText}]", _locale, i + 1, line);
                    continue;
                }

                dict[key] = value;
            }

            if (dict.Count <= 0)
            {
                m_Logger.LogWarning("Locale file {_localeFileName} is empty.", localeFileName);
                return dict;
            }

            return dict;
        }


        private readonly Dictionary<string, Dictionary<string, string>> m_Locales;

        private readonly InternalTrackerSvc m_TrackerSvc;
        private readonly ILogger<LocalizationSvc> m_Logger;
    }

}

/*  App/Services/LocalizationSvc.cs
 *  Version: v1.6 (2023.10.29)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppHelper.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Project24.App.Utils;
using Project24.Model.Identity;

namespace Project24.App.Services
{
    public class LocalizationSvc : IProject24HostedService
    {
        #region SupportedLocale Helper class
        public static class SupportedLocale
        {
            public const string EN_US = "en-US";
            public const string JA_JP = "ja-JP";
            public const string VI_VN = "vi-VN";


            public static string[] AllSupportedLocale => m_All;

            public static bool IsSupported(string _locale)
            {
                foreach (string locale in m_All)
                {
                    if (locale == _locale)
                        return true;
                }

                return false;
            }


            private static readonly string[] m_All = { EN_US, JA_JP, VI_VN };
        }
        #endregion

        #region Front-end Localization Status
        public class FrontEndLocalizationStatus
        {
            public int TotalEntries { get; private set; }
            public int UntranslatedEntries { get; private set; }
            public int InvalidEntries { get; private set; }

            public int TranslatedEntries => TotalEntries - UntranslatedEntries;
            public int ValidEntries => TotalEntries - InvalidEntries;

            public FrontEndLocalizationStatus(int _totalEntries, int _unstranslatedEntries, int _invalidEntries)
            {
                TotalEntries = _totalEntries;
                UntranslatedEntries = _unstranslatedEntries;
                InvalidEntries = _invalidEntries;
            }
        }
        #endregion

        public const string LocaleCookieKey = "p24-lang";


        public static string[] AllSupportedLocale => SupportedLocale.AllSupportedLocale;
 

        public string this[string _key] { get => GetLocalizedString(_key); }
        public string this[string _locale, string _key] { get=>GetLocalizedString(_locale, _key); }

        public string CurrentLocale { get => m_TrackerSvc[InternalTrackedKeys.CONFIG_GLOBAL_LOCALIZATION]; }


        public LocalizationSvc(InternalTrackerSvc _trackerSvc, IServiceProvider _serviceProvider, ILogger<LocalizationSvc> _logger, IHttpContextAccessor _httpContextAccessor)
        {
            m_TrackerSvc = _trackerSvc;
            m_ServiceProvider = _serviceProvider;
            m_Logger = _logger;

            m_HttpContextAccessor = _httpContextAccessor;

            m_Locales = new();
            m_FrontEndLocalizationStatuses = new();

        }


        public async Task StartAsync(CancellationToken _cancellationToken = default)
        {
            _ = await ReloadLocaleStringAsync(_cancellationToken);

            m_Logger.LogInformation("Localization Service initialized.");
        }

        public bool ContainsKey(string _key) => m_Locales[CurrentLocale].ContainsKey(_key);

        public string[] GetAllLocalizedStrings(string _locale)
        {
            if (!SupportedLocale.IsSupported(_locale))
                return Array.Empty<string>();

            return m_Locales[_locale].Values.ToArray();
        }

        public string GetLocalizedString(string _key)
        {
            if (m_HttpContextAccessor.HttpContext.Items.ContainsKey(LocaleCookieKey))
                return GetLocalizedString((string)m_HttpContextAccessor.HttpContext.Items[LocaleCookieKey], _key);

            return GetLocalizedString(CurrentLocale, _key);
        }

        public string GetLocalizedString(string _locale, string _key)
        {
            if (!m_Locales.ContainsKey(_locale) || !m_Locales[_locale].ContainsKey(_key))
                return "<code>{" + _key + "}</code>";

            return m_Locales[_locale][_key];
        }

        public string GetLocaleForUser(string _username)
        {
            if (m_UserLocaleCache.ContainsKey(_username))
                return m_UserLocaleCache[_username];

            using var scope = m_ServiceProvider.CreateScope();
            UserManager<P24IdentityUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<P24IdentityUser>>();

            P24IdentityUser user = userManager.FindByNameAsync(_username).Result;
            if (user == null)
                return null;

            string userLocale = user.PreferredLocale;
            m_UserLocaleCache[_username] = userLocale;
            return userLocale;
        }

        public FrontEndLocalizationStatus GetFrontEndLocalizationStatus (string _locale)
        {
            if (string.IsNullOrEmpty(_locale) || !m_FrontEndLocalizationStatuses.ContainsKey(_locale))
                return null;

            return m_FrontEndLocalizationStatuses[_locale];
        }

        public void ClearCachedLocaleForUser(string _username)
        {
            if (m_UserLocaleCache.ContainsKey(_username))
                m_UserLocaleCache.Remove(_username);
        }

        public async Task<bool> ReloadLocaleStringAsync(CancellationToken _cancellationToken = default)
        {
            m_Locales[SupportedLocale.EN_US] = await LoadLocalizedStringAsync(SupportedLocale.EN_US, _cancellationToken);
            m_Locales[SupportedLocale.JA_JP] = await LoadLocalizedStringAsync(SupportedLocale.JA_JP, _cancellationToken);
            m_Locales[SupportedLocale.VI_VN] = await LoadLocalizedStringAsync(SupportedLocale.VI_VN, _cancellationToken);

            m_UserLocaleCache = new();

            _ = await TaskExt.WhenAll(
                ValidateFrontEndLocaleFile(SupportedLocale.EN_US, _cancellationToken),
                ValidateFrontEndLocaleFile(SupportedLocale.JA_JP, _cancellationToken),
                ValidateFrontEndLocaleFile(SupportedLocale.VI_VN, _cancellationToken)
            );

            return true;
        }


        private async Task<Dictionary<string, string>> LoadLocalizedStringAsync(string _locale, CancellationToken _cancellationToken = default)
        {
            if (string.IsNullOrEmpty(_locale))
            {
                m_Logger.LogWarning("Invalid locale {_locale}.", _locale);
                return new();
            }

            string filePath = Program.WorkingDir + "/wwwroot/res/locale";
            string fileName = _locale + ".ini";
            if (!File.Exists(filePath + "/" + fileName))
            {
                m_Logger.LogWarning("Locale file {_localeFileName} does not exist.", fileName);
                return new();
            }

            LocaleFileUtils.LocaleFile localeFile = LocaleFileUtils.ReadLocaleFileIni(filePath, fileName, false);
            if (!string.IsNullOrEmpty(localeFile.Error))
            {
                m_Logger.LogWarning("{_error}", localeFile.Error);
                return new();
            }

            Dictionary<string, string> dict = new();

            string untranslatedKeys = "";
            int untranslatedCount = 0;

            foreach (var pair in localeFile.Entries)
            {
                var entry = pair.Value;

                if (string.IsNullOrWhiteSpace(entry.Value))
                {
                    untranslatedKeys += "\n    (" + entry.LineNumber + ") " + entry.Key;
                    ++untranslatedCount;
                    continue;
                }

                dict[entry.Key] = entry.Value;
            }

            if (localeFile.InvalidEntries.Count > 0)
            {
                string invalidKeys = "";
                foreach (var entry in localeFile.InvalidEntries)
                {
                    invalidKeys += "\n    (" + entry.LineNumber + ") ";
                    if (!string.IsNullOrWhiteSpace(entry.Key))
                        invalidKeys += entry.Key;
                }

                m_Logger.LogWarning("{_count} invalid entries in locale file {_fileName}:{_list}", localeFile.InvalidEntries.Count, fileName, invalidKeys);
            }

            if (untranslatedCount > 0)
            {
                m_Logger.LogWarning("{_count} untranslated entries in locale file {_fileName}:{_list}", untranslatedCount, fileName, untranslatedKeys);
            }

            return dict;
        }

        private async Task<bool> ValidateFrontEndLocaleFile(string _locale, CancellationToken _cancellationToken = default)
        {
            if (string.IsNullOrEmpty(_locale))
            {
                m_Logger.LogWarning("Invalid locale {_locale}.", _locale);
                return false;
            }

            string filePath = Program.WorkingDir + "/wwwroot/js/locale";
            string fileName = LocaleFileUtils.FrontEndLocaleFileNamePrefix + "-" + _locale.ToLower() + ".js";
            if (!File.Exists(filePath + "/" + fileName))
            {
                m_Logger.LogWarning("Locale file {_localeFileName} does not exist.", fileName);
                return false;
            }

            LocaleFileUtils.LocaleFile localeFile = LocaleFileUtils.ReadLocaleFileJs(filePath, fileName, false);
            if (!string.IsNullOrEmpty(localeFile.Error))
            {
                m_Logger.LogWarning("{_error}", localeFile.Error);
                return false;
            }

            string untranslatedKeys = "";
            int untranslatedCount = 0;

            foreach (var pair in localeFile.Entries)
            {
                var entry = pair.Value;

                if (string.IsNullOrWhiteSpace(entry.Value))
                {
                    untranslatedKeys += "\n    (" + entry.LineNumber + ") " + entry.Key;
                    ++untranslatedCount;
                }
            }

            if (localeFile.InvalidEntries.Count > 0)
            {
                string invalidKeys = "";
                foreach (var entry in localeFile.InvalidEntries)
                {
                    invalidKeys += "\n    (" + entry.LineNumber + ") ";
                    if (!string.IsNullOrWhiteSpace(entry.Key))
                        invalidKeys += entry.Key;
                }

                m_Logger.LogWarning("{_count} invalid entries in locale file {_fileName}:{_list}", localeFile.InvalidEntries.Count, fileName, invalidKeys);
            }

            if (untranslatedCount > 0)
            {
                m_Logger.LogWarning("{_count} untranslated entries in locale file {_fileName}:{_list}", untranslatedCount, fileName, untranslatedKeys);
            }

            m_FrontEndLocalizationStatuses[_locale] = new(localeFile.TotalEntriesCount, untranslatedCount, localeFile.InvalidEntries.Count);

            return true;
        }

        //private KeyValuePair<string, string>? ParseSingleLine(string _locale, string _line, int _index)
        //{
        //    int pos = _line.IndexOf('=');
        //    if (pos < 0)
        //    {
        //        m_Logger.LogWarning("[{_locale}]: Malformed record: ({_line})[{_lineText}]", _locale, _index + 1, _line);
        //        return null;
        //    }

        //    string key = _line[..pos].Trim();
        //    string value = _line[(pos + 1)..].Trim();

        //    if (key == "" || value == "")
        //    {
        //        m_Logger.LogWarning("[{_locale}]: Malformed record: ({_line})[{_lineText}]", _locale, _index + 1, _line);
        //        return null;
        //    }

        //    return new(key, value);
        //}


        private readonly Dictionary<string, Dictionary<string, string>> m_Locales;
        private readonly Dictionary<string, FrontEndLocalizationStatus> m_FrontEndLocalizationStatuses;
        private Dictionary<string, string> m_UserLocaleCache;

        private readonly InternalTrackerSvc m_TrackerSvc;
        private readonly IServiceProvider m_ServiceProvider;
        private readonly ILogger<LocalizationSvc> m_Logger;

        private readonly IHttpContextAccessor m_HttpContextAccessor;
    }

}

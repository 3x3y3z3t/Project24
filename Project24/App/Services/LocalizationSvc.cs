/*  App/Services/LocalizationSvc.cs
 *  Version: v1.4 (2023.10.14)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using AppHelper.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Project24.Model.Identity;
using Project24.Pages.Simulator.FinancialManagement;

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

        }


        public async Task StartAsync(CancellationToken _cancellationToken = default)
        {
            m_Locales[SupportedLocale.EN_US] = await LoadLocalizedStringAsync(SupportedLocale.EN_US, _cancellationToken);
            m_Locales[SupportedLocale.JA_JP] = await LoadLocalizedStringAsync(SupportedLocale.JA_JP, _cancellationToken);
            m_Locales[SupportedLocale.VI_VN] = await LoadLocalizedStringAsync(SupportedLocale.VI_VN, _cancellationToken);

            m_UserLocaleCache = new();

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

        public void ClearCachedLocaleForUser(string _username)
        {
            if (m_UserLocaleCache.ContainsKey(_username))
                m_UserLocaleCache.Remove(_username);
        }


        private async Task<Dictionary<string, string>> LoadLocalizedStringAsync(string _locale, CancellationToken _cancellationToken = default)
        {
            if (string.IsNullOrEmpty(_locale))
                return new();

            string localeFileName = _locale + ".ini";
            string path = Path.GetFullPath(Program.WorkingDir + "/wwwroot/res/locale/" + localeFileName);
            if (!File.Exists(path))
            {
                m_Logger.LogWarning("Locale file {_localeFileName} does not exist.", localeFileName);
                return new();
            }

            LocaleFileUtils.LocaleFile localeFile = LocaleFileUtils.ReadLocaleFile(path, _locale, false, false, false);
            if (!string.IsNullOrEmpty(localeFile.Error))
            {
                m_Logger.LogWarning("{_error}", localeFile.Error);
                return new();
            }

            Dictionary<string, string> dict = new();

            string malformRecords = "";
            int malformCount = 0;

            string untranslatedKeys = "";
            int untranslatedCount = 0;

            foreach (var pair in localeFile.Entries)
            {
                if (pair.Key.StartsWith("<>"))
                {
                    malformRecords += "\n    (" + pair.Key[2..] + ") " + pair.Value;
                    ++malformCount;
                }
                else if (pair.Value == "")
                {
                    untranslatedKeys += "\n    " + pair.Key;
                    ++untranslatedCount;
                }
                else
                {
                    dict[pair.Key] = pair.Value;
                }
            }

            if (malformCount > 0)
            {
                m_Logger.LogWarning("{_count} Malformed entries in locale file for {_locale}:{_list}", malformCount, _locale, malformRecords);
            }

            if (untranslatedCount > 0)
            {
                m_Logger.LogWarning("{_count} Untranslated entries in locale file for {_locale}:{_list}", untranslatedCount, _locale, untranslatedKeys);
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
        private Dictionary<string, string> m_UserLocaleCache;

        private readonly InternalTrackerSvc m_TrackerSvc;
        private readonly IServiceProvider m_ServiceProvider;
        private readonly ILogger<LocalizationSvc> m_Logger;

        private readonly IHttpContextAccessor m_HttpContextAccessor;
    }

}

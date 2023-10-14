/*  App/Middlewares/P24LocalizationMiddleware.cs
 *  Version: v1.0 (2023.10.08)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Project24.App.Services;

namespace Project24.App.Middlewares
{
    public class P24LocalizationMiddleware
    {
        public P24LocalizationMiddleware(LocalizationSvc _localizationSvc, RequestDelegate _next)
        {
            m_LocalizationSvc = _localizationSvc;

            m_NextMiddleware = _next;
        }


        public async Task InvokeAsync(HttpContext _httpContext)
        {
            string requestCookieValue = GetLocaleValueFromCookie(_httpContext.Request);

            // try get locale perference from user;
            if (ProcessLocaleFromUser(_httpContext))
            {
                string userValue = (string)_httpContext.Items[c_CookieKey];
                if (requestCookieValue == null || requestCookieValue != userValue)
                {
                    _httpContext.Response.Cookies.Append(c_CookieKey, userValue, s_LocaleCookieOption);
                }

                await m_NextMiddleware(_httpContext);
                return;
            }

            // try get locale preference from cookie or set using app default;
            if (requestCookieValue != null && LocalizationSvc.SupportedLocale.IsSupported(requestCookieValue))
            {
                _httpContext.Items[c_CookieKey] = requestCookieValue;
            }
            else
            {
                _httpContext.Items[c_CookieKey] = m_LocalizationSvc.CurrentLocale;
                _httpContext.Response.Cookies.Append(c_CookieKey, m_LocalizationSvc.CurrentLocale, s_LocaleCookieOption);
            }

            await m_NextMiddleware(_httpContext);
        }

        private bool ProcessLocaleFromUser(HttpContext _httpContext)
        {
            var user = _httpContext.User;
            if (user == null || user.Identity == null || string.IsNullOrWhiteSpace(user.Identity.Name))
                return false;

            string value = m_LocalizationSvc.GetLocaleForUser(user.Identity.Name);
            if (value == null)
                return false;

            _httpContext.Items[c_CookieKey] = value;

            return true;
        }

        private static string GetLocaleValueFromCookie(HttpRequest _request)
        {
            var cookies = _request.Cookies;
            foreach (var cookie in cookies)
            {
                if (cookie.Key == c_CookieKey)
                {
                    return cookie.Value;
                }
            }

            return null;
        }


        private const string c_CookieKey = LocalizationSvc.LocaleCookieKey;

        private static readonly CookieOptions s_LocaleCookieOption = new()
        {
            MaxAge = new TimeSpan(30, 0, 0, 0),
        };


        private readonly LocalizationSvc m_LocalizationSvc;

        private readonly RequestDelegate m_NextMiddleware;
    }

}

/*  App/Middlewares/P24AuthenticationMiddleware.cs
 *  Version: v1.0 (2023.10.15)
 *  
 *  Author
 *      Arime-chan
 */

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Project24.Model.Identity;
using System;
using System.Threading.Tasks;

namespace Project24.App.Middlewares
{
    public class P24PostAuthentication
    {
        public P24PostAuthentication(IServiceProvider _serviceProvider, ILogger<P24PostAuthentication> _logger, RequestDelegate _next)
        {
            m_ServiceProvider = _serviceProvider;
            m_Logger = _logger;

            m_NextMiddleware = _next;
        }

        public async Task InvokeAsync(HttpContext _httpContext)
        {
            var identity = _httpContext.User.Identity;
            if (identity == null || !identity.IsAuthenticated || string.IsNullOrWhiteSpace(identity.Name)
                || Program.RolesDirtyUser.Count <= 0)
            {
                await m_NextMiddleware(_httpContext);
                return;
            }

            foreach (string username in Program.RolesDirtyUser)
            {
                if (_httpContext.User.Identity.Name != username)
                    continue;

                using var scope = m_ServiceProvider.CreateScope();
                var signinManager = scope.ServiceProvider.GetRequiredService<SignInManager<P24IdentityUser>>();

                await signinManager.SignOutAsync();
                m_Logger.LogInformation("Force signed out user {_username} due to role changed.", username);
                // TODO: maybe let user know that they have been signed out due to role changes;

                Program.RolesDirtyUser.Remove(username);
                break;
            }

            await m_NextMiddleware(_httpContext);
        }


        private readonly IServiceProvider m_ServiceProvider;
        private readonly ILogger<P24PostAuthentication> m_Logger;

        private readonly RequestDelegate m_NextMiddleware;
    }

}

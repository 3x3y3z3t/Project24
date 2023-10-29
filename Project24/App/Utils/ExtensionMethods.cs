/*  App/Utils/ExtensionMethods.cs
 *  Version: v1.3 (2023.10.29)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project24.Data;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Project24.App.Services;
using Microsoft.Extensions.Logging;
using Project24.Model.Identity;
using Project24.SerializerContext;
using Project24.Model;
using Microsoft.AspNetCore.Builder;
using Project24.App.Middlewares;
using Microsoft.AspNetCore.Identity;

namespace Project24.App
{
    public static class ExtensionMethods
    {
        #region DateTime
        public static string ToStringDefault(this DateTime _dt)
        {
            return string.Format("{0:yyyy}/{0:MM}/{0:dd} {0:HH}:{0:mm}:{0:ss}", _dt);
        }
        #endregion

        public static bool IsDbLockedForSync(this PageModel _page, DBMaintenanceSvc _dbAccessLockSvc, ILogger<PageModel> _logger)
        {
            if (_dbAccessLockSvc.AccessState == DbAccessState.LockForSync)
            {
                _logger.LogInformation("Db access aborted (lock for sync).");
                return true;
            }

            return false;
        }


        public static void RecordUserAction(this ApplicationDbContext _dbContext,
            string _username,
            string _operation,
            string _status,
            Dictionary<string, string> _customInfo = null)
        {
            string jsonData = null;
            if (_customInfo != null)
            {
                jsonData = JsonSerializer.Serialize(_customInfo, P24JsonSerializerContext.JsonSerializerOptionsFullUnicodeRange);
            }

            _dbContext.RecordUserAction(_username, _operation, _status, jsonData);
        }

        public static void RecordUserAction(this ApplicationDbContext _dbContext,
            string _username,
            string _operation,
            string _status,
            string _customInfo)
        {
            UserAction record = new UserAction()
            {
                Timestamp = DateTime.Now,
                Username = _username,
                Operation = _operation,
                OperationStatus = _status,
                CustomInfo = _customInfo
            };

            _dbContext.Add(record);
            _dbContext.SaveChanges();
        }


        public static bool IsDbLockedForImport(this PageModel _page, DBMaintenanceSvc _dbAccessLockSvc, ILogger<PageModel> _logger)
        {
            if (_dbAccessLockSvc.AccessState == DbAccessState.LockForImport)
            {
                _logger.LogInformation("Db access aborted (lock for import).");
                return true;
            }

            return false;
        }

        public static async Task<bool> ValidateModelState(this PageModel _page, ApplicationDbContext _dbContext, P24IdentityUser _currentUser, string _operation)
        {
            if (_page.ModelState.IsValid)
                return true;

            _dbContext.RecordUserAction(
                _currentUser.UserName,
                _operation,
                UserAction.OperationStatus_.Failed,
                new Dictionary<string, string>() { { CustomInfoKeys.Error, Message.InvalidModelState } }
            );

            return false;
        }

        public static bool IsUserAuthorized(this PageModel _page, UserManager<P24IdentityUser> _userManager, string[] _roles, ApplicationDbContext _dbContext, string _operation)
        {
            if (_page.IsUserAuthorized(_userManager, _roles))
                return true;

            if (_dbContext != null)
            {
                _dbContext.RecordUserAction(
                    _page.User?.Identity?.Name,
                    _operation,
                    UserAction.OperationStatus_.Denied
                );
            }

            return false;
        }

        public static bool IsUserAuthorized(this PageModel _page, UserManager<P24IdentityUser> _userManager, string[] _roles)
        {
            if (!_page.User.Identity.IsAuthenticated)
                return false;

            P24IdentityUser user = _userManager.GetUserAsync(_page.User).Result;
            if (user == null)
                return false;

            //var userRoles = _userManager.GetRolesAsync(user).Result;
            foreach (string role in _roles)
            {
                if (!_userManager.IsInRoleAsync(user, role).Result)
                    return false;
            }

            return true;
        }

        #region Middleware
        public static IApplicationBuilder UseP24Localization(this IApplicationBuilder _builder)
        {
            return _builder.UseMiddleware<P24LocalizationMiddleware>();
        }

        public static IApplicationBuilder UseP24PostAuthentication(this IApplicationBuilder _builder)
        {
            return _builder.UseMiddleware<P24PostAuthentication>();
        }
        #endregion
    }

}

/*  App/Utils/ExtensionMethods.cs
 *  Version: v1.1 (2023.09.30)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project24.Data;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using Project24.App.Services;
using System.Net.Mime;
using Microsoft.Extensions.Logging;

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

        public static bool IsDbLockedForImport(this PageModel _page, DBMaintenanceSvc _dbAccessLockSvc, ILogger<PageModel> _logger)
        {
            if (_dbAccessLockSvc.AccessState == DbAccessState.LockForImport)
            {
                _logger.LogInformation("Db access aborted (lock for import).");
                return true;
            }

            return false;
        }
    }

}

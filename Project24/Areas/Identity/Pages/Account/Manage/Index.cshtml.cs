/*  Identity/Account/Manage/Index.cshtml.cs
 *  Version: 1.1 (2022.12.12)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project24.App;
using Project24.App.Utils;
using Project24.Data;
using Project24.Models.Identity;

namespace Project24.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public partial class IndexModel : PageModel
    {
        public struct UserUploadInfo
        {
            public AppModule Module;
            public int FilesCount;
            public long BytesCount;
        }
        public class AccountProfileQuickViewModel
        {
            public string Username { get; set; }

            public List<string> Roles { get; set; }
            public P24RoleClaimUtils.ModuleAccessAllowances Access { get; set; }

            public DateTime JoinedDate { get; set; }
            
            public UserUploadInfo ClinicManagerUploads { get; set; }
            public UserUploadInfo NasUploads { get; set; }

            public AccountProfileQuickViewModel()
            { }
        }

        public AccountProfileQuickViewModel Profile { get; private set; }

        [TempData]
        public string StatusMessage { get; set; }


        public IndexModel(ApplicationDbContext _dbContext, UserManager<P24IdentityUser> _userManager)
        {
            m_DbContext = _dbContext;
            m_UserManager = _userManager;
        }


        public async Task OnGetAsync()
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);

            var roles = await m_UserManager.GetRolesAsync(currentUser);

            var uploads = await (from _upload in m_DbContext.UserUploads
                                 where _upload.UserId == currentUser.Id
                                 group _upload by _upload.Module into _group
                                 select new UserUploadInfo()
                                 {
                                     Module = _group.Key,
                                     FilesCount = _group.Sum(_up => _up.FilesCount),
                                     BytesCount = _group.Sum(_up => _up.BytesCount)
                                 })
                         .ToDictionaryAsync(_upload => _upload.Module);
            //.ToListAsync();

            Profile = new AccountProfileQuickViewModel()
            {
                Username = currentUser.UserName,

                Roles = new List<string>(roles),
                Access = P24RoleClaimUtils.GetHighestAccessAllowance(roles),

                JoinedDate = currentUser.JoinDateTime,

                ClinicManagerUploads = uploads.GetValueOrDefault(AppModule.P24_ClinicManager),
                NasUploads = uploads.GetValueOrDefault(AppModule.P24b_Nas),
            };
        }

        public static HtmlString GetAccessAllowanceHtmlString(P24RoleClaimUtils.Module _module, P24RoleClaimUtils.AccessAllowance _access)
        {
            string moduleName = "???";

            if (_access != P24RoleClaimUtils.AccessAllowance.Restricted)
            {
                switch (_module)
                {
                    case P24RoleClaimUtils.Module.Dashboard:
                        moduleName = "Dashboard";
                        break;
                    case P24RoleClaimUtils.Module.P24_ClinicManager:
                        moduleName = "Clinic Manager";
                        break;
                    case P24RoleClaimUtils.Module.P24b_Nas:
                        moduleName = "NAS";
                        break;
                }
            }

            string access = P24Constants.No;
            string textClass = "text-danger";
            switch (_access)
            {
                case P24RoleClaimUtils.AccessAllowance.NoAccess:
                    textClass = "text-danger";
                    access = P24Constants.No;
                    break;
                case P24RoleClaimUtils.AccessAllowance.Restricted:
                    textClass = "text-warning";
                    access = P24Constants.Restricted;
                    break;
                case P24RoleClaimUtils.AccessAllowance.FullAccess:
                    textClass = "text-success";
                    access = P24Constants.Yes;
                    break;
            }

            return new HtmlString($"<div class=\"{textClass}\">{moduleName}: {access}</div>");
        }

        public static HtmlString GetUploadDataHtmlString(P24RoleClaimUtils.Module _module, P24RoleClaimUtils.AccessAllowance _access, long _uploadedBytes)
        {
            string moduleNameStatic = "";
            if (_module == P24RoleClaimUtils.Module.P24_ClinicManager)
                moduleNameStatic = "Clinic Manager";
            else if (_module == P24RoleClaimUtils.Module.P24b_Nas)
                moduleNameStatic = "NAS";

            string textClass = (_access == P24RoleClaimUtils.AccessAllowance.NoAccess) ? "text-muted" : null;
            string moduleName = (_access == P24RoleClaimUtils.AccessAllowance.NoAccess) ? "???" : (moduleNameStatic + " upload");
            string uploaded = (_access == P24RoleClaimUtils.AccessAllowance.NoAccess) ? "0" : AppUtils.FormatDataSize(_uploadedBytes);

            string dt = $"<dt class=\"{textClass} col-4\">{moduleName}:</dt>";
            string dd = $"<dd class=\"{textClass} col-8\">{uploaded}</dd>";

            return new HtmlString(dt + dd);
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
    }

}

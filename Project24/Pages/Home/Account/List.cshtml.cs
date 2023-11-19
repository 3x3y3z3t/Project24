/*  Home/Account/List.cshtml
 *  Version: v1.0 (2023.10.19)
 *  
 *  Author
 *      Arime-chan
 */

using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project24.App;
using Project24.App.Utils;
using Project24.Model.Identity;
using Project24.Data;

namespace Project24.Pages.Home.Account
{
    [Authorize(Roles = PageCollection.Home.Account.List)]
    public class ListModel : PageModel
    {
        public ListModel(UserManager<P24IdentityUser> _userManager, RoleManager<P24IdentityRole> _roleManager, ApplicationDbContext _dbContext)
        {
            m_UserManager = _userManager;
            m_RoleManager = _roleManager;
            m_DbContext = _dbContext;
        }


        public void OnGet()
        { }

        #region AJAX Handler
        // ==================================================
        // AJAX Handler

        // ajax handler;
        public IActionResult OnGetFetchPageData()
        {
            var users = (from _user in m_DbContext.P24Users
                         join _rolesCount in (from _userRole in m_DbContext.UserRoles
                                              group _userRole by _userRole.UserId into roles
                                              select new { roles.Key, Value = roles.Count() })
                         on _user.Id equals _rolesCount.Key
                         select new
                         {
                             _user.Id,
                             _user.UserName,
                             _user.AddedDateTime,
                             _user.RemovedDateTime,
                             Access = _rolesCount.Value
                         })
                        .ToList();

            string jsonData = JsonSerializer.Serialize(users);
            return Content(MessageTag.Success + jsonData, MediaTypeNames.Text.Plain);
        }

        // End: AJAX Handler
        // ==================================================
        #endregion


        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly RoleManager<P24IdentityRole> m_RoleManager;
        private readonly ApplicationDbContext m_DbContext;
    }

}

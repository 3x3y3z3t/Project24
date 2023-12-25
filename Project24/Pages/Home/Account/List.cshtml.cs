/*  Home/Account/List.cshtml
 *  Version: v1.1 (2023.12.24)
 *  Spec:    v0.1
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
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Project24.Pages.Home.Account
{
    [Authorize(Roles = PageCollection.Home.Account.List)]
    public class ListModel : PageModel
    {
        internal class ViewModel
        {
            public string Id { get; set; }
            public string UserName { get; set; }
            public DateTime AddedDateTime { get; set; }
            public DateTime RemovedDateTime { get; set; }
            public int Access { get; set; }

            public ViewModel()
            { }
        }


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

        public async Task<IActionResult> OnGetFetchPageDataAsync()
        {
            var users = await (from _user in m_DbContext.P24Users
                               //join _rolesCount in (from _userRole in m_DbContext.UserRoles
                               //                     group _userRole by _userRole.UserId into roles
                               //                     select new { roles.Key, Value = roles.Count() })
                               //on _user.Id equals _rolesCount.Key
                               orderby _user.UserName
                               select new ViewModel()
                               {
                                   Id = _user.Id,
                                   UserName = _user.UserName,
                                   AddedDateTime = _user.AddedDateTime,
                                   RemovedDateTime = _user.RemovedDateTime,
                                   Access = 0
                               })
                              .ToListAsync();

            /*  In EF Core 7.0 there is Group Join which can be used to simulate a left outer join.
             *  Unfortunately we are at 6.0, so there is no left outer join.
             */
            foreach (var user in users)
            {
                var rolesCount = await (from _roles in m_DbContext.UserRoles where _roles.UserId == user.Id select _roles.RoleId).CountAsync();
                user.Access = rolesCount;
            }

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

/*  AddUser.cshtml.cs
 *  Version: 1.3 (2022.09.07)
 *
 *  Contributor
 *      Arime-chan
 */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Project24.Data;
using Project24.Identity;
using Project24.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Project24.Pages.UserManagement
{
    [Authorize(Roles = Constants.ROLE_MANAGER)]
    public class AddUserModel : PageModel
    {
        public class DataModel
        {
            public string Username { get; set; }

            [Required(ErrorMessage = "Họ và Tên không được để trống")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Quyền hạn không được để trống")]
            public int RoleLevel { get; set; }

            [Required(ErrorMessage = "Mật khẩu không được để trống")]
            [DataType(DataType.Password)]
            public string ManagerPassword { get; set; }



        }

        [BindProperty]
        public DataModel Data { get; set; }
        public string StatusMessage { get; set; }
        public string ReturnUrl { get; set; }

        public AddUserModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager, RoleManager<P24IdentityRole> _roleManager, ILogger<AddUserModel> _logger)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
            m_RoleManager = _roleManager;
            m_Logger = _logger;

        }

        public async Task OnGetAsync(string _returnUrl = null)
        {
            if (string.IsNullOrEmpty(_returnUrl))
                ReturnUrl = "./Index";
            else
                ReturnUrl = _returnUrl;

            var currentUser = await m_UserManager.GetUserAsync(User);
            if (currentUser == null)
            {
                StatusMessage = "Error: Bạn cần đăng nhập để thực hiện chức năng này.";
                RedirectToPage(ReturnUrl); // this should not happens;
            }
            var currentRole = await m_UserManager.GetRolesAsync(currentUser);
            if (currentRole == null)
            {
                StatusMessage = "Error: Bạn không có đủ quyền hạn để thực hiện chức năng này.";
                RedirectToPage(ReturnUrl); // this should not happens;
            }

            var firstRole = (from _users in m_DbContext.P24Users
                             where _users.UserName == currentUser.UserName
                             join _userRoles in m_DbContext.UserRoles on _users.Id equals _userRoles.UserId
                             join _roles in m_DbContext.P24Roles on _userRoles.RoleId equals _roles.Id
                             orderby _roles.Level
                             select new
                             {
                                 _roles.Level,
                             })
                            .First();

            var roles = (from _role in m_DbContext.P24Roles
                         where _role.Level > firstRole.Level
                         orderby _role.Level
                         select new Tuple<int, string>(_role.Level, _role.Name))
                        .ToList();


            ViewData["RoleList"] = new SelectList(roles, "Item1", "Item2");


        }

        public async Task<IActionResult> OnPostAsync(string _returnUrl = null)
        {
            if (string.IsNullOrEmpty(_returnUrl))
                ReturnUrl = "./Index";
            else
                ReturnUrl = _returnUrl;

            //return RedirectToPage(ReturnUrl);

            var currentUser = await m_UserManager.GetUserAsync(User);
            if (currentUser == null)
            {
                StatusMessage = "Error: Bạn cần đăng nhập để thực hiện chức năng này.";
                RedirectToPage(ReturnUrl); // this should not happens;
            }
            var currentRole = await m_UserManager.GetRolesAsync(currentUser);
            if (currentRole == null)
            {
                StatusMessage = "Error: Bạn không có đủ quyền hạn để thực hiện chức năng này.";
                RedirectToPage(ReturnUrl); // this should not happens;
            }

            var firstRole = (from _users in m_DbContext.P24Users
                             where _users.UserName == currentUser.UserName
                             join _userRoles in m_DbContext.UserRoles on _users.Id equals _userRoles.UserId
                             join _roles in m_DbContext.P24Roles on _userRoles.RoleId equals _roles.Id
                             orderby _roles.Level
                             select new
                             {
                                 _roles.Level,
                             })
                            .First();

            if (ModelState.IsValid)
            {
                currentUser = await m_UserManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    StatusMessage = "Error: Bạn cần đăng nhập để thực hiện chức năng này.";
                    return Page(); // this should not happens;
                }

                if (!await m_UserManager.CheckPasswordAsync(currentUser, Data.ManagerPassword))
                {
                    StatusMessage = "Error: " + Constants.ERROR_MANAGER_PASSWORD_INCORRECT;

                    await Utils.RecordAction(
                        m_DbContext,
                        currentUser.UserName,
                        ActionRecord.Operation_.CreateUser,
                        ActionRecord.OperationStatus_.Failed,
                        Constants.ERROR_MANAGER_PASSWORD_INCORRECT
                    );

                    return Page();
                }

                if (Data.RoleLevel < firstRole.Level)
                {
                    await Utils.RecordAction(
                        m_DbContext,
                        currentUser.UserName,
                        ActionRecord.Operation_.CreateUser,
                        ActionRecord.OperationStatus_.Denied,
                        Constants.ERROR_NOT_ENOUGH_PRIVILEGE
                    );
                    return Redirect("_CommonAccessDenied");
                }

                var tokens = Utils.TokenizeName(Data.FullName);

                var user = new P24IdentityUser()
                {
                    UserName = Data.Username,
                    EmailConfirmed = true,
                    FamilyName = tokens.Item1,
                    MiddleName = tokens.Item2,
                    LastName = tokens.Item3,
                    JoinDateTime = DateTime.Now,
                };

                var result = await m_UserManager.CreateAsync(user, Constants.DEFAULT_PASSWORD);
                if (result.Succeeded)
                {
                    for (int i = Data.RoleLevel; i < Constants.s_Roles.Length; ++i)
                    {
                        result = await m_UserManager.AddToRoleAsync(user, Constants.s_Roles[i]);
                    }

                    m_Logger.LogInformation("New account created with default password.");

                    await Utils.RecordAction(
                        m_DbContext,
                        currentUser.UserName,
                        ActionRecord.Operation_.CreateUser,
                        ActionRecord.OperationStatus_.Success
                    );

                    return RedirectToPage(ReturnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

            }

            // If we got this far, something failed, redisplay form
            StatusMessage = "Error: Lỗi không xác định.";

            await Utils.RecordAction(
                m_DbContext,
                currentUser.UserName,
                ActionRecord.Operation_.CreateUser,
                ActionRecord.OperationStatus_.Failed,
                Constants.ERROR_UNKNOWN_ERROR_FROM_PROGRAM
            );

            return Page();
        }

        private readonly ApplicationDbContext m_DbContext;

        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly RoleManager<P24IdentityRole> m_RoleManager;
        private readonly ILogger<AddUserModel> m_Logger;
    }

}

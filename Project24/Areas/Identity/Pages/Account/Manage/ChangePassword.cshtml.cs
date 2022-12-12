/*  Identity/Account/Manage/ChangePassword.cshtml.cs
 *  Version: 1.0 (2022.12.12)
 *
 *  Contributor
 *      Arime-chan
 */

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.Data;
using Project24.Models;
using Project24.Models.Identity;

namespace Project24.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        public class ChangePwFormDataModel
        {
            [Required(ErrorMessage = P24Message.PasswordCannotBeEmpty)]
            [DataType(DataType.Password)]
            public string CurrentPassword { get; set; }

            [Required(ErrorMessage = P24Message.PasswordCannotBeEmpty)]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; }

            [Required(ErrorMessage = P24Message.PasswordCannotBeEmpty)]
            [DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "Xác nhận mật khẩu không khớp.")]
            public string NewPasswordConfirm { get; set; }

            public ChangePwFormDataModel()
            { }
        }

        [BindProperty]
        public ChangePwFormDataModel FormData { get; set; }

        [TempData]
        public string StatusMessage { get; set; }


        public ChangePasswordModel(ApplicationDbContext _dbContext, UserManager<P24IdentityUser> _userManager, SignInManager<P24IdentityUser> _signInManager, ILogger<ChangePasswordModel> _logger)
        {
            m_DbContext = _dbContext;
            m_UserManager = _userManager;
            m_SigninManager = _signInManager;
            m_Logger = _logger;
        }


        public void OnGet()
        { }

        public async Task<IActionResult> OnPostAsync()
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                await m_DbContext.RecordChanges(
                    currentUser.UserName,
                    ActionRecord.Operation_.Account_ChangePassword,
                    ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.Error, ErrorMessage.InvalidModelState }
                    }
                );

                return Page();
            }

            var result = await m_UserManager.ChangePasswordAsync(currentUser, FormData.CurrentPassword, FormData.NewPassword);
            if (!result.Succeeded)
            { 
                string errorMsg = "";
                foreach (var error in result.Errors)
                {
                    errorMsg += error.Description + ", ";
                }
                errorMsg.TrimEnd(new char[] { ',', ' ' });

                await m_DbContext.RecordChanges(
                    currentUser.UserName,
                    ActionRecord.Operation_.Account_ChangePassword,
                    ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.Error, errorMsg }
                    }
                );
                m_Logger.LogInformation("User " + currentUser.UserName + " failed to change password: " + errorMsg);

                StatusMessage = CustomInfoTag.Error + "Đổi mật khẩu thất bại, vui lòng thử lại.";
                return Page();
            }

            await m_SigninManager.RefreshSignInAsync(currentUser);

            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.Account_ChangePassword,
                ActionRecord.OperationStatus_.Success
            );
            m_Logger.LogInformation("User " + currentUser.UserName + " changed password.");

                StatusMessage = CustomInfoTag.Success + "Đổi mật khẩu thành công.";

            return RedirectToPage();
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly SignInManager<P24IdentityUser> m_SigninManager;
        private readonly ILogger<ChangePasswordModel> m_Logger;
    }

}

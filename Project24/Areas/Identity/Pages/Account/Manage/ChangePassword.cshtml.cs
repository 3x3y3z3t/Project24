/*  Areas/Identity/Pages/Account/Manage/ChangePassword.cshtml.cs
 *  Version: v1.1 (2023.12.24)
 *  Spec:    v0.1
 *  
 *  Contributor
 *      The .NET Foundation (Author)
 *      Arime-chan
 */
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.App;
using Project24.Data;
using Project24.Model;
using Project24.Model.Identity;

namespace Project24.Areas.Identity.Pages.Account.Manage
{
    public class ChangePasswordModel : PageModel
    {
        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Current password")]
            public string OldPassword { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }


        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }


        public ChangePasswordModel(SignInManager<P24IdentityUser> _signInManager, UserManager<P24IdentityUser> _userManager, ApplicationDbContext _dbContext, ILogger<ChangePasswordModel> _logger)
        {
            m_SignInManager = _signInManager;
            m_UserManager = _userManager;
            m_DbContext = _dbContext;
            m_Logger = _logger;
        }


        public async Task<IActionResult> OnGetAsync()
        {
            P24IdentityUser user = await m_UserManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{m_UserManager.GetUserId(User)}'.");
            }

            bool hasPassword = await m_UserManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                return RedirectToPage("./SetPassword");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            P24IdentityUser user = await m_UserManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{m_UserManager.GetUserId(User)}'.");

            if (!this.ValidateModelState(m_DbContext, user, UserAction.Operation_.IdentityAcc_Manage_ChangePass))
            {
                return Page();
            }

            if (!await m_UserManager.CheckPasswordAsync(user, Input.OldPassword))
            {
                ModelState.AddModelError(string.Empty, "Incorrect Password.");
                m_DbContext.RecordUserAction(
                    user.UserName,
                    UserAction.Operation_.IdentityAcc_Manage_ChangePass,
                    UserAction.OperationStatus_.Failed,
                    new Dictionary<string, string>() { { CustomInfoKeys.Error, "Incorrect Password" } }
                );

                return Page();
            }

            IdentityResult result = await m_UserManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                m_DbContext.RecordUserAction(
                    user.UserName,
                    UserAction.Operation_.IdentityAcc_Manage_ChangePass,
                    UserAction.OperationStatus_.Failed
                );

                return Page();
            }

            await m_SignInManager.RefreshSignInAsync(user);

            m_DbContext.RecordUserAction(
                user.UserName,
                UserAction.Operation_.IdentityAcc_Manage_ChangePass,
                UserAction.OperationStatus_.Success
            );
            m_Logger.LogInformation("User changed their password successfully.");
            StatusMessage = "Your password has been changed.";

            return RedirectToPage();
        }


        private readonly SignInManager<P24IdentityUser> m_SignInManager;
        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly ApplicationDbContext m_DbContext;
        private readonly ILogger<ChangePasswordModel> m_Logger;
    }

}

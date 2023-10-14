/*  Areas/Identity/Pages/Account/LoginWithRecoveryCode.cshtml.cs
 *  Version: v1.0 (2023.10.08)
 *  
 *  Author
 *      The .NET Foundation
 *  
 *  Contributor
 *      Arime-chan
 */
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.Model.Identity;

namespace Project24.Areas.Identity.Pages.Account
{
    public class LoginWithRecoveryCodeModel : PageModel
    {
        public class InputModel
        {
            [BindProperty]
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Recovery Code")]
            public string RecoveryCode { get; set; }
        }


        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }


        public LoginWithRecoveryCodeModel(
            SignInManager<P24IdentityUser> _signInManager,
            UserManager<P24IdentityUser> _userManager,
            ILogger<LoginWithRecoveryCodeModel> _logger)
        {
            m_SignInManager = _signInManager;
            m_UserManager = _userManager;
            m_Logger = _logger;
        }


        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            return NotFound();

            // Ensure the user has gone through the username & password screen first
            var user = await m_SignInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            ReturnUrl = returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            return NotFound();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await m_SignInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            var recoveryCode = Input.RecoveryCode.Replace(" ", string.Empty);

            var result = await m_SignInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            var userId = await m_UserManager.GetUserIdAsync(user);

            if (result.Succeeded)
            {
                m_Logger.LogInformation("User with ID '{UserId}' logged in with a recovery code.", user.Id);
                return LocalRedirect(returnUrl ?? Url.Content("~/"));
            }
            if (result.IsLockedOut)
            {
                m_Logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");
            }
            else
            {
                m_Logger.LogWarning("Invalid recovery code entered for user with ID '{UserId}' ", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
                return Page();
            }
        }


        private readonly SignInManager<P24IdentityUser> m_SignInManager;
        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly ILogger<LoginWithRecoveryCodeModel> m_Logger;
    }

}

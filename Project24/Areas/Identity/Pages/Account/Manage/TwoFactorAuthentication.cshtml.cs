/*  Areas/Identity/Pages/Account/Manage/TwoFactorAuthentication.cshtml.cs
 *  Version: v1.0 (2023.10.10)
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

using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.Model.Identity;

namespace Project24.Areas.Identity.Pages.Account.Manage
{
    public class TwoFactorAuthenticationModel : PageModel
    {
        public TwoFactorAuthenticationModel(SignInManager<P24IdentityUser> _signInManager, UserManager<P24IdentityUser> _userManager,
            ILogger<TwoFactorAuthenticationModel> _logger)
        {
            m_SignInManager = _signInManager;
            m_UserManager = _userManager;
            m_Logger = _logger;
        }


        public bool HasAuthenticator { get; set; }

        public int RecoveryCodesLeft { get; set; }

        [BindProperty]
        public bool Is2faEnabled { get; set; }

        public bool IsMachineRemembered { get; set; }

        [TempData]
        public string StatusMessage { get; set; }


        public async Task<IActionResult> OnGetAsync()
        {
            return NotFound();

            var user = await m_UserManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{m_UserManager.GetUserId(User)}'.");
            }

            HasAuthenticator = await m_UserManager.GetAuthenticatorKeyAsync(user) != null;
            Is2faEnabled = await m_UserManager.GetTwoFactorEnabledAsync(user);
            IsMachineRemembered = await m_SignInManager.IsTwoFactorClientRememberedAsync(user);
            RecoveryCodesLeft = await m_UserManager.CountRecoveryCodesAsync(user);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return NotFound();

            var user = await m_UserManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{m_UserManager.GetUserId(User)}'.");
            }

            await m_SignInManager.ForgetTwoFactorClientAsync();
            StatusMessage = "The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code.";
            return RedirectToPage();
        }


        private readonly SignInManager<P24IdentityUser> m_SignInManager;
        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly ILogger<TwoFactorAuthenticationModel> m_Logger;
    }

}

/*  Areas/Identity/Pages/Account/Manage/ResetAuthenticator.cshtml.cs
 *  Version: v1.0 (2023.10.13)
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
    public class ResetAuthenticatorModel : PageModel
    {
        [TempData]
        public string StatusMessage { get; set; }


        public ResetAuthenticatorModel(SignInManager<P24IdentityUser> _signInManager, UserManager<P24IdentityUser> _userManager, ILogger<ResetAuthenticatorModel> _logger)
        {
            m_UserManager = _userManager;
            m_SignInManager = _signInManager;
            m_Logger = _logger;
        }


        public async Task<IActionResult> OnGet()
        {
            return NotFound();

            var user = await m_UserManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{m_UserManager.GetUserId(User)}'.");
            }

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

            await m_UserManager.SetTwoFactorEnabledAsync(user, false);
            await m_UserManager.ResetAuthenticatorKeyAsync(user);
            var userId = await m_UserManager.GetUserIdAsync(user);
            m_Logger.LogInformation("User with ID '{UserId}' has reset their authentication app key.", user.Id);

            await m_SignInManager.RefreshSignInAsync(user);
            StatusMessage = "Your authenticator app key has been reset, you will need to configure your authenticator app using the new key.";

            return RedirectToPage("./EnableAuthenticator");
        }


        private readonly SignInManager<P24IdentityUser> m_SignInManager;
        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly ILogger<ResetAuthenticatorModel> m_Logger;
    }

}

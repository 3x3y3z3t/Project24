/*  Areas/Identity/Pages/Account/Manage/Disable2fa.cshtml.cs
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

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.Model.Identity;

namespace Project24.Areas.Identity.Pages.Account.Manage
{
    public class Disable2faModel : PageModel
    {
        [TempData]
        public string StatusMessage { get; set; }


        public Disable2faModel(UserManager<P24IdentityUser> _userManager, ILogger<Disable2faModel> _logger)
        {
            m_UserManager = _userManager;
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

            if (!await m_UserManager.GetTwoFactorEnabledAsync(user))
            {
                throw new InvalidOperationException($"Cannot disable 2FA for user as it's not currently enabled.");
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

            var disable2faResult = await m_UserManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred disabling 2FA.");
            }

            m_Logger.LogInformation("User with ID '{UserId}' has disabled 2fa.", m_UserManager.GetUserId(User));
            StatusMessage = "2fa has been disabled. You can reenable 2fa when you setup an authenticator app";
            return RedirectToPage("./TwoFactorAuthentication");
        }


        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly ILogger<Disable2faModel> m_Logger;
    }

}
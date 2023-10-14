/*  Areas/Identity/Pages/Account/Manage/GenerateRecoveryCodes.cshtml.cs
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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.Model.Identity;

namespace Project24.Areas.Identity.Pages.Account.Manage
{
    public class GenerateRecoveryCodesModel : PageModel
    {
        [TempData]
        public string[] RecoveryCodes { get; set; }

        [TempData]
        public string StatusMessage { get; set; }


        public GenerateRecoveryCodesModel(UserManager<P24IdentityUser> _userManager, ILogger<GenerateRecoveryCodesModel> _logger)
        {
            m_UserManager = _userManager;
            m_Logger = _logger;
        }


        public async Task<IActionResult> OnGetAsync()
        {
            return NotFound();

            var user = await m_UserManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{m_UserManager.GetUserId(User)}'.");
            }

            var isTwoFactorEnabled = await m_UserManager.GetTwoFactorEnabledAsync(user);
            if (!isTwoFactorEnabled)
            {
                throw new InvalidOperationException($"Cannot generate recovery codes for user because they do not have 2FA enabled.");
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

            var isTwoFactorEnabled = await m_UserManager.GetTwoFactorEnabledAsync(user);
            var userId = await m_UserManager.GetUserIdAsync(user);
            if (!isTwoFactorEnabled)
            {
                throw new InvalidOperationException($"Cannot generate recovery codes for user as they do not have 2FA enabled.");
            }

            var recoveryCodes = await m_UserManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            RecoveryCodes = recoveryCodes.ToArray();

            m_Logger.LogInformation("User with ID '{UserId}' has generated new 2FA recovery codes.", userId);
            StatusMessage = "You have generated new recovery codes.";
            return RedirectToPage("./ShowRecoveryCodes");
        }


        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly ILogger<GenerateRecoveryCodesModel> m_Logger;
    }

}
/*  Areas/Identity/Pages/Account/Logout.cshtml.cs
 *  Version: v1.1 (2023.12.26)
 *  Spec:    v0.1
 *  
 *  Contributor
 *      The .NET Foundation (Author)
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
using Project24.App.Utils;
using Project24.Model.Identity;

namespace Project24.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        public LogoutModel(SignInManager<P24IdentityUser> _signInManager, ILogger<LogoutModel> _logger)
        {
            m_SignInManager = _signInManager;
            m_Logger = _logger;
        }


        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            string username = User.Identity?.Name;
            await m_SignInManager.SignOutAsync();

            m_Logger.LogInformation("User '{_username}' logged out.", username);

            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // This needs to be a redirect so that the browser performs a new
                // request and the identity for the user gets updated.
                //return RedirectToPage();
                return Redirect(PageCollection.IdentityAccount.Login);
            }
        }


        private readonly SignInManager<P24IdentityUser> m_SignInManager;
        private readonly ILogger<LogoutModel> m_Logger;
    }

}

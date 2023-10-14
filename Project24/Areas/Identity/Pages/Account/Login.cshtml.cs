/*  Areas/Identity/Pages/Account/Login.cshtml.cs
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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.Model.Identity;
using Project24.App;
using Project24.Data;
using Project24.App.Services;
using Project24.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Project24.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        public class InputModel
        {
            [Required]
            public string Username { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            public bool RememberMe { get; set; }
        }


        [BindProperty]
        public InputModel Input { get; set; }

        public string StatusMessage { get; set; }
        public string ReturnUrl { get; set; }


        public LoginModel(IOptionsMonitor<RequestLocalizationOptions> _optMonitor, LocalizationSvc _localizationSvc, SignInManager<P24IdentityUser> _signInManager, ApplicationDbContext _dbContext, ILogger<LoginModel> _logger)
        {
            

            m_LocalizationSvc = _localizationSvc;

            m_SignInManager = _signInManager;
            m_DbContext = _dbContext;
            m_Logger = _logger;
        }


        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            if (returnUrl == null)
                returnUrl =  Url.Content("~/");

            if (m_SignInManager.IsSignedIn(User))
                return LocalRedirect(returnUrl);
            
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ReturnUrl = returnUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                m_DbContext.RecordUserAction(Input.Username, "Attempt Login", "Failed",
                    new Dictionary<string, string>() { { CustomInfoKeys.Error, Message.InvalidModelState} });

                return Page();
            }

            if (returnUrl == null)
                returnUrl = Url.Content("~/");

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await m_SignInManager.PasswordSignInAsync(Input.Username, Input.Password, Input.RememberMe, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                StatusMessage = MessageTag.Error + m_LocalizationSvc[LOCL.DESC_IDENTITY_ACCOUNT_LOGIN_FAILED];

                m_DbContext.RecordUserAction(Input.Username, UserAction.Operation_.Account_AttemptPasswordLogin, UserAction.OperationStatus_.Failed);
                m_Logger.LogInformation("User '{_username}' login failed.", Input.Username);

                return Page();
            }

            m_DbContext.RecordUserAction(Input.Username, UserAction.Operation_.Account_AttemptPasswordLogin, UserAction.OperationStatus_.Success);
            m_Logger.LogInformation("User '{_username}' logged in.", Input.Username);

            return LocalRedirect(returnUrl);
        }

        private readonly LocalizationSvc m_LocalizationSvc;

        private readonly SignInManager<P24IdentityUser> m_SignInManager;
        private readonly ApplicationDbContext m_DbContext;
        private readonly ILogger<LoginModel> m_Logger;
    }

}

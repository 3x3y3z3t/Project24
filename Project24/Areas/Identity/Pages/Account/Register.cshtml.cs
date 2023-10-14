/*  Areas/Identity/Pages/Account/Register.cshtml.cs
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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Project24.Model.Identity;

namespace Project24.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }


        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }


        public RegisterModel(
            UserManager<P24IdentityUser> _userManager,
            SignInManager<P24IdentityUser> _signInManager,
            IEmailSender _emailSender,
            IUserStore<P24IdentityUser> _userStore,
            ILogger<RegisterModel> _logger)
        {
            m_UserManager = _userManager;
            m_SignInManager = _signInManager;
            m_EmailSender = _emailSender;
            m_UserStore = _userStore;
            m_EmailStore = GetEmailStore();
            m_Logger = _logger;
        }


        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            return NotFound();

            ReturnUrl = returnUrl;
            ExternalLogins = (await m_SignInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            return NotFound();

            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await m_SignInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await m_UserStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await m_EmailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await m_UserManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    m_Logger.LogInformation("User created a new account with password.");

                    var userId = await m_UserManager.GetUserIdAsync(user);
                    var code = await m_UserManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await m_EmailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (m_UserManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await m_SignInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private P24IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<P24IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(P24IdentityUser)}'. " +
                    $"Ensure that '{nameof(P24IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<P24IdentityUser> GetEmailStore()
        {
            if (!m_UserManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<P24IdentityUser>)m_UserStore;
        }


        private readonly SignInManager<P24IdentityUser> m_SignInManager;
        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly IEmailSender m_EmailSender;
        private readonly IUserStore<P24IdentityUser> m_UserStore;
        private readonly IUserEmailStore<P24IdentityUser> m_EmailStore;
        private readonly ILogger<RegisterModel> m_Logger;
    }

}

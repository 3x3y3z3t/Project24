/*  Areas/Identity/Pages/Account/Manage/EnableAuthenticator.cshtml.cs
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
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.Model.Identity;

namespace Project24.Areas.Identity.Pages.Account.Manage
{
    public class EnableAuthenticatorModel : PageModel
    {
        public class InputModel
        {
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Verification Code")]
            public string Code { get; set; }
        }


        public string SharedKey { get; set; }

        public string AuthenticatorUri { get; set; }

        [TempData]
        public string[] RecoveryCodes { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }


        public EnableAuthenticatorModel(UserManager<P24IdentityUser> _userManager, ILogger<EnableAuthenticatorModel> _logger, UrlEncoder _urlEncoder)
        {
            m_UserManager = _userManager;
            m_Logger = _logger;
            m_UrlEncoder = _urlEncoder;
        }


        public async Task<IActionResult> OnGetAsync()
        {
            return NotFound();

            var user = await m_UserManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{m_UserManager.GetUserId(User)}'.");
            }

            await LoadSharedKeyAndQrCodeUriAsync(user);

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

            if (!ModelState.IsValid)
            {
                await LoadSharedKeyAndQrCodeUriAsync(user);
                return Page();
            }

            // Strip spaces and hyphens
            var verificationCode = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await m_UserManager.VerifyTwoFactorTokenAsync(
                user, m_UserManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                ModelState.AddModelError("Input.Code", "Verification code is invalid.");
                await LoadSharedKeyAndQrCodeUriAsync(user);
                return Page();
            }

            await m_UserManager.SetTwoFactorEnabledAsync(user, true);
            var userId = await m_UserManager.GetUserIdAsync(user);
            m_Logger.LogInformation("User with ID '{UserId}' has enabled 2FA with an authenticator app.", userId);

            StatusMessage = "Your authenticator app has been verified.";

            if (await m_UserManager.CountRecoveryCodesAsync(user) == 0)
            {
                var recoveryCodes = await m_UserManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                RecoveryCodes = recoveryCodes.ToArray();
                return RedirectToPage("./ShowRecoveryCodes");
            }
            else
            {
                return RedirectToPage("./TwoFactorAuthentication");
            }
        }

        private async Task LoadSharedKeyAndQrCodeUriAsync(P24IdentityUser _user)
        {
            // Load the authenticator key & QR code URI to display on the form
            var unformattedKey = await m_UserManager.GetAuthenticatorKeyAsync(_user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await m_UserManager.ResetAuthenticatorKeyAsync(_user);
                unformattedKey = await m_UserManager.GetAuthenticatorKeyAsync(_user);
            }

            SharedKey = FormatKey(unformattedKey);

            var email = await m_UserManager.GetEmailAsync(_user);
            AuthenticatorUri = GenerateQrCodeUri(email, unformattedKey);
        }

        private string GenerateQrCodeUri(string _email, string _unformattedKey)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                AuthenticatorUriFormat,
                m_UrlEncoder.Encode("Microsoft.AspNetCore.Identity.UI"),
                m_UrlEncoder.Encode(_email),
                _unformattedKey);
        }

        private static string FormatKey(string _unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < _unformattedKey.Length)
            {
                result.Append(_unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
                currentPosition += 4;
            }
            if (currentPosition < _unformattedKey.Length)
            {
                result.Append(_unformattedKey.AsSpan(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }


        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly ILogger<EnableAuthenticatorModel> m_Logger;
        private readonly UrlEncoder m_UrlEncoder;
    }

}

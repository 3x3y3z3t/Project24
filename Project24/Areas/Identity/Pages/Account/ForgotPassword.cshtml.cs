/*  Areas/Identity/Pages/Account/ForgotPassword.cshtml.cs
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

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Project24.Model.Identity;

namespace Project24.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }


        [BindProperty]
        public InputModel Input { get; set; }


        public ForgotPasswordModel(UserManager<P24IdentityUser> _userManager, IEmailSender _emailSender)
        {
            m_UserManager = _userManager;
            m_EmailSender = _emailSender;
        }


        public async Task<IActionResult> OnPostAsync()
        {
            return NotFound();

            if (ModelState.IsValid)
            {
                var user = await m_UserManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await m_UserManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await m_UserManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                await m_EmailSender.SendEmailAsync(
                    Input.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }


        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly IEmailSender m_EmailSender;

    }

}

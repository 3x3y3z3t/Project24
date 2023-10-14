/*  Areas/Identity/Pages/Account/ConfirmEmailChange.cshtml.cs
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

using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Project24.Model.Identity;

namespace Project24.Areas.Identity.Pages.Account
{
    public class ConfirmEmailChangeModel : PageModel
    {
        [TempData]
        public string StatusMessage { get; set; }

        public ConfirmEmailChangeModel(UserManager<P24IdentityUser> _userManager, SignInManager<P24IdentityUser> _signInManager)
        {
            m_SignInManager = _signInManager;
            m_UserManager = _userManager;
        }


        public async Task<IActionResult> OnGetAsync(string userId, string email, string code)
        {
            return NotFound();

            if (userId == null || email == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await m_UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await m_UserManager.ChangeEmailAsync(user, email, code);
            if (!result.Succeeded)
            {
                StatusMessage = "Error changing email.";
                return Page();
            }

            // In our UI email and user name are one and the same, so when we update the email
            // we need to update the user name.
            var setUserNameResult = await m_UserManager.SetUserNameAsync(user, email);
            if (!setUserNameResult.Succeeded)
            {
                StatusMessage = "Error changing user name.";
                return Page();
            }

            await m_SignInManager.RefreshSignInAsync(user);
            StatusMessage = "Thank you for confirming your email change.";
            return Page();
        }


        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly SignInManager<P24IdentityUser> m_SignInManager;
    }

}

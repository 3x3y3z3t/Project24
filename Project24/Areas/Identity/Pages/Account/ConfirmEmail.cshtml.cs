/*  Areas/Identity/Pages/Account/ConfirmEmail.cshtml.cs
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
    public class ConfirmEmailModel : PageModel
    {
        public ConfirmEmailModel(UserManager<P24IdentityUser> _userManager)
        {
            m_UserManager = _userManager;
        }


        [TempData]
        public string StatusMessage { get; set; }
        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            return NotFound();

            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await m_UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await m_UserManager.ConfirmEmailAsync(user, code);
            StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
            return Page();
        }


        private readonly UserManager<P24IdentityUser> m_UserManager;
    }

}

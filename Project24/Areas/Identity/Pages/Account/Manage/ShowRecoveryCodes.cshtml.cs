/*  Identity/Account/Manage/ShowRecoveryCodes.cshtml.cs
 *  Version: 1.0 (2022.12.11)
 *
 *  Contributor
 *      Arime-chan
 */

#pragma warning disable CS0162 // Unreachable code detected

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Project24.Areas.Identity.Pages.Account.Manage
{
    public class ShowRecoveryCodesModel : PageModel
    {
        [TempData]
        public string[] RecoveryCodes { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public IActionResult OnGet()
        {
            return NotFound();

            if (RecoveryCodes == null || RecoveryCodes.Length == 0)
            {
                return RedirectToPage("./TwoFactorAuthentication");
            }

            return Page();
        }
    }
}

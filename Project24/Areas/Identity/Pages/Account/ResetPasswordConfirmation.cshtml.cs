/*  Identity/Account/ResetPasswordConfirmation.cshtml.cs
 *  Version: 1.0 (2022.12.11)
 *
 *  Contributor
 *      Arime-chan
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Project24.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResetPasswordConfirmationModel : PageModel
    {
        public IActionResult OnGet()
        {
            return NotFound();
        }
    }
}

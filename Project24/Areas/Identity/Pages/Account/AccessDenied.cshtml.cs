/*  Identity/Account/AccessDenied.cshtml.cs
 *  Version: 1.0 (2022.12.11)
 *
 *  Contributor
 *      Arime-chan
 */

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Project24.Areas.Identity.Pages.Account
{
    public class AccessDeniedModel : PageModel
    {
        public string ReturnUrl { get; private set; }

        public void OnGet(string returnUrl = null)
        {
            if (returnUrl == null)
                returnUrl = Url.Content("~/");

            ReturnUrl = returnUrl;
        }
    }

}

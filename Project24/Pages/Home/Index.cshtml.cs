/*  Index.cshtml.cs
 *  Version: 1.3 (2022.11.28)
 *
 *  Contributor
 *      Arime-chan
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Project24.Pages.Home
{
    [AllowAnonymous]
    public partial class IndexModel : PageModel
    {
        public IndexModel(ILogger<IndexModel> _logger)
        {


        }

        public IActionResult OnGet()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToPage("Login");
            }

            bool isCMSide = HttpContext.User.IsInRole(P24RoleName.Manager);
            
            bool isNasSide = HttpContext.User.IsInRole(P24RoleName.NasUser)
                || HttpContext.User.IsInRole(P24RoleName.NasTester);

            if (isCMSide && isNasSide)
            {
                return Page();
            }

            if (isCMSide)
            {
                return RedirectToPage("../ClinicManager/Customer/List");
            }

            if (isNasSide)
            {
                return RedirectToPage("../Nas/Index");
                return RedirectToPage("../Nas/Upload");
            }

            return NotFound(); // TODO: maybe redirect to access denied page;
        }
    }

}

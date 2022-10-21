/*  Index.cshtml.cs
 *  Version: 1.2 (2022.10.21)
 *
 *  Contributor
 *      Arime-chan
 */

using System.Threading.Tasks;
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

        public async Task<IActionResult> OnGetAsync()
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
                return RedirectToPage("../ClinicManager/Index");
            }

            if (isNasSide)
            {
                return RedirectToPage("../Nas/Index");
            }

            return NotFound(); // TODO: maybe redirect to access denied page;
        }
    }

}

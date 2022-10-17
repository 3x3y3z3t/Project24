/*  Index.cshtml.cs
 *  Version: 1.1 (2022.10.10)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Linq;
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

            if (HttpContext.User.IsInRole(Constants.ROLE_ADMIN))
            {
                return RedirectToPage("../Nas/Upload");
                //return RedirectToPage("../ClinicManager/Index");
                //return RedirectToPage("About");
                //return Partial("_Navigator");
            }

            if (HttpContext.User.IsInRole(Constants.ROLE_NAS_USER))
            {
                return RedirectToPage("../Nas/Index");
            }

            if (HttpContext.User.IsInRole(Constants.ROLE_MANAGER))
            {
                return RedirectToPage("../ClinicManager/Index");
            }

            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                //return await OnPostAsync_Login();
            }





            return BadRequest();

        }






    }

}

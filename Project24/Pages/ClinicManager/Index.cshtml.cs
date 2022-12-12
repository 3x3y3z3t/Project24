/*  Index.cshtml.cs
 *  Version: 1.4 (2022.12.04)
 *
 *  Contributor
 *      Arime-chan
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Project24.Pages.ClinicManager
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            return RedirectToPage("Customer/List");
        }
    }

}

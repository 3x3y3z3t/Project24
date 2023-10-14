/*  Home/Index.cshtml.cs
 *  Version: v1.1 (2023.10.15)
 *  
 *  Author
 *      Arime-chan
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project24.App.Utils;

namespace Project24.Pages.Home
{
    [Authorize(Roles = PageCollection.Home.Index)]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }

}

/*  Index.cshtml.cs
 *  Version: v1.1 (2023.10.29)
 *  
 *  Author
 *      Arime-chan
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Project24.Pages
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        public IndexModel(ILogger<IndexModel> _logger)
        {
            m_Logger = _logger;
        }


        public IActionResult OnGet()
        {
            return Page();
        }


        private readonly ILogger<IndexModel> m_Logger;
    }

}

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
using Project24.App.Utils;

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
            return Redirect(PageCollection.IdentityAccount.Manage.ChangePassword);
            return Redirect(PageCollection.IdentityAccount.Manage.Index);
            return RedirectToPage(PageCollection.Home.Account.List);
            return RedirectToPage(PageCollection.Home.Account.Create);
            return RedirectToPage(PageCollection.Home.Account.Manage, new { _id = "e677bd02-fd0e-45f7-a054-9432217c766f" });
            return RedirectToPage(PageCollection.Simulator.FinancialManagement.List);
            return RedirectToPage(PageCollection.Simulator.FinancialManagement.Create);

            return LocalRedirect("/Identity/Error");
            return RedirectToPage(PageCollection.HOME_MANAGEMENT_CONFIG_PANEL);
            return RedirectToPage("Identity/Account/Logout");
            return RedirectToPage("Identity/Account/Login");
            return RedirectToPage(PageCollection.PAGE_SIMULATOR_FINANCIAL_MANAGEMENT_LIST);
            return RedirectToPage(PageCollection.PAGE_HOME_MANAGEMENT_UPDATER);
        }


        private readonly ILogger<IndexModel> m_Logger;
    }

}

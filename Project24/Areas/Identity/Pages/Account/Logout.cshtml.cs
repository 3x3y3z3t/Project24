/*  Identity/Account/Logout.cshtml.cs
 *  Version: 1.0 (2022.12.11)
 *
 *  Contributor
 *      Arime-chan
 */

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.Models.Identity;

namespace Project24.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        public LogoutModel(SignInManager<P24IdentityUser> _signInManager, ILogger<LogoutModel> _logger)
        {
            m_SignInManager = _signInManager;
            m_Logger = _logger;
        }


        public IActionResult OnGet(string returnUrl = null)
        {
            if (returnUrl == null)
                return BadRequest();

            return LocalRedirect(returnUrl);
        }

        public async Task OnPostAsync()
        {
            await m_SignInManager.SignOutAsync();

            //await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            //await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            //await HttpContext.SignOutAsync(IdentityConstants.TwoFactorRememberMeScheme);
            //await HttpContext.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);

            m_Logger.LogInformation("An user logged out.");
        }


        private readonly SignInManager<P24IdentityUser> m_SignInManager;
        private readonly ILogger<LogoutModel> m_Logger;
    }

}

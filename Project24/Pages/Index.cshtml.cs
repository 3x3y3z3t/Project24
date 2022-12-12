/*  Updater.cshtml.cs
 *  Version: 1.0 (2022.12.12)
 *
 *  Contributor
 *      Arime-chan
 */

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project24.App.Utils;
using Project24.Models.Identity;

namespace Project24.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        //public string StatusMessage { get; private set; }

        public bool IsAdmin { get; private set; }


        public IndexModel(UserManager<P24IdentityUser> _userManager)
        {
            m_UserManager = _userManager;


        }


        public async Task<IActionResult> OnGetAsync()
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            var roles = await m_UserManager.GetRolesAsync(currentUser);

            var access = P24RoleClaimUtils.GetHighestAccessAllowance(roles);

            IsAdmin = access.DashboardAccess != P24RoleClaimUtils.AccessAllowance.NoAccess;
            bool isCMSide = access.ClinicManagerAccess != P24RoleClaimUtils.AccessAllowance.NoAccess;
            bool isNasSide = access.NasAccess != P24RoleClaimUtils.AccessAllowance.NoAccess;

            if (isCMSide && isNasSide)
            {
                return Page();
            }

            if (isCMSide)
            {
                return RedirectToPage("ClinicManager/Index");
            }

            if (isNasSide)
            {
                return RedirectToPage("Nas/Index");
            }

            return BadRequest();
        }


        private readonly UserManager<P24IdentityUser> m_UserManager;
    }

}

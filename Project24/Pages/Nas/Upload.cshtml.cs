/*  Upload.cshtml.cs
 *  Version: 1.0 (2022.10.16)
 *
 *  Contributor
 *      Arime-chan
 */

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project24.Data;
using Project24.Identity;

namespace Project24.Pages.Nas
{
    [Authorize(Roles = P24RoleName.NasUser + "," + P24RoleName.NasTester)]
    public class UploadModel : PageModel
    {
        public UploadModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
        }

        public async Task OnGetAsync()
        {

        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
    }

}

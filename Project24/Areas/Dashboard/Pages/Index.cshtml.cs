/*  Index.cshtml.cs
 *  Version: 1.0 (2022.09.06)
 *
 *  Contributor
 *      Arime-chan
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Project24.Areas.Dashboard.Pages
{
    [Authorize(Roles = Constants.ROLE_EMPLOYEE)]
    public class IndexModel : PageModel
    {
        public async Task OnGetAsync()
        {




        }
    }
}

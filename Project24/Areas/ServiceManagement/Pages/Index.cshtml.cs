/*  Index.cshtml.cs
 *  Version: 1.0 (2022.09.08)
 *
 *  Contributor
 *      Arime-chan
 */
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project24.Data;
using Project24.Models;

namespace Project24.Areas.ServiceManagement.Pages
{
    [Authorize(Roles = Constants.ROLE_MANAGER)]
    public class IndexModel : PageModel
    {
        public class DataModel
        {
            public List<ServiceDev> Services { get; set; }

            public DataModel()
            { }
        }

        [BindProperty]
        public DataModel Data { get; private set; }

        public IndexModel(ApplicationDbContext _context, ILogger<IndexModel> _logger)
        {
            m_DbContext = _context;
            m_Logger = _logger;
        }

        public async Task OnGetAsync()
        {
            Data = new DataModel
            {
                Services = await m_DbContext.ServicesDev.ToListAsync()
            };
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly ILogger<IndexModel> m_Logger;
    }

}

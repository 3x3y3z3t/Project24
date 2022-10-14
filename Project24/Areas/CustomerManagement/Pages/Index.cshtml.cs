/*  Index.cshtml.cs
 *  Version: 1.0 (2022.09.09)
 *
 *  Contributor
 *      Arime-chan
 */
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project24.Data;
using Project24.Identity;
using Project24.Models;

namespace Project24.Pages.CútomerManagement
{
    [Authorize(Roles = Constants.ROLE_EMPLOYEE)]
    public class IndexModel : PageModel
    {
        public class DataModel
        {
            public List<CustomerProfileDev> Customers { get; set; }

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
            Data = new DataModel()
            {
                Customers = await m_DbContext.CustomerProfilesDev.ToListAsync()
            };
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly ILogger<IndexModel> m_Logger;
    }

}

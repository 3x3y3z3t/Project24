/*  Detail.cshtml.cs
 *  Version: 1.0 (2022.09.08)
 *
 *  Contributor
 *      Arime-chan
 */
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.Data;
using Project24.Models;

namespace Project24.Areas.ServiceManagement.Pages
{
    [Authorize(Roles = Constants.ROLE_MANAGER)]
    public class DetailModel : PageModel
    {
        public class DataModel
        {
            public ServiceDev Service { get; set; }

            public DataModel()
            { }
        }

        [BindProperty]
        [ReadOnly(true)]
        public DataModel Data { get; set; }

        public string StatusMessage { get; set; }

        public DetailModel(ApplicationDbContext _context, ILogger<CreateModel> _logger)
        {
            m_DbContext = _context;
            m_Logger = _logger;
        }

        public async Task OnGetAsync(int _id)
        {
            ServiceDev service = await m_DbContext.ServicesDev.FindAsync(_id);
            if (service == null)
            {
                StatusMessage = "Error: " + Constants.ERROR_NOT_FOUND_SERVICE;
            }
            else
            {
                Data = new DataModel()
                {
                    Service = service
                };
            }

        }

        private readonly ApplicationDbContext m_DbContext;

        private readonly ILogger<CreateModel> m_Logger;
    }

}

/*  Index.cshtml.cs
 *  Version: 1.0 (2022.09.11)
 *
 *  Contributor
 *      Arime-chan
 */
using System;
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

namespace Project24.Pages.VisitingManagement
{
    [Authorize(Roles = Constants.ROLE_EMPLOYEE)]
    public class IndexModel : PageModel
    {
        public class DataModel
        {
            public class VisitingProfileViewModel
            {
                public int Id { get; set; }
                public string CustomerDetails { get; set; }
                public string ServicesUser { get; set; }

                public VisitingProfileViewModel()
                { }
            }

            public List<VisitingProfileViewModel> VisitingsToday { get; set; }

            public List<VisitingProfileViewModel> VisitingsYesterday { get; set; }

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
            var today = from _profile in m_DbContext.VisitingProfilesDev
                            //join _customer in m_DbContext.CustomersDev on _profile.CustomerId equals _customer.Id
                        where _profile.CheckInDateTime.Date == DateTime.Now.Date
                        select new DataModel.VisitingProfileViewModel()
                        {
                            Id = _profile.Id,
                            CustomerDetails = "[" + _profile.Customer.CustomerCode + "]" + _profile.Customer.FullName,
                            ServicesUser = "Un-set"
                        };

            var yesterday = from _profile in m_DbContext.VisitingProfilesDev
                            where _profile.CheckInDateTime.Date.AddDays(1.0) == DateTime.Now.Date
                            select new DataModel.VisitingProfileViewModel()
                            {
                                Id = _profile.Id,
                                CustomerDetails = "[" + _profile.Customer.CustomerCode + "]" + _profile.Customer.FullName,
                                ServicesUser = "Un-set"
                            };

            Data = new DataModel()
            {
                VisitingsToday = await today.ToListAsync(),
                VisitingsYesterday = await yesterday.ToListAsync()
            };
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly ILogger<IndexModel> m_Logger;
    }

}

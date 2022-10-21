/*  Index.cshtml.cs
 *  Version: 1.3 (2022.10.21)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.Data;

namespace Project24.Pages.ClinicManager
{
    [Authorize(Roles = P24Roles.Manager)]
    public class IndexModel : PageModel
    {
        public class DataModel
        {
            public class CustomerViewModel
            {
                public string CustomerCode { get; set; }
                public string FullName { get; set; }
                public string Address { get; set; }
                public string PhoneNumber { get; set; }
                public DateTime LastUpdated { get; set; }
                public int ImageCount { get; set; }

                public CustomerViewModel()
                { }
            }

            public List<CustomerViewModel> Customers { get; set; }

            public DataModel()
            {
                Customers = new List<CustomerViewModel>();
            }
        }

        [BindProperty]
        public DataModel Data { get; private set; }

        public string CustomerCount { get; private set; } = "0";


        public IndexModel(ApplicationDbContext _context, ILogger<IndexModel> _logger)
        {
            m_DbContext = _context;
            m_Logger = _logger;
        }


        // async this method will cause NRE since data won't make it to the client (customerViews.Count);
        public void OnGet()
        {
            Data = new DataModel();

            var customers = from _customers in m_DbContext.CustomerProfiles
                            where _customers.DeletedDate == DateTime.MinValue
                            select new DataModel.CustomerViewModel()
                            {
                                CustomerCode = _customers.CustomerCode,
                                FullName = _customers.FullName,
                                Address = _customers.Address,
                                PhoneNumber = _customers.PhoneNumber,
                                LastUpdated = _customers.UpdatedDate,
                                ImageCount = (from _images in m_DbContext.CustomerImages
                                              where _images.OwnedCustomerId == _customers.Id && _images.DeletedDate == DateTime.MinValue
                                              select _images.Id).Count()
                            };

            List<DataModel.CustomerViewModel> customerViews = customers.ToList();

            Data.Customers = customerViews;
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly ILogger<IndexModel> m_Logger;
    }

}

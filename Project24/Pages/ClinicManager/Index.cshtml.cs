/*  Index.cshtml.cs
 *  Version: 1.4 (2022.10.29)
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
using Microsoft.Extensions.Logging;
using Project24.Data;

namespace Project24.Pages.ClinicManager
{
    [Authorize(Roles = P24RoleName.Manager)]
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

        public DataModel Data { get; private set; }

        public bool IsSearchMode { get; private set; } = false;
        public string SearchName { get; private set; } = "";
        public string SearchPhone { get; private set; } = "";
        public string SearchAddress { get; private set; } = "";

        public string CustomerCount { get; private set; } = "0";


        public IndexModel(ApplicationDbContext _context, ILogger<IndexModel> _logger)
        {
            m_DbContext = _context;
            m_Logger = _logger;
        }


        // async this method will cause NRE since data won't make it to the client (customerViews.Count);
        public IActionResult OnGet()
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

            return Page();
        }

        public async Task<IActionResult> OnGetSearchAsync(string _name, string _phone, string _address, string _date)
        {
            IsSearchMode = true;

            if (_name == null)
                _name = "";
            if (_phone == null)
                _phone = "";
            if (_address == null)
                _address = "";
            if (_date == null)
                _date = "";

            if (_name == "" && _phone == "" && _address == "" && _date == "")
                return OnGet();

            // NOTE: Search by Date didn't make it into this version;
            //IEnumerable<DataModel.CustomerViewModel> perfectHit;
            if (_date != "")
            {
                //perfectHit = from _customers in m_DbContext.CustomerProfiles
                //             where _customers.AddedDate
            }
            else
            {

            }


            var perfectHit = m_DbContext.CustomerProfiles
                            .ToList()
                            .Where(_customer => _customer.DeletedDate == DateTime.MinValue
                                && _customer.FullName.EndsWith(_name, StringComparison.OrdinalIgnoreCase)
                                && _customer.PhoneNumber.EndsWith(_phone)
                                && _customer.Address.Contains(_address, StringComparison.OrdinalIgnoreCase))
                            .Select(_customer => new DataModel.CustomerViewModel()
                            {
                                CustomerCode = _customer.CustomerCode,
                                FullName = _customer.FullName,
                                Address = _customer.Address,
                                PhoneNumber = _customer.PhoneNumber,
                                LastUpdated = _customer.UpdatedDate,
                                ImageCount = (from _images in m_DbContext.CustomerImages
                                              where _images.OwnedCustomerId == _customer.Id && _images.DeletedDate == DateTime.MinValue
                                              select _images.Id).Count()
                            });

            /*
            var nearHit = m_DbContext.CustomerProfiles
                            .ToList()
                            .Where(_customer => _customer.DeletedDate == DateTime.MinValue &&
                                (_customer.FullName.EndsWith(_name, StringComparison.OrdinalIgnoreCase)
                                || _customer.PhoneNumber.EndsWith(_phone)
                                || _customer.Address.Contains(_address, StringComparison.OrdinalIgnoreCase)))
                            .Select(_customer => new DataModel.CustomerViewModel()
                            {
                                CustomerCode = _customer.CustomerCode,
                                FullName = _customer.FullName,
                                Address = _customer.Address,
                                PhoneNumber = _customer.PhoneNumber,
                                LastUpdated = _customer.UpdatedDate,
                                ImageCount = (from _images in m_DbContext.CustomerImages
                                              where _images.OwnedCustomerId == _customer.Id && _images.DeletedDate == DateTime.MinValue
                                              select _images.Id).Count()
                            });
            */

            List<DataModel.CustomerViewModel> customerViews = perfectHit.ToList();
            //customerViews.AddRange(nearHit);

            Data = new DataModel()
            {
                Customers = customerViews
            };

            SearchName = _name;
            SearchPhone = _phone;
            SearchAddress = _address;

            return Page();
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly ILogger<IndexModel> m_Logger;
    }

}

/*  P24/Customer/List.cshtml
 *  Version: 1.1 (2022.12.04)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project24.App.Utils;
using Project24.Data;

namespace Project24.Pages.ClinicManager
{
    [Authorize(Roles = P24RoleName.Manager)]
    public class ListCustomerModel : PageModel
    {
        public class CustomerViewModel
        {
            public string Code { get; set; }

            public string Fullname { get; set; }

            public int DoB { get; set; }

            [DataType(DataType.PhoneNumber)]
            public string PhoneNumber { get; set; }

            [DataType(DataType.MultilineText)]
            public string Address { get; set; }

            [DataType(DataType.MultilineText)]
            public string Notes { get; set; }

            public int TicketCount { get; set; }

            public CustomerViewModel()
            { }
        }

        public class QuickSearchFormDataModel
        {
            public string Name { get; set; }

            public string PhoneNumber { get; set; }

            public string Address { get; set; }

            [DataType(DataType.Date)]
            public DateTime Date { get; set; }

            public QuickSearchFormDataModel()
            { }
        }

        public List<CustomerViewModel> Customers { get; private set; }

        public QuickSearchFormDataModel SearchFormData { get; private set; }

        public bool IsSearchMode { get; private set; } = false;


        public ListCustomerModel(ApplicationDbContext _context, ILogger<ListCustomerModel> _logger)
        {
            m_DbContext = _context;
            m_Logger = _logger;
        }


        public async Task OnGetAsync()
        {
            var customers = await (from _customer in m_DbContext.CustomerProfiles
                                   where _customer.DeletedDate == DateTime.MinValue
                                   select new CustomerViewModel()
                                   {
                                       Code = _customer.Code,
                                       Fullname = _customer.FullName,
                                       DoB = _customer.DateOfBirth,
                                       PhoneNumber = _customer.PhoneNumber,
                                       Address = _customer.Address,
                                       Notes = _customer.Notes,
                                       TicketCount = (from _ticket in m_DbContext.VisitingProfiles
                                                      where _ticket.CustomerId == _customer.Id && _ticket.DeletedDate == DateTime.MinValue
                                                      select _ticket.Id).Count()
                                   })
                            .ToListAsync();

            Customers = customers;

            SearchFormData = new QuickSearchFormDataModel();
        }

        public async Task OnGetSearchAsync(string _name, string _phone, string _addr, DateTime _date)
        {
            if (_name == null)
                _name = "";
            if (_phone == null)
                _phone = "";
            if (_addr == null)
                _addr = "";

            if (_name != "")
                _name = StringUtils.ToTitleCase(_name);

            var customers = await (from _ticket in m_DbContext.VisitingProfiles.Include(_t => _t.Customer)
                                   where _ticket.Customer.DeletedDate == DateTime.MinValue
                                       && (_date == DateTime.MinValue || _ticket.AddedDate.Date == _date.Date)
                                       && (_name == "" || _ticket.Customer.LastName == _name)
                                       && _ticket.Customer.PhoneNumber.EndsWith(_phone)
                                       && _ticket.Customer.Address.Contains(_addr)
                                   select new CustomerViewModel()
                                   {
                                       Code = _ticket.Customer.Code,
                                       Fullname = _ticket.Customer.FullName,
                                       DoB = _ticket.Customer.DateOfBirth,
                                       PhoneNumber = _ticket.Customer.PhoneNumber,
                                       Address = _ticket.Customer.Address,
                                       Notes = _ticket.Customer.Notes,
                                       TicketCount = (from _tk in m_DbContext.VisitingProfiles
                                                      where _tk.CustomerId == _ticket.Customer.Id && _ticket.DeletedDate == DateTime.MinValue
                                                      select _tk.Id).Count()
                                   })
                            .ToListAsync();

            Customers = customers;

            SearchFormData = new QuickSearchFormDataModel()
            {
                Name = _name,
                PhoneNumber = _phone,
                Address = _addr,
                Date = _date
            };

            IsSearchMode = true;
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly ILogger<ListCustomerModel> m_Logger;
    }

}

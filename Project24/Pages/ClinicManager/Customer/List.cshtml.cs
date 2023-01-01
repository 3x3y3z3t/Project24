/*  P24/Customer/List.cshtml
 *  Version: 1.4 (2023.01.02)
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
using Project24.App.Utils;
using Project24.Data;
using Project24.Models.ClinicManager.DataModel;

namespace Project24.Pages.ClinicManager.Customer
{
    [Authorize(Roles = P24RoleName.Manager)]
    public class ListModel : PageModel
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
            public string Note { get; set; }

            public int TicketCount { get; set; }

            public CustomerViewModel()
            { }
        }


        public List<CustomerViewModel> Customers { get; private set; }

        public QuickSearchFormDataModel SearchFormData { get; private set; }

        public bool IsSearchMode { get; private set; } = false;


        public ListModel(ApplicationDbContext _context)
        {
            m_DbContext = _context;
        }


        public async Task OnGetAsync()
        {
            var customers = await (from _customer in m_DbContext.CustomerProfiles.Include(_c => _c.VisitingTickets)
                                   where _customer.DeletedDate == DateTime.MinValue
                                   select new CustomerViewModel()
                                   {
                                       Code = _customer.Code,
                                       Fullname = _customer.FullName,
                                       DoB = _customer.DateOfBirth,
                                       PhoneNumber = _customer.PhoneNumber,
                                       Address = _customer.Address,
                                       Note = _customer.Note,
                                       TicketCount = _customer.VisitingTickets.Count()
                                   })
                            .ToListAsync();

            Customers = customers;

            SearchFormData = new QuickSearchFormDataModel();
        }

        public async Task OnGetSearchAsync(string _name, string _phone, string _addr, DateTime _startDate, DateTime _endDate)
        {
            if (_name == null)
                _name = "";
            if (_phone == null)
                _phone = "";
            if (_addr == null)
                _addr = "";
            if (_endDate == P24Constants.MinDate)
                _endDate = DateTime.Today;

            if (_name != "")
                _name = StringUtils.ToTitleCase(_name);

            var customers = await (from _ticket in m_DbContext.TicketProfiles.Include(_t => _t.Customer)
                                   where _ticket.Customer.DeletedDate == DateTime.MinValue
                                      && _ticket.AddedDate.Date > _startDate
                                      && _ticket.AddedDate.Date < _endDate
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
                                       Note = _ticket.Customer.Note,
                                       TicketCount = (from _tk in m_DbContext.TicketProfiles
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
                StartDate = _startDate,
                EndDate = _endDate
            };

            IsSearchMode = true;
        }


        private readonly ApplicationDbContext m_DbContext;
    }

}

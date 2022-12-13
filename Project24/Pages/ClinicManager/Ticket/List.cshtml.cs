/*  List.cshtml.cs
 *  Version: 1.2 (2022.12.13)
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
    public class ListTicketsModel : PageModel
    {
        public class TicketViewModel
        {
            public string Code { get; set; }

            public string Diagnose { get; set; }

            public string Treatment { get; set; }

            [DataType(DataType.MultilineText)]
            public string Notes { get; set; }

            public string CustomerCode { get; set; }

            public string CustomerFullName { get; set; }

            public int CustomerDoB { get; set; }

            public TicketViewModel()
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

        public List<TicketViewModel> Tickets { get; private set; }

        public QuickSearchFormDataModel SearchFormData { get; private set; }

        public bool IsSearchMode { get; private set; } = false;



        public ListTicketsModel(ApplicationDbContext _context, ILogger<ListTicketsModel> _logger)
        {
            m_DbContext = _context;
            m_Logger = _logger;
        }


        public async Task OnGetAsync()
        {
            var tickets = await (from _ticket in m_DbContext.TicketProfiles.Include(_t => _t.Customer)
                                 where _ticket.DeletedDate == DateTime.MinValue
                                 select new TicketViewModel()
                                 {
                                     Code = _ticket.Code,
                                     Diagnose = _ticket.Diagnose,
                                     Treatment = _ticket.ProposeTreatment,
                                     Notes = _ticket.Notes,
                                     CustomerCode = _ticket.Customer.Code,
                                     CustomerFullName = _ticket.Customer.FullName,
                                     CustomerDoB = _ticket.Customer.DateOfBirth,
                                 })
                          .ToListAsync();

            Tickets = tickets;
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

            var tickets = await (from _ticket in m_DbContext.TicketProfiles.Include(_t => _t.Customer)
                                   where _ticket.Customer.DeletedDate == DateTime.MinValue
                                       && (_date == DateTime.MinValue || _ticket.AddedDate.Date == _date.Date)
                                       && (_name == "" || _ticket.Customer.LastName == _name)
                                       && _ticket.Customer.PhoneNumber.EndsWith(_phone)
                                       && _ticket.Customer.Address.Contains(_addr)
                                   select new TicketViewModel()
                                   {
                                       Code = _ticket.Code,
                                       Diagnose = _ticket.Diagnose,
                                       Treatment = _ticket.ProposeTreatment,
                                       Notes = _ticket.Notes,
                                       CustomerCode = _ticket.Customer.Code,
                                       CustomerFullName = _ticket.Customer.FullName,
                                       CustomerDoB = _ticket.Customer.DateOfBirth,
                                   })
                            .ToListAsync();

            Tickets = tickets;

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
        private readonly ILogger<ListTicketsModel> m_Logger;
    }

}

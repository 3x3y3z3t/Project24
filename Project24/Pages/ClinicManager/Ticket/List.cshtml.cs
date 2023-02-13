/*  P24/Ticket/List.cshtml.cs
 *  Version: 1.6 (2023.02.11)
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project24.App.Utils;
using Project24.Data;
using Project24.Models.ClinicManager;
using Project24.Models.ClinicManager.DataModel;

namespace Project24.Pages.ClinicManager
{
    [Authorize(Roles = P24RoleName.Manager)]
    public class ListModel : PageModel
    {
        public class TicketViewModel
        {
            public string Code { get; set; }

            public string Diagnose { get; set; }

            public string Treatment { get; set; }

            public int? DrugExportBatchId { get; set; }

            [DataType(DataType.MultilineText)]
            public string Notes { get; set; }

            public string CustomerCode { get; set; }

            public string CustomerFullName { get; set; }

            public int CustomerDoB { get; set; }

            public TicketViewModel()
            { }
        }

        public List<TicketViewModel> Tickets { get; private set; }

        public QuickSearchFormDataModel SearchFormData { get; private set; }

        public bool IsSearchMode { get; private set; } = false;



        public ListModel(ApplicationDbContext _context)
        {
            m_DbContext = _context;
        }


        public async Task OnGetAsync()
        {
            var tickets = await (from _ticket in m_DbContext.TicketProfiles.Include(_t => _t.Customer).Include(_t => _t.DrugExportBatch)
                                 where _ticket.DeletedDate == DateTime.MinValue
                                 select new TicketViewModel()
                                 {
                                     Code = _ticket.Code,
                                     Diagnose = _ticket.Diagnose,
                                     Treatment = _ticket.ProposeTreatment,
                                     DrugExportBatchId = _ticket.DrugExportBatch.Id,
                                     Notes = _ticket.Note,
                                     CustomerCode = _ticket.Customer.Code,
                                     CustomerFullName = _ticket.Customer.FullName,
                                     CustomerDoB = _ticket.Customer.DateOfBirth,
                                 })
                          .ToListAsync();

            Tickets = tickets;

            SearchFormData = new QuickSearchFormDataModel();
        }

        public async Task<IActionResult> OnGetSearchAsync(string _code, string _name, string _phone, string _addr, DateTime _startDate, DateTime _endDate)
        {
            if (!string.IsNullOrEmpty(_code))
                return await SearchTicketByCustomerCode(_code);

            if (_name == null)
                _name = "";
            if (_phone == null)
                _phone = "";
            if (_addr == null)
                _addr = "";
            if (_endDate == P24Constants.MinDate || _endDate == DateTime.MinValue)
                _endDate = DateTime.Today;

            if (_name != "")
                _name = StringUtils.ToTitleCase(_name);

            return await SearchTicketByCompositeData(_name, _phone, _addr, _startDate, _endDate);
        }

        private async Task<IActionResult> SearchTicketByCustomerCode(string _code)
        {
            var customer = await (from _customer in m_DbContext.CustomerProfiles
                                  where _customer.Code == _code && _customer.DeletedDate == DateTime.MinValue
                                  select new
                                  {
                                      _customer.Id,
                                      _customer.Code,
                                      _customer.LastName,
                                      _customer.FullName,
                                      DoB = _customer.DateOfBirth,
                                      _customer.PhoneNumber,
                                      _customer.Address
                                  })
                           .FirstOrDefaultAsync();

            if (customer == null)
                return Partial("_CommonNotFound", new CommonNotFoundModel(P24Constants.Customer, _code, "ClinicManager/Ticket/List"));

            var tickets = await (from _ticket in m_DbContext.TicketProfiles.Include(_t => _t.Customer).Include(_t => _t.DrugExportBatch)
                                 where _ticket.CustomerId == customer.Id && _ticket.DeletedDate == DateTime.MinValue
                                 select new TicketViewModel()
                                 {
                                     Code = _ticket.Code,
                                     Diagnose = _ticket.Diagnose,
                                     Treatment = _ticket.ProposeTreatment,
                                     DrugExportBatchId = _ticket.DrugExportBatch.Id,
                                     Notes = _ticket.Note,
                                     CustomerCode = customer.Code,
                                     CustomerFullName = customer.FullName,
                                     CustomerDoB = customer.DoB
                                 })
                          .ToListAsync();

            Tickets = tickets;

            SearchFormData = new QuickSearchFormDataModel()
            {
                Name = customer.LastName,
                PhoneNumber = customer.PhoneNumber,
                Address = customer.Address,
            };

            return Page();
        }

        private async Task<IActionResult> SearchTicketByCompositeData(string _name, string _phone, string _addr, DateTime _startDate, DateTime _endDate)
        {
            var tickets = await (from _ticket in m_DbContext.TicketProfiles.Include(_t => _t.Customer).Include(_t => _t.DrugExportBatch)
                                 where _ticket.Customer.DeletedDate == DateTime.MinValue
                                    && _ticket.AddedDate.Date > _startDate
                                    && _ticket.AddedDate.Date < _endDate
                                    && (_name == "" || _ticket.Customer.LastName == _name)
                                    && _ticket.Customer.PhoneNumber.EndsWith(_phone)
                                    && _ticket.Customer.Address.Contains(_addr)
                                 select new TicketViewModel()
                                 {
                                     Code = _ticket.Code,
                                     Diagnose = _ticket.Diagnose,
                                     Treatment = _ticket.ProposeTreatment,
                                     DrugExportBatchId = _ticket.DrugExportBatch.Id,
                                     Notes = _ticket.Note,
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
                StartDate = _startDate,
                EndDate = _endDate
            };

            IsSearchMode = true;

            return Page();
        }


        private readonly ApplicationDbContext m_DbContext;
    }

}

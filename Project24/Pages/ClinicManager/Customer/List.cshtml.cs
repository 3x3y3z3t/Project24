/*  P24/Customer/List.cshtml
 *  Version: 1.0 (2022.11.20)
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
using Project24.Data;

namespace Project24.Pages.ClinicManager
{
    [Authorize(Roles = P24RoleName.Manager)]
    public class ListCustomerModel : PageModel
    {
        public class DataModel
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

            public DataModel()
            { }
        }

        public List<DataModel> Data { get; private set; }


        public ListCustomerModel(ApplicationDbContext _context, ILogger<ListCustomerModel> _logger)
        {
            m_DbContext = _context;
            m_Logger = _logger;

            Data = new List<DataModel>();
        }


        public async Task OnGetAsync()
        {
            var customers = from _customer in m_DbContext.CustomerProfiles
                            where _customer.DeletedDate == DateTime.MinValue
                            select new DataModel()
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
                            };

            Data = await customers.ToListAsync();
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly ILogger<ListCustomerModel> m_Logger;
    }

}

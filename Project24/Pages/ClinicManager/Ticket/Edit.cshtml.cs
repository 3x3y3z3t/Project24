/*  P24/Customer/Edit.cshtml
 *  Version: 1.2 (2022.12.04)
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
using Project24.App;
using Project24.Data;
using Project24.Models;
using Project24.Models.ClinicManager;
using Project24.Models.ClinicManager.DataModel;
using Project24.Models.Identity;

namespace Project24.Pages.ClinicManager.Ticket
{
    [Authorize(Roles = P24RoleName.Manager)]
    public class EditModel : PageModel
    {
        [BindProperty]
        public P24EditTicketFormDataModel FormData { get; set; }

        public P24CustomerDetailsViewModel Customer { get; set; }


        public EditModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager, ILogger<EditModel> _logger)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
            m_Logger = _logger;
        }


        public async Task<IActionResult> OnGetAsync(string _code)
        {
            if (string.IsNullOrEmpty(_code))
                return Partial("_CommonNotFound", new CommonNotFoundModel(P24Constants.Ticket, "null", "List"));

            var deleted = await (from _ticket in m_DbContext.VisitingProfiles
                                 where _ticket.Code == _code && _ticket.DeletedDate != DateTime.MinValue
                                 select _ticket.Code)
                          .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(deleted))
            {
                return RedirectToPage("Details", new { _code = _code });
            }

            var ticket = await (from _ticket in m_DbContext.VisitingProfiles
                                where _ticket.Code == _code
                                select new P24EditTicketFormDataModel()
                                {
                                    Code = _ticket.Code,
                                    Diagnose = _ticket.Diagnose,
                                    Treatment = _ticket.ProposeTreatment,
                                    Notes = _ticket.Notes
                                })
                         .FirstOrDefaultAsync();

            if (ticket == null)
                return Partial("_CommonNotFound", new CommonNotFoundModel(P24Constants.Ticket, _code, "List"));

            var customer = await (from _ticket in m_DbContext.VisitingProfiles.Include(_t => _t.Customer)
                                  where _ticket.Code == _code
                                  select new P24CustomerDetailsViewModel()
                                  {
                                      Code = _ticket.Customer.Code,
                                      Fullname = _ticket.Customer.FullName,
                                      Gender = AppUtils.NormalizeGenderString(_ticket.Customer.Gender),
                                      DoB = _ticket.Customer.DateOfBirth,
                                      PhoneNumber = _ticket.Customer.PhoneNumber,
                                      Address = _ticket.Customer.Address,
                                      Notes = _ticket.Customer.Notes
                                  })
                           .FirstOrDefaultAsync();

            if (customer == null)
                return Partial("_CommonNotFound", new CommonNotFoundModel(P24Constants.Ticket, "null", "List"));

            FormData = ticket;
            Customer = customer;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                await m_DbContext.RecordChanges(
                    currentUser.UserName,
                    ActionRecord.Operation_.UpdateTicket,
                    ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.Error, ErrorMessage.InvalidModelState }
                    }
                );

                return Page();
            }

            var ticket = await (from _ticket in m_DbContext.VisitingProfiles
                                where _ticket.Code == FormData.Code
                                select _ticket)
                           .FirstOrDefaultAsync();

            if (ticket == null)
                return BadRequest();

            if (ticket.DeletedDate != DateTime.MinValue)
            {
                await m_DbContext.RecordChanges(
                    currentUser.UserName,
                    ActionRecord.Operation_.UpdateTicket,
                    ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.TicketCode, FormData.Code },
                        { CustomInfoKey.Message, ErrorMessage.TicketDeleted},
                    }
                );

                return RedirectToPage("Details", new { _code = FormData.Code });
            }

            ticket.Diagnose = FormData.Diagnose;
            ticket.ProposeTreatment = FormData.Treatment;
            ticket.Notes = FormData.Notes;
            ticket.UpdatedDate = DateTime.Now;
            ticket.UpdatedUser = currentUser;

            m_DbContext.Update(ticket);

            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.UpdateTicket,
                ActionRecord.OperationStatus_.Success,
                new Dictionary<string, string>()
                {
                    { CustomInfoKey.TicketCode, FormData.Code }
                }
            );

            return RedirectToPage("Details", new { _code = FormData.Code });
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly ILogger<EditModel> m_Logger;
    }

}

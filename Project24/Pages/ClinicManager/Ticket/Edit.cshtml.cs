/*  P24/Ticket/Edit.cshtml
 *  Version: 1.6 (2023.01.03)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project24.App;
using Project24.App.Extension;
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


        public EditModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
        }


        public async Task<IActionResult> OnGetAsync(string _code)
        {
            if (string.IsNullOrEmpty(_code))
                return Partial("_CommonNotFound", new CommonNotFoundModel(P24Constants.Ticket, "null", "List"));

            var deleted = await (from _ticket in m_DbContext.TicketProfiles
                                 where _ticket.Code == _code && _ticket.DeletedDate != DateTime.MinValue
                                 select _ticket.Code)
                          .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(deleted))
                return RedirectToPage("Details", new { _code = _code });

            var ticket = await (from _ticket in m_DbContext.TicketProfiles
                                where _ticket.Code == _code
                                select new P24EditTicketFormDataModel()
                                {
                                    Code = _ticket.Code,
                                    Symptom = _ticket.Symptom,
                                    Diagnose = _ticket.Diagnose,
                                    Treatment = _ticket.ProposeTreatment,
                                    Note = _ticket.Note
                                })
                         .FirstOrDefaultAsync();

            if (ticket == null)
                return Partial("_CommonNotFound", new CommonNotFoundModel(P24Constants.Ticket, _code, "List"));

            var customer = await (from _ticket in m_DbContext.TicketProfiles.Include(_t => _t.Customer)
                                  where _ticket.Code == _code
                                  select new P24CustomerDetailsViewModel()
                                  {
                                      Code = _ticket.Customer.Code,
                                      Fullname = _ticket.Customer.FullName,
                                      Gender = AppUtils.NormalizeGenderString(_ticket.Customer.Gender),
                                      DoB = _ticket.Customer.DateOfBirth,
                                      PhoneNumber = _ticket.Customer.PhoneNumber,
                                      Address = _ticket.Customer.Address,
                                      Note = _ticket.Customer.Note
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
            if (!await this.ValidateModelState(m_DbContext, currentUser, ActionRecord.Operation_.UpdateTicket))
                return Page();

            var ticket = await (from _ticket in m_DbContext.TicketProfiles.Include(_t => _t.PreviousVersion)
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

            var jsonEncoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            string jsonData = JsonSerializer.Serialize(ticket, new JsonSerializerOptions() { Encoder = jsonEncoder });
            P24ObjectPreviousVersion previousVersion = new P24ObjectPreviousVersion(nameof(TicketProfile), ticket.Id.ToString(), jsonData, ticket.PreviousVersion);
            await m_DbContext.AddAsync(previousVersion);

            ticket.Symptom = FormData.Symptom;
            ticket.Diagnose = FormData.Diagnose;
            ticket.ProposeTreatment = FormData.Treatment;
            ticket.Note = FormData.Note;
            ticket.PreviousVersion = previousVersion;
            ticket.EditedDate = DateTime.Now;
            ticket.EditedUser = currentUser;

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
    }

}

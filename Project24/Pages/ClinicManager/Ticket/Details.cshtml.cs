/*  P24/Ticket/Details.cshtml.cs
 *  Version: 1.2 (2022.12.29)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
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
using Project24.Models.ClinicManager;
using Project24.Models.ClinicManager.DataModel;
using Project24.Models.Identity;

namespace Project24.Pages.ClinicManager.Ticket
{
    [Authorize(Roles = P24RoleName.Manager)]
    public class DetailsModel : PageModel
    {
        public P24TicketDetailsViewModelEx TicketViewData { get; private set; }

        public P24ImageListingModel ListImageModel { get; private set; }


        public DetailsModel(ApplicationDbContext _context)
        {
            m_DbContext = _context;
        }

        //https://eamonkeane.dev/how-to-view-sql-generated-by-entity-framework-core-using-logging/
        //https://eamonkeane.dev/3-ways-to-view-sql-generated-by-entity-framework-core-5/
        public async Task<IActionResult> OnGetAsync(string _code)
        {
            if (string.IsNullOrEmpty(_code))
                return Partial("_CommonNotFound", new CommonNotFoundModel(P24Constants.Ticket, "null", "List"));

            var ticket = await (from _ticket in m_DbContext.TicketProfiles.Include(_t => _t.Customer)
                                where _ticket.Code == _code
                                select new P24TicketDetailsViewModelEx()
                                {
                                    Code = _ticket.Code,
                                    Diagnose = _ticket.Diagnose,
                                    Treatment = _ticket.ProposeTreatment,
                                    Notes = _ticket.Note,
                                    AddedDate = _ticket.AddedDate,
                                    UpdatedDate = _ticket.EditedDate,
                                    DeletedDate = _ticket.DeletedDate,
                                    AddedUserName = _ticket.AddedUser.UserName,
                                    UpdatedUserName = _ticket.EditedUser.UserName,
                                    Customer = new P24CustomerDetailsViewModel()
                                    {
                                        Code = _ticket.Customer.Code,
                                        Fullname = _ticket.Customer.FullName,
                                        Gender = AppUtils.NormalizeGenderString(_ticket.Customer.Gender),
                                        DoB = _ticket.Customer.DateOfBirth,
                                        PhoneNumber = _ticket.Customer.PhoneNumber,
                                        Address = _ticket.Customer.Address,
                                        Note = _ticket.Customer.Note
                                    }
                                })
                         .FirstOrDefaultAsync();

            if (ticket == null)
                return Partial("_CommonNotFound", new CommonNotFoundModel(P24Constants.Ticket, _code, "List"));

            TicketViewData = ticket;

            var images = await (from _image in m_DbContext.TicketImages.Include(_i => _i.OwnerTicket)
                                where _image.OwnerTicket.Code == _code && _image.DeletedDate == DateTime.MinValue
                                select new P24ImageViewModel()
                                {
                                    Id = _image.Id,
                                    Path = _image.Path,
                                    Name = _image.Name
                                })
                         .ToListAsync();

            ListImageModel = new P24ImageListingModel()
            {
                Module = P24Module.Ticket,
                OwnerCode = _code,
                Images = images,
            };

            return Page();
        }


        private readonly ApplicationDbContext m_DbContext;
    }

}

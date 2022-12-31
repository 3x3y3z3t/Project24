/*  P24/Ticket/Delete.cshtml
 *  Version: 1.2 (2022.12.29)
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
using Project24.App;
using Project24.App.Extension;
using Project24.App.Services.P24ImageManager;
using Project24.Data;
using Project24.Models;
using Project24.Models.ClinicManager;
using Project24.Models.ClinicManager.DataModel;
using Project24.Models.Identity;

namespace Project24.Pages.ClinicManager.Ticket
{
    [Authorize(Roles = P24RoleName.Manager)]
    public class DeleteModel : PageModel
    {
        public string TicketCode { get; }

        public P24TicketDetailsViewModelEx TicketViewData { get; private set; }

        public P24ImageListingModel ListImageModel { get; private set; }


        public DeleteModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager, P24ImageManagerService _imageManagerSvc)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
            m_ImageManagerSvc = _imageManagerSvc;
        }


        public async Task<IActionResult> OnGetAsync(string _code)
        {
            if (string.IsNullOrEmpty(_code))
                return Partial("_CommonNotFound", new CommonNotFoundModel(P24Constants.Ticket, "null", "List"));

            var ticket = await (from _ticket in m_DbContext.TicketProfiles.Include(_t => _t.AddedUser).Include(_t => _t.EditedUser).Include(_t => _t.Customer)
                                where _ticket.Code == _code
                                select new P24TicketDetailsViewModelEx()
                                {
                                    Code = _code,
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

            ListImageModel = new P24ImageListingModel()
            {
                Module = P24Module.Ticket,
                OwnerCode = _code,
                IsReadonly = true,
                Images = await FetchImages(_code)
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync([Bind] string TicketCode)
        {
            if (!await ValidateModelState(ActionRecord.Operation_.DeleteTicket))
                return Page();

            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);

            var ticket = await (from _ticket in m_DbContext.TicketProfiles.Include(_t => _t.Customer).Include(_t => _t.TicketImages)
                                where _ticket.Code == TicketCode
                                select _ticket)
                           .FirstOrDefaultAsync();

            if (ticket == null)
                return BadRequest();

            ticket.DeletedDate = DateTime.Now;
            ticket.EditedUser = currentUser;
            m_DbContext.Update(ticket);

            Dictionary<string, string> customInfo = new Dictionary<string, string>()
            {
                { CustomInfoKey.TicketCode, ticket.Code },
                { CustomInfoKey.CustomerCode, ticket.Customer.Code },
            };

            var responseData = m_ImageManagerSvc.Delete(currentUser, ticket.TicketImages);
            if (responseData.IsSuccess)
            {
                customInfo.Add(CustomInfoKey.DeletedList, responseData.DeletedFileNames.Count.ToString());
                customInfo.Add(CustomInfoKey.Error, responseData.ErrorFileMessages.Count.ToString());
            }

            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.DeleteTicket,
                ActionRecord.OperationStatus_.Success,
                customInfo
            );

            return RedirectToPage("List");
        }

        // Ajax call only;
        public async Task<IActionResult> OnPostDeleteImageAsync([FromBody] string _imageId)
        {
            if (!await ValidateModelState(ActionRecord.Operation_.DeleteTicket_DeleteImage))
                return BadRequest();

            if (!int.TryParse(_imageId, out int imgId))
                return BadRequest();

            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);

            var image = await (from _image in m_DbContext.TicketImages.Include(_img => _img.OwnerTicket)
                               where _image.Id == imgId && _image.DeletedDate == DateTime.MinValue
                               select _image)
                         .FirstOrDefaultAsync();

            if (image == null)
                return BadRequest();

            Dictionary<string, string> customInfo = new Dictionary<string, string>()
            {
                { CustomInfoKey.TicketCode, image.OwnerTicket.Code }
            };

            var responseData = m_ImageManagerSvc.Delete(currentUser, image);
            if (responseData.IsSuccess)
            {
                customInfo.Add(CustomInfoKey.DeletedList, responseData.DeletedFileNames.Count.ToString());
                customInfo.Add(CustomInfoKey.Error, responseData.ErrorFileMessages.Count.ToString());
            }

            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.DeleteTicket_DeleteImage,
                ActionRecord.OperationStatus_.Success,
                customInfo
            );

            ListImageModel = new P24ImageListingModel()
            {
                Module = P24Module.Ticket,
                OwnerCode = image.OwnerTicket.Code,
                Images = await FetchImages(image.OwnerTicket.Code)
            };

            return Partial("_CommonListImage", ListImageModel);
        }

        private async Task<List<P24ImageViewModel>> FetchImages(string _ticketCode)
        {
            var images = from _image in m_DbContext.TicketImages.Include(_i => _i.OwnerTicket)
                         where _image.OwnerTicket.Code == _ticketCode && _image.DeletedDate == DateTime.MinValue
                         select new P24ImageViewModel()
                         {
                             Id = _image.Id,
                             Path = _image.Path,
                             Name = _image.Name
                         };
            return await images.ToListAsync();
        }

        private async Task<bool> ValidateModelState(string _operation)
        {
            if (ModelState.IsValid)
                return true;

            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            await m_DbContext.RecordChanges(
                currentUser.UserName,
                _operation,
                ActionRecord.OperationStatus_.Failed,
                new Dictionary<string, string>()
                {
                    { CustomInfoKey.Error, ErrorMessage.InvalidModelState }
                }
            );

            return false;
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly P24ImageManagerService m_ImageManagerSvc;
    }

}

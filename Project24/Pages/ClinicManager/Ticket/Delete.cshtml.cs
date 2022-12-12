/*  P24/Ticket/Delete.cshtml
 *  Version: 1.0 (2022.12.04)
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


        public DeleteModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager, P24ImageManagerService _imageManagerSvc, ILogger<DeleteModel> _logger)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
            m_ImageManagerSvc = _imageManagerSvc;
            m_Logger = _logger;
        }


        public async Task<IActionResult> OnGetAsync(string _code)
        {
            if (string.IsNullOrEmpty(_code))
                return Partial("_CommonNotFound", new CommonNotFoundModel(P24Constants.Ticket, "null", "List"));

            var ticket = await (from _ticket in m_DbContext.VisitingProfiles.Include(_t => _t.AddedUser).Include(_t => _t.UpdatedUser).Include(_t => _t.Customer)
                                where _ticket.Code == _code
                                select new P24TicketDetailsViewModelEx()
                                {
                                    Code = _code,
                                    Diagnose = _ticket.Diagnose,
                                    Treatment = _ticket.ProposeTreatment,
                                    Notes = _ticket.Notes,
                                    AddedDate = _ticket.AddedDate,
                                    UpdatedDate = _ticket.UpdatedDate,
                                    DeletedDate = _ticket.DeletedDate,
                                    AddedUserName = _ticket.AddedUser.UserName,
                                    UpdatedUserName = _ticket.UpdatedUser.UserName,
                                    Customer = new P24CustomerDetailsViewModel()
                                    {
                                        Code = _ticket.Customer.Code,
                                        Fullname = _ticket.Customer.FullName,
                                        Gender = AppUtils.NormalizeGenderString(_ticket.Customer.Gender),
                                        DoB = _ticket.Customer.DateOfBirth,
                                        PhoneNumber = _ticket.Customer.PhoneNumber,
                                        Address = _ticket.Customer.Address,
                                        Notes = _ticket.Customer.Notes
                                    }
                                })
                         .FirstOrDefaultAsync();

            if (ticket == null)
                return Partial("_CommonNotFound", new CommonNotFoundModel(P24Constants.Ticket, _code, "List"));

            TicketViewData = ticket;

            ListImageModel = new P24ImageListingModel()
            {
                Images = await FetchImages(_code),
                CustomerCode = _code,
                IsReadonly = true
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync([Bind] string TicketCode)
        {
            if (!await ValidateModelState(ActionRecord.Operation_.DeleteTicket))
                return Page();

            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);

            var ticket = await (from _ticket in m_DbContext.VisitingProfiles.Include(_t => _t.TicketImages)
                                  where _ticket.Code == TicketCode
                                  select _ticket)
                           .FirstOrDefaultAsync();

            var customerCode = await (from _ticket in m_DbContext.VisitingProfiles.Include(_t => _t.Customer)
                                      where _ticket.Code == TicketCode
                                      select _ticket.Customer.Code)
                               .FirstOrDefaultAsync();

            if (ticket == null)
                return BadRequest();

            ticket.DeletedDate = DateTime.Now;
            ticket.UpdatedUser = currentUser;
            m_DbContext.Update(ticket);

            Dictionary<string, string> customInfo = new Dictionary<string, string>()
            {
                { CustomInfoKey.TicketCode, ticket.Code },
                { CustomInfoKey.CustomerCode, customerCode },
            };

            var responseData = await m_ImageManagerSvc.DeleteAsync(currentUser, ticket.TicketImages);
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

        //        // Ajax call only;
        //        public async Task<IActionResult> OnPostDeleteImageAsync([FromBody] P24DeleteImageFormDataModel _formData)
        //        {
        //            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);

        //            if (!ModelState.IsValid)
        //            {
        //                await m_DbContext.RecordChanges(
        //                    currentUser.UserName,
        //                    ActionRecord.Operation_.DeleteCustomer_DeleteImage,
        //                    ActionRecord.OperationStatus_.Failed,
        //                    new Dictionary<string, string>()
        //                    {
        //                        { CustomInfoKey.Error, ErrorMessage.InvalidModelState }
        //                    }
        //                );

        //                return BadRequest();
        //            }

        //            //if (_formData == null)
        //            //    return BadRequest();

        //            //if (string.IsNullOrEmpty(_formData.ImageId) || string.IsNullOrEmpty(_formData.CustomerCode))
        //            //    return BadRequest();

        //            int imageId = -1;
        //            if (!int.TryParse(_formData.ImageId, out imageId))
        //                return BadRequest();

        //            var customer = await (from _customer in m_DbContext.CustomerProfiles
        //                                  where _customer.Code == _formData.CustomerCode
        //                                  select _customer)
        //                           .FirstOrDefaultAsync();

        //            if (customer == null)
        //                return BadRequest();

        //            var image = await (from _image in m_DbContext.CustomerImages
        //                               where _image.Id == imageId
        //                               select _image)
        //                        .FirstOrDefaultAsync();

        //            if (image == null)
        //                return BadRequest();

        //            string operationStatus = ActionRecord.OperationStatus_.Success;
        //            ImageProcessor processor = new ImageProcessor(m_DbContext, currentUser, customer);
        //            if (!await processor.ProcessDelete(image))
        //            {
        //                operationStatus = ActionRecord.OperationStatus_.Failed;
        //                return StatusCode(StatusCodes.Status410Gone);
        //            }

        //            await m_DbContext.RecordChanges(
        //                currentUser.UserName,
        //                ActionRecord.Operation_.DeleteCustomer_DeleteImage,
        //                operationStatus,
        //                processor.CustomInfo
        //            );

        //            ListImageModel = new P24ImageListingModel()
        //            {
        //                Images = await FetchImages(customer.Code),
        //                CustomerCode = customer.Code,
        //                IsReadonly = false
        //            };
        //            return Partial("_CommonListImage", ListImageModel);
        //        }

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
        private readonly ILogger<DeleteModel> m_Logger;
    }

}

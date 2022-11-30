/*  P24/Ticket/Create.cshtml
 *  Version: 1.0 (2022.11.30)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project24.App.Services.P24ImageManager;
using Project24.Data;
using Project24.Identity;
using Project24.Models;
using Project24.Models.ClinicManager;
using Project24.Models.ClinicManager.DataModel;
using Project24.Utils.ClinicManager;

namespace Project24.Pages.ClinicManager.Ticket
{
    [Authorize(Roles = P24RoleName.Manager)]
    [RequestFormLimits(MultipartBodyLengthLimit = 32L * 1024L * 1024L)]
    public class CreateModel : PageModel
    {
        public P24CreateTicketFormDataModel TicketFormData { get; }
        //public P24CreateCustomerFormDataModel CustomerFormData { get; }

        public string NextTicketCode { get; set; }
        public string NextCustomerCode { get; set; }


        public CreateModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager, P24ImageManagerService _imageManagerSvc, ILogger<CreateModel> _logger)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
            m_ImageManagerSvc = _imageManagerSvc;
            m_Logger = _logger;
        }


        public void OnGet(string _customerCode)
        {
            DailyIndexes dind = m_DbContext.DailyIndexes;

            NextTicketCode = string.Format(AppConfig.TicketCodeFormatString, DateTime.Today, dind.VisitingIndex + 1);

            if (!string.IsNullOrEmpty(_customerCode))
            {
                NextCustomerCode = _customerCode;
            }
        }

        public async Task<IActionResult> OnPostOldCustomerAsync([Bind] P24CreateTicketFormDataModel TicketFormData)
        {
            if (!await ValidateModelState(ActionRecord.Operation_.CreateTicket))
                return Page();

            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);

            DailyIndexes dind = m_DbContext.DailyIndexes;

            var customer = await (from _customer in m_DbContext.CustomerProfiles
                                  where _customer.Code == TicketFormData.CustomerFormData.Code && _customer.DeletedDate == DateTime.MinValue
                                  select _customer)
                           .FirstOrDefaultAsync();

            if (customer == null)
                return BadRequest();

            VisitingProfile ticket = new VisitingProfile(currentUser, customer, dind.VisitingIndex + 1)
            {
                Diagnose = TicketFormData.Diagnose,
                ProposeTreatment = TicketFormData.Treatment,
                Note = TicketFormData.Notes
            };
            await m_DbContext.AddAsync(ticket);
            ++dind.VisitingIndex;

            m_DbContext.DailyIndexes = dind;

            Dictionary<string, string> customInfo = new Dictionary<string, string>()
            {
                { CustomInfoKey.TicketCode, ticket.Code },
                { CustomInfoKey.CustomerCode, customer.Code },
                { CustomInfoKey.HasNewCustomer, "false" }
            };

            if (TicketFormData.UploadedFiles != null && TicketFormData.UploadedFiles.Length > 0)
            {
                var resposeData = await m_ImageManagerSvc.UploadAsync(currentUser, ticket, TicketFormData.UploadedFiles);

                if (resposeData.IsSuccess)
                {
                    customInfo.Add(CustomInfoKey.AddedList, resposeData.AddedFileNames.Count.ToString());
                    customInfo.Add(CustomInfoKey.Invalid, resposeData.InvalidFileNames.Count.ToString());
                    customInfo.Add(CustomInfoKey.Error, resposeData.ErrorFileMessages.Count.ToString());
                }
            }

            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.CreateTicket,
                ActionRecord.OperationStatus_.Success,
                customInfo
            );

            return RedirectToPage("List");
        }

        public async Task<IActionResult> OnPostNewCustomerAsync([Bind] P24CreateTicketFormDataModel TicketFormData)
        {
            if (!await ValidateModelState(ActionRecord.Operation_.CreateTicket))
                return Page();

            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);

            DailyIndexes dind = m_DbContext.DailyIndexes;

            CustomerProfile customer = await CreateCustomerMinimal(currentUser, dind.CustomerIndex + 1, TicketFormData.CustomerFormData);
            ++dind.CustomerIndex;

            VisitingProfile ticket = new VisitingProfile(currentUser, customer, dind.VisitingIndex + 1)
            {
                Diagnose = TicketFormData.Diagnose,
                ProposeTreatment = TicketFormData.Treatment,
                Note = TicketFormData.Notes
            };
            await m_DbContext.AddAsync(ticket);
            ++dind.VisitingIndex;

            m_DbContext.DailyIndexes = dind;

            Dictionary<string, string> customInfo = new Dictionary<string, string>()
            {
                { CustomInfoKey.TicketCode, ticket.Code },
                { CustomInfoKey.CustomerCode, customer.Code },
                { CustomInfoKey.HasNewCustomer, "true" }
            };

            if (TicketFormData.UploadedFiles != null && TicketFormData.UploadedFiles.Length > 0)
            {
                var resposeData = await m_ImageManagerSvc.UploadAsync(currentUser, ticket, TicketFormData.UploadedFiles);

                if (resposeData.IsSuccess)
                {
                    customInfo.Add(CustomInfoKey.AddedList, resposeData.AddedFileNames.Count.ToString());
                    customInfo.Add(CustomInfoKey.Invalid, resposeData.InvalidFileNames.Count.ToString());
                    customInfo.Add(CustomInfoKey.Error, resposeData.ErrorFileMessages.Count.ToString());
                }
            }

            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.CreateTicket,
                ActionRecord.OperationStatus_.Success,
                customInfo
            );

            return RedirectToPage("List");
        }

        private async Task<CustomerProfile> CreateCustomerMinimal(P24IdentityUser _currentUser, int _customerIndex, P24CreateCustomerFormDataModel _formData)
        {
            var tokens = P24Utils.SplitFirstLastName(_formData.FullName);
            CustomerProfile customer = new CustomerProfile(_currentUser, _customerIndex)
            {
                FirstMidName = tokens.Item1,
                LastName = tokens.Item2,
                Gender = _formData.Gender,
                DateOfBirth = _formData.DateOfBirth,
                PhoneNumber = _formData.PhoneNumber,
                Address = _formData.Address,
                Notes = _formData.Notes
            };
            await m_DbContext.AddAsync(customer);

            return customer;
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
        private readonly ILogger<CreateModel> m_Logger;
    }

}




///*  P24/Customer/Create.cshtml
// *  Version: 1.4 (2022.11.28)
// *
// *  Contributor
// *      Arime-chan
// */
//namespace Project24.Pages.ClinicManager.Customer
//{
//    public class CreateModel : PageModel
//    {

//        public async Task<IActionResult> OnPostAsync([Bind] P24CreateCustomerFormDataModel FormData)
//        {
//            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);

//            if (!ModelState.IsValid)
//            {
//                await m_DbContext.RecordChanges(
//                    currentUser.UserName,
//                    ActionRecord.Operation_.CreateCustomer,
//                    ActionRecord.OperationStatus_.Failed,
//                    new Dictionary<string, string>()
//                    {
//                        { CustomInfoKey.Error, ErrorMessage.InvalidModelState }
//                    }
//                );

//                return Page();
//            }

//            DailyIndexes dind = m_DbContext.DailyIndexes;

//            var tokens = P24Utils.SplitFirstLastName(FormData.FullName);
//            CustomerProfile customer = new CustomerProfile(currentUser, dind.CustomerIndex + 1)
//            {
//                FirstMidName = tokens.Item1,
//                LastName = tokens.Item2,
//                Gender = FormData.Gender,
//                DateOfBirth = FormData.DateOfBirth,
//                PhoneNumber = FormData.PhoneNumber,
//                Address = FormData.Address,
//                Notes = FormData.Notes
//            };
//            await m_DbContext.AddAsync(customer);

//            ++dind.CustomerIndex;
//            m_DbContext.DailyIndexes = dind;

//            await ProcessUploadedFiles(currentUser, customer, FormData.UploadedFiles, ActionRecord.Operation_.CreateCustomer);
//            return RedirectToPage("List");
//        }

//        public async Task<IActionResult> OnPostCreateImageAsync([Bind] P24CreateImageFormDataModel _formData)
//        {
//            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);

//            if (!ModelState.IsValid)
//            {
//                await m_DbContext.RecordChanges(
//                    currentUser.UserName,
//                    ActionRecord.Operation_.CreateCustomer_CreateImage,
//                    ActionRecord.OperationStatus_.Failed,
//                    new Dictionary<string, string>()
//                    {
//                        { CustomInfoKey.Error, ErrorMessage.InvalidModelState }
//                    }
//                );

//                return BadRequest();
//            }

//            var customer = await (from _customer in m_DbContext.CustomerProfiles
//                                  where _customer.Code == _formData.CustomerCode
//                                  select _customer)
//                           .FirstOrDefaultAsync();

//            if (customer == null)
//                return BadRequest();

//            await ProcessUploadedFiles(currentUser, customer, _formData.UploadedFiles, ActionRecord.Operation_.CreateCustomer_CreateImage);
//            return RedirectToPage("Details", new { _code = customer.Code });
//        }
//    }

//}

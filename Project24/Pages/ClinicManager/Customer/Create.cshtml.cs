/*  P24/Customer/Create.cshtml
 *  Version: 1.7 (2022.12.29)
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
using Project24.App.Extension;
using Project24.App.Services.P24ImageManager;
using Project24.Data;
using Project24.Models;
using Project24.Models.ClinicManager;
using Project24.Models.ClinicManager.DataModel;
using Project24.Models.Identity;
using Project24.Utils.ClinicManager;

namespace Project24.Pages.ClinicManager.Customer
{
    [Authorize(Roles = P24RoleName.Manager)]
    [RequestFormLimits(MultipartBodyLengthLimit = 32L * 1024L * 1024L)]
    public class CreateModel : PageModel
    {
        public P24CreateCustomerFormDataModelEx FormData { get; }

        public string NextCustomerCode { get; set; }

        //public string StatusMessage { get; set; }


        public CreateModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager, P24ImageManagerService _imageManagerSvc)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
            m_ImageManagerSvc = _imageManagerSvc;
        }


        public void OnGet()
        {
            DailyIndexes dind = m_DbContext.DailyIndexes;

            NextCustomerCode = string.Format(AppConfig.CustomerCodeFormatString, DateTime.Today, dind.CustomerIndex + 1);
        }

        public async Task<IActionResult> OnPostAsync([Bind] P24CreateCustomerFormDataModelEx FormData)
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            if (!await this.ValidateModelState(m_DbContext, currentUser, ActionRecord.Operation_.CreateCustomer))
                return Page();

            DailyIndexes dind = m_DbContext.DailyIndexes;

            var tokens = P24Utils.SplitFirstLastName(FormData.FullName);
            CustomerProfile customer = new CustomerProfile(currentUser, dind.CustomerIndex + 1)
            {
                FirstMidName = tokens.Item1,
                LastName = tokens.Item2,
                Gender = FormData.Gender,
                DateOfBirth = FormData.DateOfBirth,
                PhoneNumber = FormData.PhoneNumber,
                Address = FormData.Address,
                Note = FormData.Note
            };
            await m_DbContext.AddAsync(customer);
            ++dind.CustomerIndex;

            m_DbContext.DailyIndexes = dind;

            Dictionary<string, string> customInfo = new Dictionary<string, string>()
            {
                { CustomInfoKey.CustomerCode, customer.Code }
            };

            if (FormData.UploadedFiles != null && FormData.UploadedFiles.Length > 0)
            {
                var resposeData = await m_ImageManagerSvc.UploadAsync(currentUser, customer, FormData.UploadedFiles);

                if (resposeData.IsSuccess)
                {
                    customInfo.Add(CustomInfoKey.AddedList, resposeData.AddedFileNames.Count.ToString());
                    customInfo.Add(CustomInfoKey.Invalid, resposeData.InvalidFileNames.Count.ToString());
                    customInfo.Add(CustomInfoKey.Error, resposeData.ErrorFileMessages.Count.ToString());
                }
            }

            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.CreateCustomer,
                ActionRecord.OperationStatus_.Success,
                customInfo
            );

            return RedirectToPage("List");
        }

        public async Task<IActionResult> OnPostCreateImageAsync([Bind] P24CreateImageFormDataModel _formData)
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                await m_DbContext.RecordChanges(
                    currentUser.UserName,
                    ActionRecord.Operation_.CreateCustomer_CreateImage,
                    ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.Error, ErrorMessage.InvalidModelState }
                    }
                );

                return BadRequest();
            }

            var customer = await (from _customer in m_DbContext.CustomerProfiles
                                  where _customer.Code == _formData.OwnerCode
                                  select _customer)
                           .FirstOrDefaultAsync();

            if (customer == null)
                return BadRequest();

            Dictionary<string, string> customInfo = new Dictionary<string, string>()
            {
                { CustomInfoKey.CustomerCode, customer.Code }
            };

            if (_formData.UploadedFiles != null && _formData.UploadedFiles.Length > 0)
            {
                var resposeData = await m_ImageManagerSvc.UploadAsync(currentUser, customer, _formData.UploadedFiles);

                if (resposeData.IsSuccess)
                {
                    customInfo.Add(CustomInfoKey.AddedList, resposeData.AddedFileNames.Count.ToString());
                    customInfo.Add(CustomInfoKey.Invalid, resposeData.InvalidFileNames.Count.ToString());
                    customInfo.Add(CustomInfoKey.Error, resposeData.ErrorFileMessages.Count.ToString());
                }
            }

            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.CreateCustomer_CreateImage,
                ActionRecord.OperationStatus_.Success,
                customInfo
            );

            return RedirectToPage("Details", new { _code = customer.Code });
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly P24ImageManagerService m_ImageManagerSvc;
    }

}

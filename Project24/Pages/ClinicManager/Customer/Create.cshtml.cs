/*  P24/Customer/Create.cshtml
 *  Version: 1.4 (2022.11.28)
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
using Project24.Data;
using Project24.Identity;
using Project24.Models;
using Project24.Models.ClinicManager;
using Project24.Models.ClinicManager.DataModel;
using Project24.Utils.ClinicManager;

namespace Project24.Pages.ClinicManager.Customer
{
    [Authorize(Roles = P24RoleName.Manager)]
    [RequestFormLimits(MultipartBodyLengthLimit = 32L * 1024L * 1024L)]
    public class CreateModel : PageModel
    {
        public P24CreateCustomerFormDataModel FormData { get; }

        public string NextCustomerCode { get; set; }

        //public string StatusMessage { get; set; }


        public CreateModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager, ILogger<CreateModel> _logger)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
            m_Logger = _logger;
        }


        public void OnGet()
        {
            DailyIndexes dind = m_DbContext.DailyIndexes;

            NextCustomerCode = string.Format(AppConfig.CustomerCodeFormatString, DateTime.Today, dind.CustomerIndex + 1);
        }

        public async Task<IActionResult> OnPostAsync([Bind] P24CreateCustomerFormDataModel FormData)
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                await m_DbContext.RecordChanges(
                    currentUser.UserName,
                    ActionRecord.Operation_.CreateCustomer,
                    ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.Error, ErrorMessage.InvalidModelState }
                    }
                );

                return Page();
            }

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
                Notes = FormData.Notes
            };
            await m_DbContext.AddAsync(customer);

            ++dind.CustomerIndex;
            m_DbContext.DailyIndexes = dind;

            await ProcessUploadedFiles(currentUser, customer, FormData.UploadedFiles, ActionRecord.Operation_.CreateCustomer);
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
                                  where _customer.Code == _formData.CustomerCode
                                  select _customer)
                           .FirstOrDefaultAsync();

            if (customer == null)
                return BadRequest();

            await ProcessUploadedFiles(currentUser, customer, _formData.UploadedFiles, ActionRecord.Operation_.CreateCustomer_CreateImage);
            return RedirectToPage("Details", new { _code = customer.Code });
        }

        private async Task<bool> ProcessUploadedFiles(P24IdentityUser _currentUser, CustomerProfile _customer, IFormFile[] _files, string _operation)
        {
            if (_files == null && _files.Length <= 0)
            {
                await m_DbContext.RecordChanges(
                    _currentUser.UserName,
                    _operation,
                    ActionRecord.OperationStatus_.Success,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.CustomerCode, _customer.Code }
                    }
                );

                return true;
            }

            ImageProcessor processor = new ImageProcessor(m_DbContext, _currentUser, _customer);
            string operationStatus = await processor.ProcessUpload(_files);

            await m_DbContext.RecordChanges(
                _currentUser.UserName,
                _operation,
                operationStatus,
                processor.CustomInfo
            );

            return true;
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly ILogger<CreateModel> m_Logger;
    }

}

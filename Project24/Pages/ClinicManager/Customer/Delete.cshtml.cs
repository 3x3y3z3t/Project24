/*  P24/Customer/Delete.cshtml
 *  Version: 1.2 (2022.11.28)
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
using Project24.Models;
using Project24.Models.ClinicManager;
using Project24.Models.ClinicManager.DataModel;
using Project24.Models.Identity;
using Project24.Utils.ClinicManager;

namespace Project24.Pages.ClinicManager.Customer
{
    [Authorize(Roles = P24RoleName.Manager)]
    public class DeleteModel : PageModel
    {
        public string CustomerCode { get; private set; }

        public P24CustomerDetailsViewModelEx CustomerViewData { get; private set; }

        public P24ImageListingModel ListImageModel { get; private set; }


        public DeleteModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager, ILogger<DeleteModel> _logger)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
            m_Logger = _logger;
        }


        public async Task<IActionResult> OnGetAsync(string _code)
        {
            if (string.IsNullOrEmpty(_code))
                return Partial("_CommonNotFound", new CommonNotFoundModel(P24Constants.Customer, "null", "List"));

            var customers = from _customer in m_DbContext.CustomerProfiles.Include(_c => _c.AddedUser).Include(_c => _c.UpdatedUser)
                            where _customer.Code == _code
                            select new P24CustomerDetailsViewModelEx()
                            {
                                Code = _code,
                                Fullname = _customer.FullName,
                                Gender = App.AppUtils.NormalizeGenderString(_customer.Gender),
                                DoB = _customer.DateOfBirth,
                                PhoneNumber = _customer.PhoneNumber,
                                Address = _customer.Address,
                                Notes = _customer.Notes,
                                AddedDate = _customer.AddedDate,
                                UpdatedDate = _customer.UpdatedDate,
                                DeletedDate = _customer.DeletedDate,
                                AddedUserName = _customer.AddedUser.UserName,
                                UpdatedUserName = _customer.UpdatedUser.UserName
                            };

            CustomerViewData = await customers.FirstOrDefaultAsync();
            if (CustomerViewData == null)
                return Partial("_CommonNotFound", new CommonNotFoundModel(P24Constants.Customer, _code, "List"));

            ListImageModel = new P24ImageListingModel()
            {
                Images = await FetchImages(_code),
                CustomerCode = _code,
                IsReadonly = true
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync([Bind] string CustomerCode)
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                await m_DbContext.RecordChanges(
                    currentUser.UserName,
                    ActionRecord.Operation_.DeleteCustomer,
                    ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.Error, ErrorMessage.InvalidModelState }
                    }
                );

                return BadRequest();
            }

            var customer = await (from _customer in m_DbContext.CustomerProfiles.Include(_c => _c.CustomerImages)
                                  where _customer.Code == CustomerCode
                                  select _customer)
                           .FirstOrDefaultAsync();

            if (customer == null)
                return BadRequest();

            customer.DeletedDate = DateTime.Now;
            customer.UpdatedUser = currentUser;

            m_DbContext.Update(customer);

            string error = "";
            ImageProcessor processor = new ImageProcessor(m_DbContext, currentUser, customer);
            foreach (var image in customer.CustomerImages)
            {
                if (image.DeletedDate != DateTime.MinValue)
                    continue;

                if (!await processor.ProcessDelete(image))
                    error += image.Id + ", ";
            }

            Dictionary<string, string> customInfo = new Dictionary<string, string>()
            {
                { CustomInfoKey.CustomerCode, customer.Code }
            };

            if (error != "")
            {
                error = error[0..^2];
                customInfo[CustomInfoKey.Error] = error;
            }

            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.DeleteCustomer,
                ActionRecord.OperationStatus_.Success,
                customInfo
            );

            return RedirectToPage("List");
        }

        // Ajax call only;
        public async Task<IActionResult> OnPostDeleteImageAsync([FromBody] P24DeleteImageFormDataModel _formData)
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                await m_DbContext.RecordChanges(
                    currentUser.UserName,
                    ActionRecord.Operation_.DeleteCustomer_DeleteImage,
                    ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.Error, ErrorMessage.InvalidModelState }
                    }
                );

                return BadRequest();
            }

            //if (_formData == null)
            //    return BadRequest();

            //if (string.IsNullOrEmpty(_formData.ImageId) || string.IsNullOrEmpty(_formData.CustomerCode))
            //    return BadRequest();

            int imageId = -1;
            if (!int.TryParse(_formData.ImageId, out imageId))
                return BadRequest();

            var customer = await (from _customer in m_DbContext.CustomerProfiles
                                  where _customer.Code == _formData.CustomerCode
                                  select _customer)
                           .FirstOrDefaultAsync();

            if (customer == null)
                return BadRequest();

            var image = await (from _image in m_DbContext.CustomerImages
                               where _image.Id == imageId
                               select _image)
                        .FirstOrDefaultAsync();

            if (image == null)
                return BadRequest();

            string operationStatus = ActionRecord.OperationStatus_.Success;
            ImageProcessor processor = new ImageProcessor(m_DbContext, currentUser, customer);
            if (!await processor.ProcessDelete(image))
            {
                operationStatus = ActionRecord.OperationStatus_.Failed;
                return StatusCode(StatusCodes.Status410Gone);
            }

            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.DeleteCustomer_DeleteImage,
                operationStatus,
                processor.CustomInfo
            );

            ListImageModel = new P24ImageListingModel()
            {
                Images = await FetchImages(customer.Code),
                CustomerCode = customer.Code,
                IsReadonly = false
            };
            return Partial("_CommonListImage", ListImageModel);
        }

        private async Task<List<P24ImageViewModel>> FetchImages(string _customerCode)
        {
            var id = await (from _customer in m_DbContext.CustomerProfiles
                            where _customer.Code == _customerCode
                            select _customer.Id)
                           .FirstOrDefaultAsync();

            var images = from _image in m_DbContext.CustomerImages
                         where _image.OwnerCustomerId == id && _image.DeletedDate == DateTime.MinValue
                         select new P24ImageViewModel()
                         {
                             Id = _image.Id,
                             Path = _image.Path,
                             Name = _image.Name
                         };
            return await images.ToListAsync();
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly ILogger<DeleteModel> m_Logger;
    }

}

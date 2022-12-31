/*  P24/Customer/Delete.cshtml
 *  Version: 1.4 (2022.12.29)
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

namespace Project24.Pages.ClinicManager.Customer
{
    [Authorize(Roles = P24RoleName.Manager)]
    public class DeleteModel : PageModel
    {
        public string CustomerCode { get; private set; }

        public P24CustomerDetailsViewModelEx CustomerViewData { get; private set; }

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
                return Partial("_CommonNotFound", new CommonNotFoundModel(P24Constants.Customer, "null", "List"));

            var customers = from _customer in m_DbContext.CustomerProfiles.Include(_c => _c.AddedUser).Include(_c => _c.EditedUser)
                            where _customer.Code == _code
                            select new P24CustomerDetailsViewModelEx()
                            {
                                Code = _code,
                                Fullname = _customer.FullName,
                                Gender = App.AppUtils.NormalizeGenderString(_customer.Gender),
                                DoB = _customer.DateOfBirth,
                                PhoneNumber = _customer.PhoneNumber,
                                Address = _customer.Address,
                                Note = _customer.Note,
                                AddedDate = _customer.AddedDate,
                                UpdatedDate = _customer.EditedDate,
                                DeletedDate = _customer.DeletedDate,
                                AddedUserName = _customer.AddedUser.UserName,
                                UpdatedUserName = _customer.EditedUser.UserName
                            };

            CustomerViewData = await customers.FirstOrDefaultAsync();
            if (CustomerViewData == null)
                return Partial("_CommonNotFound", new CommonNotFoundModel(P24Constants.Customer, _code, "List"));

            ListImageModel = new P24ImageListingModel()
            {
                Module = P24Module.Customer,
                OwnerCode = _code,
                IsReadonly = true,
                Images = await FetchImages(_code)
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync([Bind] string CustomerCode)
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            if (!await this.ValidateModelState(m_DbContext, currentUser, ActionRecord.Operation_.DeleteCustomer))
                return Page();

            var customer = await (from _customer in m_DbContext.CustomerProfiles.Include(_c => _c.CustomerImages)
                                  where _customer.Code == CustomerCode
                                  select _customer)
                           .FirstOrDefaultAsync();

            if (customer == null)
                return BadRequest();

            customer.DeletedDate = DateTime.Now;
            customer.EditedUser = currentUser;
            m_DbContext.Update(customer);

            Dictionary<string, string> customInfo = new Dictionary<string, string>()
            {
                { CustomInfoKey.CustomerCode, customer.Code },
            };

            var responseData = m_ImageManagerSvc.Delete(currentUser, customer.CustomerImages);
            if (responseData.IsSuccess)
            {
                customInfo.Add(CustomInfoKey.DeletedList, responseData.DeletedFileNames.Count.ToString());
                customInfo.Add(CustomInfoKey.Error, responseData.ErrorFileMessages.Count.ToString());
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
        public async Task<IActionResult> OnPostDeleteImageAsync([FromBody] string _imageId)
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            if (!await this.ValidateModelState(m_DbContext, currentUser, ActionRecord.Operation_.DeleteCustomer))
                return BadRequest();

            if (!int.TryParse(_imageId, out int imgId))
                return BadRequest();


            var image = await (from _image in m_DbContext.CustomerImages.Include(_img => _img.OwnerCustomer)
                               where _image.Id == imgId && _image.DeletedDate == DateTime.MinValue
                               select _image)
                         .FirstOrDefaultAsync();

            if (image == null)
                return BadRequest();

            Dictionary<string, string> customInfo = new Dictionary<string, string>()
            {
                { CustomInfoKey.TicketCode, image.OwnerCustomer.Code }
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
                Module = P24Module.Customer,
                OwnerCode = image.OwnerCustomer.Code,
                Images = await FetchImages(image.OwnerCustomer.Code)
            };

            return Partial("_CommonListImage", ListImageModel);
        }

        private async Task<List<P24ImageViewModel>> FetchImages(string _customerCode)
        {
            var images = from _image in m_DbContext.CustomerImages.Include(_i => _i.OwnerCustomer)
                         where _image.OwnerCustomer.Code == _customerCode && _image.DeletedDate == DateTime.MinValue
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
        private readonly P24ImageManagerService m_ImageManagerSvc;
    }

}

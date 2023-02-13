/*  P24/ImageManager.cshtml.cs
 *  Version: 1.0 (2023.02.14)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
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
using Project24.Models.ClinicManager.DataModel;
using Project24.Models.Identity;

namespace Project24.Pages.ClinicManager
{
    [Authorize(Roles = P24RoleName.Manager)]
    public class ImageManagerModel : PageModel
    {
        public class RenameImageFormData
        {
            public string ImageId { get; set; }
            public string NewName { get; set; }

            public RenameImageFormData()
            { }
        }


        public ImageManagerModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager, P24ImageManagerService _imageManagerSvc)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
            m_ImageManagerSvc = _imageManagerSvc;
        }


        public void OnGet() => BadRequest();
        public void OnPost() => BadRequest();

        // Ajax call only;
        public async Task<IActionResult> OnPostRenameCustomerImageAsync([FromBody] RenameImageFormData _formData)
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            if (!await this.ValidateModelState(m_DbContext, currentUser, ActionRecord.Operation_.ImageManager_RenameCustomerImage))
                return BadRequest();

            if (!int.TryParse(_formData.ImageId, out int imgId))
                return BadRequest();


            var image = await (from _image in m_DbContext.CustomerImages.Include(_img => _img.OwnerCustomer)
                               where _image.Id == imgId && _image.DeletedDate == DateTime.MinValue
                               select _image)
                         .FirstOrDefaultAsync();

            if (image == null)
                return BadRequest();

            Dictionary<string, string> customInfo = new Dictionary<string, string>()
            {
                { CustomInfoKey.CustomerCode, image.OwnerCustomer.Code }
            };

            var responseData = m_ImageManagerSvc.Rename(currentUser, image, _formData.NewName);
            if (!responseData.IsSuccess)
            {
                customInfo.Add(CustomInfoKey.Error, responseData.ErrorFileMessages[0]);

                await m_DbContext.RecordChanges(
                    currentUser.UserName,
                    ActionRecord.Operation_.ImageManager_RenameCustomerImage,
                    ActionRecord.OperationStatus_.Failed,
                    customInfo
                );

                return Content(CustomInfoTag.Error + responseData.LastMessage, MediaTypeNames.Text.Plain);
            }

            customInfo.Add(CustomInfoKey.Filename, image.Name + " -> " + _formData.NewName);

            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.ImageManager_RenameCustomerImage,
                ActionRecord.OperationStatus_.Success,
                customInfo
            );

            P24ImageListingModel listImageModel = new P24ImageListingModel()
            {
                Module = P24Module.Customer,
                OwnerCode = image.OwnerCustomer.Code,
                Images = await this.FetchCustomerImages(m_DbContext, image.OwnerCustomer.Code)
            };

            return Partial("_CommonListImage", listImageModel);
        }

        // Ajax call only;
        public async Task<IActionResult> OnPostRenameTicketImageAsync([FromBody] RenameImageFormData _formData)
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            if (!await this.ValidateModelState(m_DbContext, currentUser, ActionRecord.Operation_.ImageManager_RenameTicketImage))
                return BadRequest();

            if (!int.TryParse(_formData.ImageId, out int imgId))
                return BadRequest();


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

            var responseData = m_ImageManagerSvc.Rename(currentUser, image, _formData.NewName);
            if (!responseData.IsSuccess)
            {
                customInfo.Add(CustomInfoKey.Error, responseData.ErrorFileMessages[0]);

                await m_DbContext.RecordChanges(
                    currentUser.UserName,
                    ActionRecord.Operation_.ImageManager_RenameTicketImage,
                    ActionRecord.OperationStatus_.Failed,
                    customInfo
                );

                return Content(CustomInfoTag.Error + responseData.LastMessage, MediaTypeNames.Text.Plain);
            }

            customInfo.Add(CustomInfoKey.Filename, image.Name + " -> " + _formData.NewName);

            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.ImageManager_RenameTicketImage,
                ActionRecord.OperationStatus_.Success,
                customInfo
            );

            P24ImageListingModel listImageModel = new P24ImageListingModel()
            {
                Module = P24Module.Customer,
                OwnerCode = image.OwnerTicket.Code,
                Images = await this.FetchTicketImages(m_DbContext, image.OwnerTicket.Code)
            };

            return Partial("_CommonListImage", listImageModel);
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly P24ImageManagerService m_ImageManagerSvc;
    }

}

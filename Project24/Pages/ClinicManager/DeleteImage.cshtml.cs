/*  Details_DeleteImage.cshtml.cs
 *  Version: 1.1 (2022.10.21)
 *
 *  Contributor
 *      Arime-chan
 */
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project24.Data;
using Project24.Identity;
using Project24.Models;

namespace Project24.Pages.ClinicManager
{
    [Authorize(Roles = P24Roles.Manager)]
    public class DeleteImageModel : PageModel
    {
        public class DataModel
        {
            public string CustomerCode { get; set; }
            public string ImageId { get; set; }

            public DataModel()
            { }
        }


        public DeleteImageModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
        }


        public async Task<IActionResult> OnPostAsync([FromBody] DataModel _data)
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Content(ErrorMessage.CurrentUserIsNull, MediaTypeNames.Text.Plain);
            }
            
            if (!ModelState.IsValid)
            {
                await m_DbContext.RecordChanges(
                    currentUser.UserName,
                    ActionRecord.Operation_.DetailCustomer_DelImage,
                    ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.Error, ErrorMessage.InvalidModelState }
                    }
                );

                return Content(ErrorMessage.InvalidModelState, MediaTypeNames.Text.Plain);
            }

            var customer = await m_DbContext.CustomerProfiles.FirstOrDefaultAsync(_customer => _customer.CustomerCode == _data.CustomerCode);
            if (customer == null)
            {
                await m_DbContext.RecordChanges(
                    currentUser.UserName,
                    ActionRecord.Operation_.DetailCustomer_DelImage,
                    ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.CustomerCode, _data.CustomerCode },
                        { CustomInfoKey.Error, ErrorMessage.CustomerNotFound }
                    }
                );

                return Content(ErrorMessage.CustomerNotFound, MediaTypeNames.Text.Plain);
            }

            int imageId = 0;
            if (!int.TryParse(_data.ImageId, out imageId))
            {
                await m_DbContext.RecordChanges(
                    currentUser.UserName,
                    ActionRecord.Operation_.DetailCustomer_DelImage,
                    ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.CustomerCode, _data.CustomerCode },
                        { CustomInfoKey.ImageId, _data.ImageId.ToString() },
                        { CustomInfoKey.Error, ErrorMessage.ImageNotFound }
                    }
                );

                return Content(ErrorMessage.ImageNotFound, MediaTypeNames.Text.Plain);
            }

            var image = await m_DbContext.CustomerImages.FirstOrDefaultAsync(_image => _image.Id == imageId);
            if (image == null)
            {
                await m_DbContext.RecordChanges(
                    currentUser.UserName,
                    ActionRecord.Operation_.DetailCustomer_DelImage,
                    ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.CustomerCode, _data.CustomerCode },
                        { CustomInfoKey.ImageId, _data.ImageId.ToString() },
                        { CustomInfoKey.Error, ErrorMessage.ImageNotFound }
                    }
                );

                return Content(ErrorMessage.ImageNotFound, MediaTypeNames.Text.Plain);
            }

            string dataRoot = Path.GetFullPath(Utils.AppRoot + "/" + AppConfig.DataRoot);

            string currentFilepath = dataRoot + image.Filepath;
            string deletedCacheFilepath = currentFilepath.Replace("data", "deletedData");

            int pos = deletedCacheFilepath.LastIndexOf("/");
            //string dbg = deletedCacheFilepath.Remove(pos);
            Directory.CreateDirectory(deletedCacheFilepath.Remove(pos));
            System.IO.File.Move(currentFilepath, deletedCacheFilepath, true);

            image.DeletedDate = DateTime.Now;
            //image.Filepath = deletedCacheFilepath;
            m_DbContext.Update(image);

            customer.UpdatedDate = DateTime.Now;
            customer.UpdatedUser = currentUser;
            m_DbContext.Update(customer);

            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.DetailCustomer_DelImage,
                ActionRecord.OperationStatus_.Success,
                new Dictionary<string, string>()
                {
                    { CustomInfoKey.CustomerCode, _data.CustomerCode },
                    { CustomInfoKey.ImageId, image.Id.ToString() },
                    { CustomInfoKey.Path, image.Filepath }
                }
            );

            return Content("success", MediaTypeNames.Text.Plain);
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
    }

}

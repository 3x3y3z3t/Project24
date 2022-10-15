/*  Details_DeleteImage.cshtml.cs
 *  Version: 1.0 (2022.10.15)
 *
 *  Contributor
 *      Arime-chan
 */
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

using System;
using System.IO;
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
        public DeleteImageModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
        }

        public async Task<IActionResult> OnGetAsync(string? _code, int? _imgId)
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            if (currentUser == null || _code == null || _imgId == null)
                return BadRequest();

            var customer = await m_DbContext.CustomerProfilesDev2.FirstOrDefaultAsync(_customer => _customer.CustomerCode == _code);
            if (customer == null)
            {
                await Utils.RecordAction(
                    currentUser.UserName,
                ActionRecord.Operation_.DetailCustomer_DelImage,
                    ActionRecord.OperationStatus_.Failed,
                    Constants.ERROR_NOT_FOUND_CUSTOMER + ";" + _code
                );

                return LocalRedirect("/ClinicManager/Details/" + _code);
            }

            var image = await m_DbContext.CustomerImageDev.FirstOrDefaultAsync(_image => _image.Id == _imgId);
            if (image == null)
            {
                await Utils.RecordAction(
                    currentUser.UserName,
                ActionRecord.Operation_.DetailCustomer_DelImage,
                    ActionRecord.OperationStatus_.Failed,
                    Constants.ERROR_NOT_FOUND_IMG_ID + ";" + _code + ";" + _imgId
                );

                return LocalRedirect("/ClinicManager/Details/" + _code);
            }

            string dirRoot = Directory.GetDirectoryRoot(Constants.WorkingDir);
            string dataRoot = Path.GetFullPath(Constants.DataRoot, Constants.WorkingDir);

            string currentFilepath = dataRoot + image.Filepath;
            string deletedCacheFilepath = currentFilepath.Replace("data", "deletedData");
            string deletedCacheDir = Path.GetDirectoryName(deletedCacheFilepath);
            Directory.CreateDirectory(deletedCacheDir);

            System.IO.File.Move(currentFilepath, deletedCacheFilepath, true);

            image.DeletedDate = DateTime.Now;
            //image.Filepath = deletedCacheFilepath;
            m_DbContext.Update(image);

            customer.UpdatedDate = DateTime.Now;
            customer.UpdatedUser = currentUser;
            m_DbContext.Update(customer);

            await m_DbContext.SaveChangesAsync();

            await Utils.RecordAction(
                currentUser.UserName,
                ActionRecord.Operation_.DetailCustomer_DelImage,
                ActionRecord.OperationStatus_.Success,
                image.Id + ": " + image.Filepath
            );

            return LocalRedirect("/ClinicManager/Details/" + _code);
        }

        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
    }

}

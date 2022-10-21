/*  Details.cshtml.cs
 *  Version: 1.3 (2022.10.22)
 *
 *  Contributor
 *      Arime-chan
 */
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    [RequestFormLimits(MultipartBodyLengthLimit = 32L * 1024L * 1024L)]
    public class DetailsModel : PageModel
    {
        public class DataModel
        {
            public string CustomerCode { get; set; }

            [DataType(DataType.Upload)]
            public IFormFile[] FileUploads { get; set; }


            //[Required(ErrorMessage = Constants.ERROR_MANAGER_PASSWORD_REQUIRED)]
            [DataType(DataType.Password)]
            public string ManagerPassword { get; set; }

            public DataModel()
            { }
        }

        [BindProperty]
        public DataModel Data { get; set; }

        public string StatusMessage { get; set; }

        public CustomerProfile CustomerProfile { get; set; }
        public List<CustomerImage> CustomerImages { get; set; }


        public DetailsModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
        }


        public async Task<IActionResult> OnGetAsync(string? _code, string? _mode = null)
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            if (currentUser == null)
                return BadRequest();

            if (_mode != null && _mode == "AddImage")
            {
                ViewData["AddImgFormStyle"] = "";
            }
            else
            {
                ViewData["AddImgFormStyle"] = "display: none;";
            }

            if (_code == null)
            {
                TempData["CustomerCode"] = "null";
                return Page();
            }

            var customers = from _customers in m_DbContext.CustomerProfiles.Include(_c => _c.AddedUser).Include(_c => _c.UpdatedUser)
                            where _customers.CustomerCode == _code
                            select _customers;

            if (customers.Count() <= 0)
            {
                TempData["CustomerCode"] = _code;
                return Page();
            }

            CustomerProfile = customers.First();

            if (CustomerProfile.DeletedDate != DateTime.MinValue)
            {
                await m_DbContext.RecordChanges(
                    currentUser.UserName,
                    ActionRecord.Operation_.DetailCustomer,
                    ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.Message, "Already deleted on " + CustomerProfile.DeletedDate.ToString() }
                    }
                );

                return RedirectToPage("./Index");
            }

            var images = from _images in m_DbContext.CustomerImages
                         where _images.OwnedCustomerId == CustomerProfile.Id && _images.DeletedDate == DateTime.MinValue
                         select _images;

            if (images.Count() > 0)
            {
                CustomerImages = images.ToList();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            if (currentUser == null)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                await m_DbContext.RecordChanges(
                    currentUser.UserName,
                    ActionRecord.Operation_.DetailCustomer_AddImage,
                    ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.Error, ErrorMessage.InvalidModelState }
                    }
                );

                return Page();
            }

#if false
            if (!await m_UserManager.CheckPasswordAsync(currentUser, Data.ManagerPassword))
            {
                StatusMessage = "Error: " + Constants.ERROR_MANAGER_PASSWORD_INCORRECT;

                await Utils.RecordAction(
                    currentUser.UserName,
                    ActionRecord.Operation_.DetailCustomer_AddImage,
                    ActionRecord.OperationStatus_.Failed,
                    Constants.ERROR_MANAGER_PASSWORD_INCORRECT
                );

                return await OnGetAsync(Data.CustomerCode);
            }
#endif

            var customer = await m_DbContext.CustomerProfiles.FirstOrDefaultAsync(_customer => _customer.CustomerCode == Data.CustomerCode);
            if (customer == null)
            {
                await m_DbContext.RecordChanges(
                    currentUser.UserName,
                    ActionRecord.Operation_.DetailCustomer_AddImage,
                    ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.Error, ErrorMessage.CustomerNotFound },
                        { CustomInfoKey.CustomerCode, Data.CustomerCode }
                    }
                );

                return await OnGetAsync(Data.CustomerCode);
            }

            Dictionary<string, string> customInfo = ProcessUpload(customer);

            customer.UpdatedUser = currentUser;
            m_DbContext.Update(customer);

            string opStatus = ActionRecord.OperationStatus_.Success;
            if (customInfo.ContainsKey(CustomInfoKey.AddedList))
            {
                opStatus += ", " + ActionRecord.OperationStatus_.HasUpload;
            }
            if (customInfo.ContainsKey(CustomInfoKey.Malfunctions))
            {
                opStatus += ", " + ActionRecord.OperationStatus_.HasMalfunction;
            }

            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.DetailCustomer_AddImage,
                opStatus,
                customInfo
            );

            //await m_DbContext.SaveChangesAsync();

            return await OnGetAsync(Data.CustomerCode);
        }

        private Dictionary<string, string> ProcessUpload(CustomerProfile _customer)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result["customerCode"] = _customer.CustomerCode;

            if (Data.FileUploads != null && Data.FileUploads.Length != 0)
            {
                string addedList = "";
                Dictionary<string, int> malfunctionRecord = new Dictionary<string, int>();
                List<CustomerImage> images = new List<CustomerImage>();

                foreach (var file in Data.FileUploads)
                {
                    string[] contentType = file.ContentType.Split('/');
                    if (contentType[0] != "image")
                    {
                        if (malfunctionRecord.ContainsKey(contentType[1]))
                            ++malfunctionRecord[contentType[1]];
                        else
                            malfunctionRecord[contentType[1]] = 1;

                        continue;
                    }

                    string fullPath = Path.GetFullPath(Utils.AppRoot + "/" + AppConfig.DataRoot + "/" + Data.CustomerCode);
                    Directory.CreateDirectory(fullPath);
                    using (FileStream stream = new FileStream(fullPath + "/" + file.FileName, FileMode.Create))
                    {
                        file.CopyTo(stream);

                        //TODO: check file signature;
                    }

                    string path = "/" + Data.CustomerCode + "/" + file.FileName;
                    CustomerImage image = new CustomerImage()
                    {
                        OwnedCustomer = _customer,
                        Filepath = path
                    };
                    images.Add(image);

                    addedList += file.FileName + "; ";
                }

                m_DbContext.AddRange(images);
                _customer.UpdatedDate = DateTime.Now;

                result[CustomInfoKey.AddedList] = addedList;

                if (malfunctionRecord.Count > 0)
                {
                    string malfunctions = "";
                    foreach (var pair in malfunctionRecord)
                    {
                        malfunctions += pair.Value + " " + pair.Key + "; ";
                    }

                    result[CustomInfoKey.Malfunctions] = malfunctions;
                }
            }

            return result;
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
    }

}

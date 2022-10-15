/*  Details.cshtml.cs
 *  Version: 1.2 (2022.10.15)
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
    public class DetailsModel : PageModel
    {
        public class DataModel
        {
            [Required(ErrorMessage = Constants.ERROR_EMPTY_CUSTOMER_ID)]
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

        public CustomerProfileDev2 CustomerProfile { get; set; }
        public List<CustomerImageDev> CustomerImages { get; set; }


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

            var customers = from _customers in m_DbContext.CustomerProfilesDev2.Include(_c => _c.AddedUser).Include(_c => _c.UpdatedUser)
                            where _customers.CustomerCode == _code
                            select _customers;

            if (customers.Count() <= 0)
            {
                TempData["CustomerCode"] = _code;
                return Page();
            }

            if (customers.Count() != 1)
            {
                await Utils.RecordAction(
                    null,
                    ActionRecord.Operation_.DetailCustomer,
                    ActionRecord.OperationStatus_.UnexpectedError,
                    "code=" + _code + ";count=" + customers.Count()
                );

                TempData["Error"] = "true";
                TempData["CustomerCode"] = _code;
                return Page();
            }

            CustomerProfile = customers.First();

            if (CustomerProfile.DeletedDate != DateTime.MinValue)
            {
                await Utils.RecordAction(
                    currentUser.UserName,
                    ActionRecord.Operation_.DetailCustomer,
                    ActionRecord.OperationStatus_.Failed,
                    "Already deleted on " + CustomerProfile.DeletedDate.ToString()
                );

                return RedirectToPage("./Index");
            }

            var images = from _images in m_DbContext.CustomerImageDev
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
                await Utils.RecordAction(
                    currentUser.UserName,
                    ActionRecord.Operation_.DetailCustomer_AddImage,
                    ActionRecord.OperationStatus_.Failed,
                    Constants.ERROR_MODEL_STATE_INVALID
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

            var customer = await m_DbContext.CustomerProfilesDev2.FirstOrDefaultAsync(_customer => _customer.CustomerCode == Data.CustomerCode);
            if (customer == null)
            {
                await Utils.RecordAction(
                    currentUser.UserName,
                    ActionRecord.Operation_.DetailCustomer_AddImage,
                    ActionRecord.OperationStatus_.Failed,
                    Constants.ERROR_NOT_FOUND_CUSTOMER + ";" + Data.CustomerCode
                );

                return await OnGetAsync(Data.CustomerCode);
            }

            Dictionary<string, int> malfunctionRecord = new Dictionary<string, int>();
            bool hasUpload = false;
            string addedList = "";
            if (Data.FileUploads != null && Data.FileUploads.Length != 0)
            {
                hasUpload = true;

                List<CustomerImageDev> images = new List<CustomerImageDev>();
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

                    string dirRoot = Directory.GetDirectoryRoot(Constants.WorkingDir);
                    string dataRoot = Path.GetFullPath(Constants.DataRoot, Constants.WorkingDir);
                    string path = Data.CustomerCode + "/" + file.FileName;

                    Directory.CreateDirectory(dataRoot + Data.CustomerCode);
                    using (FileStream stream = new FileStream(dataRoot + path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);

                        //TODO: check file signature;
                    }

                    CustomerImageDev image = new CustomerImageDev()
                    {
                        OwnedCustomer = customer,
                        Filepath = path
                    };
                    images.Add(image);

                    addedList += file.FileName + "; ";
                }

                m_DbContext.AddRange(images);
            }

            customer.UpdatedDate = DateTime.Now;
            customer.UpdatedUser = currentUser;
            m_DbContext.Update(customer);

            await m_DbContext.SaveChangesAsync();


            string opStatus = ActionRecord.OperationStatus_.Success;
            string description = null;

            if (hasUpload)
            {
                opStatus += ActionRecord.OperationStatus_.HasUpload;
            }

            if (malfunctionRecord.Count > 0)
            {
                description = "";
                foreach (var pair in malfunctionRecord)
                {
                    description += pair.Key + "=" + pair.Value + "; ";
                }
                opStatus += ActionRecord.OperationStatus_.MalfunctionUploadAttempted;
            }

            description += "added=" + addedList;

            await Utils.RecordAction(
                currentUser.UserName,
                ActionRecord.Operation_.DetailCustomer_AddImage,
                opStatus,
                "CustomerCode=" + customer.CustomerCode + ";" + description
            );

            return await OnGetAsync(Data.CustomerCode);
        }

        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
    }

}

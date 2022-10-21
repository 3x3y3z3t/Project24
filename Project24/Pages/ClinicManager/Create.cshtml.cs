/*  Create.cshtml.cs
 *  Version: 1.3 (2022.10.22)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
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
    public class CreateModel : PageModel
    {
        public class DataModel
        {
            public string CustomerCode { get; set; }

            //[Required(ErrorMessage = Constants.ERROR_EMPTY_FULLNAME)]
            public string FullName { get; set; }

            [DataType(DataType.MultilineText)]
            public string Address { get; set; }

            [DataType(DataType.PhoneNumber)]
            public string PhoneNumber { get; set; }

            [DataType(DataType.MultilineText)]
            public string Notes { get; set; }

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


        public CreateModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
        }


        public async Task OnGetAsync()
        {
            int count = await m_DbContext.CustomerProfiles.CountAsync();
            string nextCustomerCode = "BN" + (count + 1);

            Data = new DataModel()
            {
                CustomerCode = nextCustomerCode
            };

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
                    ActionRecord.Operation_.CreateCustomer,
                    ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { "err", ErrorMessage.InvalidModelState }
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
                    ActionRecord.Operation_.CreateCustomer,
                    ActionRecord.OperationStatus_.Failed,
                    Constants.ERROR_MANAGER_PASSWORD_INCORRECT
                );

                return Page();
            }
#endif

            var tokens = Utils.TokenizeName(Data.FullName);
            CustomerProfile customer = new CustomerProfile(Data.CustomerCode, currentUser.Id)
            {
                FirstMidName = tokens.Item1 + " " + tokens.Item2,
                LastName = tokens.Item3,
                Address = Data.Address,
                PhoneNumber = Data.PhoneNumber,
                Notes = Data.Notes,
            };
            m_DbContext.Add(customer);

            Dictionary<string, string> customInfo = ProcessUpload(customer);

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
                ActionRecord.Operation_.CreateCustomer,
                opStatus,
                customInfo
            );

            //await m_DbContext.SaveChangesAsync();

            return RedirectToPage("./Index");
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

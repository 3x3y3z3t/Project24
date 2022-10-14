/*  Create.cshtml.cs
 *  Version: 1.2 (2022.10.15)
 *
 *  Contributor
 *      Arime-chan
 */

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
    [Authorize(Roles = P24Role_.Manager)]
    [RequestFormLimits(MultipartBodyLengthLimit = 10L * 1024L * 1024L)]
    public class CreateModel : PageModel
    {
        public class DataModel
        {
            [Required(ErrorMessage = Constants.ERROR_EMPTY_CUSTOMER_ID)]
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
            int count = await m_DbContext.CustomerProfilesDev2.CountAsync();
            string nextCustomerCode = "BN" + (count + 1);

            Data = new DataModel()
            {
                CustomerCode = nextCustomerCode
            };

        }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            if (currentUser == null)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                //StatusMessage = "Error: Lỗi không xác định.";

                //m_Logger.LogError("Unknown Error during Create Service.");

                await Utils.RecordAction(
                    currentUser.UserName,
                    ActionRecord.Operation_.CreateCustomer,
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
                    ActionRecord.Operation_.CreateCustomer,
                    ActionRecord.OperationStatus_.Failed,
                    Constants.ERROR_MANAGER_PASSWORD_INCORRECT
                );

                return Page();
            }
#endif

            var tokens = Utils.TokenizeName(Data.FullName);
            CustomerProfileDev2 customer = new CustomerProfileDev2(Data.CustomerCode, currentUser.Id)
            {
                FirstMidName = tokens.Item1 + " " + tokens.Item2,
                LastName = tokens.Item3,
                Address = Data.Address,
                PhoneNumber = Data.PhoneNumber,
                Notes = Data.Notes,
            };
            m_DbContext.Add(customer);

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
                description = description.Remove(description.Length - 3);
                opStatus += ActionRecord.OperationStatus_.MalfunctionUploadAttempted;
            }

            description += "added=" + addedList;

            await Utils.RecordAction(
                currentUser.UserName,
                ActionRecord.Operation_.CreateCustomer,
                opStatus,
                "CustomerCode=" + customer.CustomerCode + ";" + description
            );

            return RedirectToPage("./Index");
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
    }

}

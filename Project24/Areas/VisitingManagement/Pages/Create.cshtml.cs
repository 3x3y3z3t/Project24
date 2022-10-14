/*  Create.cshtml.cs
 *  Version: 1.0 (2022.09.11)
 *
 *  Contributor
 *      Arime-chan
 */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project24.Data;
using Project24.Identity;
using Project24.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Project24.Pages.VisitingManagement
{
    [Authorize(Roles = Constants.ROLE_EMPLOYEE)]
    public class CreateModel : PageModel
    {
        public class DataModel
        {
            [Required(ErrorMessage = Constants.ERROR_EMPTY_VISITING_ID)]
            public string VisitingCode { get; set; }

            [Required(ErrorMessage = Constants.ERROR_EMPTY_FULLNAME)]
            public string CustomerName { get; set; }

            [DataType(DataType.MultilineText)]
            public string Address { get; set; }

            [DataType(DataType.PhoneNumber)]
            public string PhoneNumber { get; set; }

            [DataType(DataType.MultilineText)]
            public string ServicesUsed { get; set; }


            [Required(ErrorMessage = Constants.ERROR_MANAGER_PASSWORD_REQUIRED)]
            [DataType(DataType.Password)]
            public string ManagerPassword { get; set; }

            public DataModel()
            { }
        }

        [BindProperty]
        public DataModel Data { get; set; }
        public string StatusMessage { get; set; }

        public CreateModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager, ILogger<CreateModel> _logger)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
            m_Logger = _logger;

        }

        public async Task OnGetAsync()
        {
            int count = await m_DbContext.VisitingProfilesDev.CountAsync();
            ViewData["NextVisitingCode"] = "PK" + (count + 1);

            var data = (from _service in m_DbContext.ServicesDev
                        select new Tuple<string, string>(_service.ServiceCode, _service.Name));

            List<Tuple<string, string>> services = await data.ToListAsync();
            ViewData["AvailableServices"] = services;
        
        }

        public async Task<IActionResult> OnPostAsync()
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Page(); // this should not happens;
            }

            //if (ModelState.IsValid)
            //{

            //    if (!await m_UserManager.CheckPasswordAsync(currentUser, Data.ManagerPassword))
            //    {
            //        StatusMessage = "Error: " + Constants.ERROR_MANAGER_PASSWORD_INCORRECT;

            //        await Utils.RecordAction(
            //            m_DbContext,
            //            currentUser.UserName,
            //            ActionRecord.Operation_.CreateCustomer,
            //            ActionRecord.OperationStatus_.Failed,
            //            Constants.ERROR_MANAGER_PASSWORD_INCORRECT
            //        );

            //        return Page();
            //    }

            //    var tokens = Utils.TokenizeName(Data.FullName);

            //    CustomerDev customer = new CustomerDev()
            //    {
            //        CustomerCode = Data.CustomerCode,
            //        FirstMidName = tokens.Item1 + " " + tokens.Item2,
            //        LastName = tokens.Item3,
            //        Address = Data.Address,
            //        PhoneNumber = Data.PhoneNumber,
            //        AddedDate = DateTime.Now,
            //        UpdatedDate = DateTime.Now
            //    };

            //    m_DbContext.Add(customer);

            //    await m_DbContext.SaveChangesAsync();

            //    await Utils.RecordAction(
            //        m_DbContext,
            //        currentUser.UserName,
            //        ActionRecord.Operation_.CreateCustomer,
            //        ActionRecord.OperationStatus_.Success
            //    );

            //    return RedirectToPage("./Index");

            //    //foreach (var error in result.Errors)
            //    //{
            //    //    ModelState.AddModelError(string.Empty, error.Description);
            //    //}
            //}

            //// If we got this far, something failed, redisplay form
            //StatusMessage = "Error: Lỗi không xác định.";

            //m_Logger.LogError("Unknown Error during Create Service.");

            //await Utils.RecordAction(
            //    m_DbContext,
            //    currentUser.UserName,
            //    ActionRecord.Operation_.CreateUser,
            //    ActionRecord.OperationStatus_.Failed,
            //    Constants.ERROR_UNKNOWN_ERROR_FROM_PROGRAM
            //);

            return Page();
        }

        private readonly ApplicationDbContext m_DbContext;

        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly ILogger<CreateModel> m_Logger;
    }

}

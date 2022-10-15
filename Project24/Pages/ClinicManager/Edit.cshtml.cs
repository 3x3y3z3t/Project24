/*  Edit.cshtml.cs
 *  Version: 1.0 (2022.10.15)
 *
 *  Contributor
 *      Arime-chan
 */
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
    public class EditModel : PageModel
    {
        public class DataModel
        {
            [Required(ErrorMessage = Constants.ERROR_EMPTY_CUSTOMER_ID, AllowEmptyStrings = false)]
            public string CustomerCode { get; set; }

            //[Required(ErrorMessage = Constants.ERROR_EMPTY_FULLNAME)]
            public string FullName { get; set; }

            [DataType(DataType.MultilineText)]
            public string Address { get; set; }

            [DataType(DataType.PhoneNumber)]
            public string PhoneNumber { get; set; }

            [DataType(DataType.MultilineText)]
            public string Notes { get; set; }


            //[Required(ErrorMessage = Constants.ERROR_MANAGER_PASSWORD_REQUIRED)]
            [DataType(DataType.Password)]
            public string ManagerPassword { get; set; }

            public DataModel()
            { }
        }

        [BindProperty]
        public DataModel Data { get; set; }


        public EditModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
        }


        public async Task<IActionResult> OnGetAsync(string? _code)
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            if (currentUser == null)
                return BadRequest();

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
                    ActionRecord.Operation_.UpdateCustomer,
                    ActionRecord.OperationStatus_.UnexpectedError,
                    "code=" + _code + ";count=" + customers.Count()
                );

                TempData["Error"] = "error";
                TempData["CustomerCode"] = _code;
                return Page();
            }

            var customer = customers.First();


            // ============

            //TempData["Error"] = "deleted";
            //TempData["CustomerCode"] = _code;
            //TempData["DeletedOn"] = DateTime.Now.ToString();
            //TempData["DeletedBy"] = customer.UpdatedUser.UserName;

            // ============


            if (customer.DeletedDate != DateTime.MinValue)
            {
                await Utils.RecordAction(
                    currentUser.UserName,
                    ActionRecord.Operation_.UpdateCustomer,
                    ActionRecord.OperationStatus_.Failed,
                    "Already deleted on " + customer.DeletedDate.ToString()
                );

                TempData["Error"] = "deleted";
                TempData["CustomerCode"] = _code;
                TempData["DeletedOn"] = customer.DeletedDate.ToString();
                TempData["DeletedBy"] = customer.UpdatedUser.UserName;
                return Page();
            }

            Data = new DataModel()
            {
                CustomerCode = customer.CustomerCode,
                FullName = customer.FullName,
                Address = customer.Address,
                PhoneNumber = customer.PhoneNumber,
                Notes = customer.Notes
            };

            return Page();
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
                    ActionRecord.Operation_.UpdateCustomer,
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
                    ActionRecord.Operation_.UpdateCustomer,
                    ActionRecord.OperationStatus_.Failed,
                    Constants.ERROR_MANAGER_PASSWORD_INCORRECT
                );

                return Page();
            }
#endif

            var customer = await m_DbContext.CustomerProfilesDev2.FirstOrDefaultAsync(_customer => _customer.CustomerCode == Data.CustomerCode);
            if (customer == null)
            {
                await Utils.RecordAction(
                    currentUser.UserName,
                    ActionRecord.Operation_.UpdateCustomer,
                    ActionRecord.OperationStatus_.Failed,
                    Constants.ERROR_NOT_FOUND_CUSTOMER + ";" + Data.CustomerCode
                );

                return await OnGetAsync(Data.CustomerCode);
            }

            var tokens = Utils.TokenizeName(Data.FullName);
            customer.FirstMidName = tokens.Item1 + " " + tokens.Item2;
            customer.LastName = tokens.Item3;
            customer.Address = Data.Address;
            customer.PhoneNumber = Data.PhoneNumber;
            customer.Notes = Data.Notes;

            customer.UpdatedDate = DateTime.Now;
            customer.UpdatedUser = currentUser;

            m_DbContext.Update(customer);

            await m_DbContext.SaveChangesAsync();


            await Utils.RecordAction(
                currentUser.UserName,
                ActionRecord.Operation_.UpdateCustomer,
                ActionRecord.OperationStatus_.Success,
                "CustomerCode=" + customer.CustomerCode
            );

            return RedirectToPage("./Index");
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
    }

}

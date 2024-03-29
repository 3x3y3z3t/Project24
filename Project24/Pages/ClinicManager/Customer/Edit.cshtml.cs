/*  P24/Customer/Edit.cshtml
 *  Version: 1.8 (2023.02.14)
 *
 *  Contributor
 *      Arime-chan
 */

//#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project24.App.Extension;
using Project24.Data;
using Project24.Models;
using Project24.Models.ClinicManager;
using Project24.Models.ClinicManager.DataModel;
using Project24.Models.Identity;
using Project24.Utils.ClinicManager;

namespace Project24.Pages.ClinicManager.Customer
{
    [Authorize(Roles = P24RoleName.Manager)]
    public class EditModel : PageModel
    {
        [BindProperty]
        public P24EditCustomerFormDataModel FormData { get; set; }


        public EditModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
        }


        public async Task<IActionResult> OnGetAsync(string _code)
        {
            if (string.IsNullOrEmpty(_code))
                return Partial("_CommonNotFound", new CommonNotFoundModel(P24Constants.Customer, "null", "List"));

            var deleted = await (from _customer in m_DbContext.CustomerProfiles
                                  where _customer.Code == _code && _customer.DeletedDate != DateTime.MinValue
                                  select _customer.Code)
                           .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(deleted))
                return RedirectToPage("Details", new { _code = _code });

            var customer = from _customer in m_DbContext.CustomerProfiles
                            where _customer.Code == _code
                            select new P24EditCustomerFormDataModel()
                            {
                                Code = _customer.Code,
                                Fullname = _customer.FullName,
                                Gender = _customer.Gender,
                                DoB = _customer.DateOfBirth,
                                PhoneNumber = _customer.PhoneNumber,
                                Address = _customer.Address,
                                Note = _customer.Note
                            };

            FormData = await customer.FirstOrDefaultAsync();
            if (FormData == null)
                return Partial("_CommonNotFound", new CommonNotFoundModel(P24Constants.Customer, _code, "List"));

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);

            if (!await this.ValidateModelState(m_DbContext, currentUser, ActionRecord.Operation_.UpdateCustomer))
                return Page();

            var customer = await (from _customer in m_DbContext.CustomerProfiles.Include(_c => _c.PreviousVersion)
                                  where _customer.Code == FormData.Code
                                  select _customer)
                           .FirstOrDefaultAsync();

            if (customer == null)
                return BadRequest();

            if (customer.DeletedDate != DateTime.MinValue)
            {
                await m_DbContext.RecordChanges(
                    currentUser.UserName,
                    ActionRecord.Operation_.UpdateCustomer,
                    ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.CustomerCode, FormData.Code },
                        { CustomInfoKey.Message, ErrorMessage.CustomerDeleted },
                    }
                );

                return RedirectToPage("Details", new { _code = FormData.Code });
            }

            P24ObjectPreviousVersion previousVersion = customer.ConstructCurrentVersionObject();
            await m_DbContext.AddAsync(previousVersion);
            
            var tokens = P24Utils.SplitFirstLastName(FormData.Fullname);
            customer.FirstMidName = tokens.Item1;
            customer.LastName = tokens.Item2;
            customer.Gender = FormData.Gender;
            customer.DateOfBirth = FormData.DoB;
            customer.PhoneNumber = FormData.PhoneNumber;
            customer.Address = FormData.Address;
            customer.Note = FormData.Note;
            customer.PreviousVersion = previousVersion;
            customer.EditedDate = DateTime.Now;
            customer.EditedUser = currentUser;

            m_DbContext.Update(customer);

            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.UpdateCustomer,
                ActionRecord.OperationStatus_.Success,
                new Dictionary<string, string>()
                {
                    { CustomInfoKey.CustomerCode, FormData.Code }
                }
            );

            return RedirectToPage("Details", new { _code = FormData.Code });
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
    }

}

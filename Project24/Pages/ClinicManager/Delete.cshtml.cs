/*  Delete.cshtml.cs
 *  Version: 1.1 (2022.10.22)
 *
 *  Contributor
 *      Arime-chan
 */
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

using System;
using System.Collections.Generic;
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
    [Authorize(Roles = P24RoleName.Manager)]
    public class DeleteModel : PageModel
    {
        public CustomerProfile CustomerProfile { get; set; }
        public List<CustomerImage> CustomerImages { get; set; }


        public DeleteModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
        }


        public async Task<IActionResult> OnGetAsync(string? _code)
        {
            if (_code == null)
                return NotFound();

            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            if (currentUser == null)
                return BadRequest();

            var customers = from _customers in m_DbContext.CustomerProfiles.Include(_c => _c.AddedUser).Include(_c => _c.UpdatedUser)
                            where _customers.CustomerCode == _code
                            select _customers;

            if (customers.Count() <= 0)
                return BadRequest();

            CustomerProfile = customers.First();

            if (CustomerProfile.DeletedDate != DateTime.MinValue)
            {
                await m_DbContext.RecordChanges(
                    currentUser.UserName,
                    ActionRecord.Operation_.DeleteCustomer,
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

        public async Task<IActionResult> OnPostAsync(string? _code)
        {
            if (_code == null)
                return BadRequest();

            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            if (currentUser == null || !ModelState.IsValid)
                return BadRequest();

            var customers = from _customers in m_DbContext.CustomerProfiles.Include(_c => _c.AddedUser).Include(_c => _c.UpdatedUser)
                            where _customers.CustomerCode == _code
                            select _customers;

            if (customers.Count() <= 0)
                return BadRequest();

            var customer = customers.First();

            if (customer.DeletedDate != DateTime.MinValue)
            {
                await m_DbContext.RecordChanges(
                    currentUser.UserName,
                    ActionRecord.Operation_.DeleteCustomer,
                    ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.Message, "Already deleted on " + customer.DeletedDate.ToString() }
                    }
                );

                return RedirectToPage("./Index");
            }

            var images = from _images in m_DbContext.CustomerImages
                         where _images.OwnedCustomerId == customer.Id && _images.DeletedDate == DateTime.MinValue
                         select _images;

            customer.DeletedDate = DateTime.Now;
            customer.UpdatedUser = currentUser;
            m_DbContext.Update(customer);

            foreach(var image in images.ToList())
            {
                image.DeletedDate = DateTime.Now;
            }
            m_DbContext.UpdateRange(images);

            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.DeleteCustomer,
                ActionRecord.OperationStatus_.Success,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.CustomerCode, customer.CustomerCode }
                    }
            );

            //await m_DbContext.SaveChangesAsync();

            return RedirectToPage("./Index");
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
    }

}

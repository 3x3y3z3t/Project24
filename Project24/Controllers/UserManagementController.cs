/*  UserManagementController.cs
 *  Version: 1.0 (2022.09.03)
 *
 *  Contributor
 *      Arime-chan
 */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project24.Data;
using Project24.Identity;
using Project24.Models;
using Project24.Pages.UserManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project24.Controllers
{
#if false
    [Authorize]
    public class UserManagement2Controller : Controller
    {
        private readonly ApplicationDbContext m_AppDbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;

        public UserManagement2Controller(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager)
        {
            m_AppDbContext = _context;
            m_UserManager = _userManager;
        }

        // GET: UserManagement
        [Authorize(Roles = Constants.ROLE_MANAGER)]
        public async Task<IActionResult> Index()
        {
            //if (!User.Identity.IsAuthenticated || !User.IsInRole(Constants.ROLE_MANAGER))
            //{
            //    return View("_CommonAccessDenied", new AccessDeniedModel());
            //}

            return View(await m_AppDbContext.P24Users.ToListAsync());
        }

        // GET: UserManagement/Details?_username=usn
        [Authorize(Roles = Constants.ROLE_EMPLOYEE)]
        public async Task<IActionResult> Details(string _username)
        {
            if (string.IsNullOrEmpty(_username))
                return View("_CommonAccessDenied", new AccessDeniedModel("User " + _username + " không tồn tại."));

            P24IdentityUser user = await m_AppDbContext.P24Users
                .FirstOrDefaultAsync((_user) => (_user.UserName == _username));

            if (user == null)
                return NotFound();

            return View(user);
        }

        // GET: UserManagemanet/AddUser
        [Authorize(Roles = Constants.ROLE_MANAGER)]
        public async Task<IActionResult> AddUser()
        {
            //var lastCustomer = await _context.Customer.OrderByDescending(_customer => _customer.Id).FirstOrDefaultAsync();
            //Console.WriteLine(lastCustomer.Id);


            //ViewData["NextPatientId"] = lastCustomer.Id;

            return View("AddUser");
        }


        //    // POST: UserManagement/AddUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Constants.ROLE_MANAGER)]
        //    // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        //    // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        public async Task<IActionResult> AddUser([Bind("Username,Fullname")] AddUserModel.DataModel _input)
        {
            if (ModelState.IsValid)
            {




            }


            //        if (ModelState.IsValid)
            //        {
            //            _context.Add(customer);
            //            await _context.SaveChangesAsync();
            //            return RedirectToAction(nameof(Index));
            //        }
                    return View(_input);
        }

    }
#endif

}


    //public class CustomersController : Controller
    //{





    //    // GET: Customers/Edit/5
    //    public async Task<IActionResult> Edit(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return NotFound();
    //        }

    //        var customer = await _context.Customer.FindAsync(id);
    //        if (customer == null)
    //        {
    //            return NotFound();
    //        }
    //        return View(customer);
    //    }

    //    // POST: Customers/Edit/5
    //    // To protect from overposting attacks, enable the specific properties you want to bind to, for 
    //    // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public async Task<IActionResult> Edit(int id, [Bind("Id,UniqueId,Name,Address,CustomerProfileId")] Customer customer)
    //    {
    //        if (id != customer.Id)
    //        {
    //            return NotFound();
    //        }

    //        if (ModelState.IsValid)
    //        {
    //            try
    //            {
    //                _context.Update(customer);
    //                await _context.SaveChangesAsync();
    //            }
    //            catch (DbUpdateConcurrencyException)
    //            {
    //                if (!CustomerExists(customer.Id))
    //                {
    //                    return NotFound();
    //                }
    //                else
    //                {
    //                    throw;
    //                }
    //            }
    //            return RedirectToAction(nameof(Index));
    //        }
    //        return View(customer);
    //    }

    //    // GET: Customers/Delete/5
    //    public async Task<IActionResult> Delete(int? id)
    //    {
    //        if (id == null)
    //        {
    //            return NotFound();
    //        }

    //        var customer = await _context.Customer
    //            .FirstOrDefaultAsync(m => m.Id == id);
    //        if (customer == null)
    //        {
    //            return NotFound();
    //        }

    //        return View(customer);
    //    }

    //    // POST: Customers/Delete/5
    //    [HttpPost, ActionName("Delete")]
    //    [ValidateAntiForgeryToken]
    //    public async Task<IActionResult> DeleteConfirmed(int id)
    //    {
    //        var customer = await _context.Customer.FindAsync(id);
    //        _context.Customer.Remove(customer);
    //        await _context.SaveChangesAsync();
    //        return RedirectToAction(nameof(Index));
    //    }

    //    private bool CustomerExists(int id)
    //    {
    //        return _context.Customer.Any(e => e.Id == id);
    //    }
    //}


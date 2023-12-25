/*  Areas/Identity/Pages/Account/Manage/Index.cshtml.cs
 *  Version: v1.1 (2023.12.24)
 *  Spec:    v0.1
 *  
 *  Contributor
 *      The .NET Foundation (Author)
 *      Arime-chan
 */
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project24.Data;
using Project24.Model.Identity;

namespace Project24.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }


        public string Username { get; set; }
        public int AccessCount { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }


        public IndexModel(SignInManager<P24IdentityUser> _signInManager, UserManager<P24IdentityUser> _userManager, ApplicationDbContext _dbContext)
        {
            m_SignInManager = _signInManager;
            m_UserManager = _userManager;
            m_DbContext = _dbContext;
        }


        private async Task LoadAsync(P24IdentityUser _user)
        {
            var userName = await m_UserManager.GetUserNameAsync(_user);
            //var phoneNumber = await m_UserManager.GetPhoneNumberAsync(_user);

            var accessCount = await (from _role in m_DbContext.UserRoles where  _role.UserId == _user.Id select _role.RoleId).CountAsync();

            Username = userName;
            AccessCount = accessCount;

            //Input = new InputModel
            //{
            //    PhoneNumber = phoneNumber
            //};
        }

        public async Task<IActionResult> OnGetAsync()
        {
            P24IdentityUser user = await m_UserManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{m_UserManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return NotFound();

            var user = await m_UserManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{m_UserManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await m_UserManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await m_UserManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            await m_SignInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }


        private readonly SignInManager<P24IdentityUser> m_SignInManager;
        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly ApplicationDbContext m_DbContext;
    }

}

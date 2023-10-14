/*  Areas/Identity/Pages/Account/Manage/PersonalData.cshtml.cs
 *  Version: v1.0 (2023.10.13)
 *  
 *  Author
 *      The .NET Foundation
 *  
 *  Contributor
 *      Arime-chan
 */
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.Model.Identity;

namespace Project24.Areas.Identity.Pages.Account.Manage
{
    public class PersonalDataModel : PageModel
    {
        public PersonalDataModel(UserManager<P24IdentityUser> _userManager, ILogger<PersonalDataModel> _logger)
        {
            m_UserManager = _userManager;
            m_Logger = _logger;
        }


        public async Task<IActionResult> OnGet()
        {
            return NotFound();

            var user = await m_UserManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{m_UserManager.GetUserId(User)}'.");
            }

            return Page();
        }


        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly ILogger<PersonalDataModel> m_Logger;
    }

}

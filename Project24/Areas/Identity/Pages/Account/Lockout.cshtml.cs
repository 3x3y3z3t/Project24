/*  Areas/Identity/Pages/Account/Lockout.cshtml.cs
 *  Version: v1.0 (2023.10.08)
 *  
 *  Author
 *      The .NET Foundation
 *  
 *  Contributor
 *      Arime-chan
 */
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Project24.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LockoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            return NotFound();
        }
    }

}

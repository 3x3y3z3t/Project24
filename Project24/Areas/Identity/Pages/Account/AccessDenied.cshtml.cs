/*  Areas/Identity/Pages/Account/AccessDenied.cshtml.cs
 *  Version: v1.1 (2023.10.29)
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

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Project24.Areas.Identity.Pages.Account
{
    public class AccessDeniedModel : PageModel
    {
        public string ReturnUrl { get; set; }


        public void OnGet(string returnUrl = null)
        {
            if (returnUrl == null)
                returnUrl = Url.Content("~/");
            else
            {
                int pos = returnUrl.IndexOf('?');
                if (pos >= 0)
                    returnUrl = returnUrl.Substring(0, pos);
            }

            ReturnUrl = returnUrl;
        }
    }

}

/*  Identity/Account/Login.cshtml.cs
 *  Version: 1.1 (2022.12.11)
 *
 *  Contributor
 *      Arime-chan
 */

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.Data;
using Project24.Models;
using Project24.Models.Identity;

namespace Project24.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        public class DataModel
        {
            [Required]
            public string Username { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            public bool RememberLogin { get; set; }

            public DataModel()
            { }
        }

        [BindProperty]
        public DataModel FormData { get; set; }

        public string ReturnUrl { get; set; }

        public string StatusMessage { get; private set; }


        public LoginModel(ApplicationDbContext _dbContext, SignInManager<P24IdentityUser> _signInManager, ILogger<LoginModel> _logger)
        {
            m_DbContext = _dbContext;
            m_SignInManager = _signInManager;
            m_Logger = _logger;
        }


        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            if (returnUrl == null)
                returnUrl = Url.Content("~/");

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return LocalRedirect("/");
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ReturnUrl = returnUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (returnUrl == null)
                returnUrl = Url.Content("~/");

            ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                await m_DbContext.RecordChanges(
                    FormData.Username,
                    ActionRecord.Operation_.Account_AttemptLogin,
                    ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.Error, ErrorMessage.InvalidModelState }
                    }
                );

                return Page();
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await m_SignInManager.PasswordSignInAsync(FormData.Username, FormData.Password, FormData.RememberLogin, false);
            if (!result.Succeeded)
            {
                await m_DbContext.RecordChanges(
                    FormData.Username,
                    ActionRecord.Operation_.Account_AttemptLogin,
                    ActionRecord.OperationStatus_.Failed
                );
                m_Logger.LogInformation("User " + FormData.Username + "login failed.");

                StatusMessage = CustomInfoTag.Error + "Đăng nhập thất bại.";

                return Page();
            }

            await m_DbContext.RecordChanges(
                FormData.Username,
                ActionRecord.Operation_.Account_AttemptLogin,
                ActionRecord.OperationStatus_.Success
            );
            m_Logger.LogInformation("User " + FormData.Username + "logged in.");

            return LocalRedirect(returnUrl);
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly SignInManager<P24IdentityUser> m_SignInManager;
        private readonly ILogger<LoginModel> m_Logger;
    }

}

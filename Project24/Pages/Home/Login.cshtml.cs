/*  Login.cshtml.cs
 *  Version: 1.2 (2022.10.03)
 *
 *  Contributor
 *      Arime-chan
 */

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.Data;
using Project24.Identity;
using Project24.Models;

namespace Project24.Pages.Home
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        public class DataModel
        {
            [Required(ErrorMessage = Constants.ERROR_EMPTY_USERNAME)]
            public string Username { get; set; }

            [Required(ErrorMessage = Constants.ERROR_EMPTY_PASWORD)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            public bool RememberLogin { get; set; }


            public DataModel()
            { }
        }

        [BindProperty]
        public DataModel Data { get; set; } = null;

        public string ErrorMessage { get; set; }


        public LoginModel(SignInManager<P24IdentityUser> _signInManager, ILogger<LoginModel> _logger)
        {
            m_SignInManager = _signInManager;
            m_Logger = _logger;
        }

        public async Task OnGetAsync()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await m_SignInManager.PasswordSignInAsync(Data.Username, Data.Password, Data.RememberLogin, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    await Utils.RecordAction(ApplicationDbContext.Instance,
                        Data.Username,
                        ActionRecord.Operation_.AttemptLogin,
                        ActionRecord.OperationStatus_.Success
                    );

                    return LocalRedirect("/");
                }

                await Utils.RecordAction(ApplicationDbContext.Instance,
                    Data.Username,
                    ActionRecord.Operation_.AttemptLogin,
                    ActionRecord.OperationStatus_.Failed
                );

                ErrorMessage = "Đăng nhập thất bại.";

                return Page();
            }

            await Utils.RecordAction(ApplicationDbContext.Instance,
                Data.Username,
                ActionRecord.Operation_.AttemptLogin,
                ActionRecord.OperationStatus_.UnexpectedError,
                "Invalid Model State"
            );

            // If we got this far, something failed, redisplay form
            return Page();

        }

        private readonly SignInManager<P24IdentityUser> m_SignInManager;
        private readonly ILogger<LoginModel> m_Logger;
    }

}

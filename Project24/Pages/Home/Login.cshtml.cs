/*  Login.cshtml.cs
 *  Version: 1.3 (2022.10.21)
 *
 *  Contributor
 *      Arime-chan
 */

using System.Collections.Generic;
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

        public string StatusMessage { get; protected set; }


        public LoginModel(ApplicationDbContext _dbContext, SignInManager<P24IdentityUser> _signInManager, ILogger<LoginModel> _logger)
        {
            m_DbContext = _dbContext;
            m_SignInManager = _signInManager;
            m_Logger = _logger;
        }


        public async Task OnGetAsync()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await m_DbContext.RecordChanges(
                    Data.Username,
                    ActionRecord.Operation_.AttemptLogin,
                    ActionRecord.OperationStatus_.UnexpectedError,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.Error, Project24.ErrorMessage.InvalidModelState }
                    }
                );

                return Page();
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await m_SignInManager.PasswordSignInAsync(Data.Username, Data.Password, Data.RememberLogin, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                await m_DbContext.RecordChanges(
                    Data.Username,
                    ActionRecord.Operation_.AttemptLogin,
                    ActionRecord.OperationStatus_.Success
                );

                return LocalRedirect("/");
            }

            await m_DbContext.RecordChanges(
                Data.Username,
                ActionRecord.Operation_.AttemptLogin,
                ActionRecord.OperationStatus_.Failed
            );

            StatusMessage = "Đăng nhập thất bại.";

            return Page();
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly SignInManager<P24IdentityUser> m_SignInManager;
        private readonly ILogger<LoginModel> m_Logger;
    }

}

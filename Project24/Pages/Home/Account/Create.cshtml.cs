/*  Home/Account/List.cshtml
 *  Version: v1.0 (2023.12.24)
 *  Spec:    v0.1
 *  
 *  Contributor
 *      Arime-chan (Author)
 */

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.App;
using Project24.App.Utils;
using Project24.Data;
using Project24.Model;
using Project24.Model.Identity;

namespace Project24.Pages.Home.Account
{
    [Authorize(Roles = PageCollection.Home.Account.Create)]
    public class CreateModel : PageModel
    {
        // TODO: Viewmodel
        public class InputModel
        {
            //[Required(AllowEmptyStrings = false)]
            public string Username { get; set; }

            public string FirstName { get; set; }
            public string LastName { get; set; }

            [DataType(DataType.Password)]
            public string CreatorPassword { get; set; }

            public InputModel()
            { }
        }


        [BindProperty]
        public InputModel Input { get; set; }


        public CreateModel(UserManager<P24IdentityUser> _userManager, ApplicationDbContext _dbContext, ILogger<CreateModel> _logger)
        {
            m_UserManager = _userManager;
            m_DbContext = _dbContext;
            m_Logger = _logger;
        }


        public void OnGet()
        { }

        #region AJAX Handler
        // ==================================================
        // AJAX Handler

        public IActionResult OnGetSuggestUsername(string _proposedUsername)
        {
            if (string.IsNullOrWhiteSpace(_proposedUsername))
                return Content(MessageTag.Error + "Username is empty, no suggestion.", MediaTypeNames.Text.Plain);

            int lastAlphaPos = _proposedUsername.Length - 1;
            while (lastAlphaPos >= 0)
            {
                if (!char.IsDigit(_proposedUsername[lastAlphaPos]))
                {
                    lastAlphaPos += 1;
                    break;
                }

                --lastAlphaPos;
            }
            string usn = _proposedUsername[0..lastAlphaPos];

            if (usn.ToLower() == "power")
                return Content(MessageTag.Error + "Username " + usn + " has been taken.", MediaTypeNames.Text.Plain);

            var usernames = (from _user in m_DbContext.P24Users
                             where _user.UserName.StartsWith(usn)
                             select _user.UserName)
                            .ToList();

            if (usernames.Count <= 0 || !usernames.Contains(_proposedUsername))
                return Content(MessageTag.Success + _proposedUsername, MediaTypeNames.Text.Plain);

            for (int i = 1; i < int.MaxValue; ++i)
            {
                if (!usernames.Contains(usn + i))
                    return Content(MessageTag.Success + usn + i, MediaTypeNames.Text.Plain);
            }

            m_Logger.LogWarning("There are too many user with username {_username}. This should not happens!", usn);
            return Content(MessageTag.Error + "There are too many user with username " + usn + ". This should not happens!", MediaTypeNames.Text.Plain);
        }

        public async Task<IActionResult> OnPostAsync([FromBody] string _username)
        {
            P24IdentityUser user = await m_UserManager.GetUserAsync(User);
            if (!this.ValidateModelState(m_DbContext, user, UserAction.Operation_.Home_Account_Create))
            {
                return Content(MessageTag.Error + "Invalid ModelState.", MediaTypeNames.Text.Plain);
            }

            var usernames = (from _user in m_DbContext.P24Users
                             where _user.UserName == _username
                             select _user.UserName)
                            .ToList();

            if (usernames.Count > 0 || _username.ToLower().StartsWith("power"))
            {
                string msg = "Username " + _username + " has been taken";
                m_DbContext.RecordUserAction(
                    user.UserName,
                    UserAction.Operation_.Home_Account_Create,
                    UserAction.OperationStatus_.Failed,
                    new Dictionary<string, string>() { { CustomInfoKeys.Error, msg } }
                );

                return Content(MessageTag.Error + msg + ".", MediaTypeNames.Text.Plain);
            }

            // TODO: create user;
            P24IdentityUser newUser = new()
            {
                UserName = _username,

                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
            };

            IdentityResult result = await m_UserManager.CreateAsync(newUser, Constants.DefaultPassword);
            if (!result.Succeeded)
            {
                m_DbContext.RecordUserAction(
                    user.UserName,
                    UserAction.Operation_.Home_Account_Create,
                    UserAction.OperationStatus_.Failed,
                    new Dictionary<string, string>() { { CustomInfoKeys.Error, JsonSerializer.Serialize(result.Errors) } }
                );

                return Content(MessageTag.Error, MediaTypeNames.Text.Plain);
            }

            m_DbContext.RecordUserAction(
                user.UserName,
                UserAction.Operation_.Home_Account_Create,
                UserAction.OperationStatus_.Success,
                new Dictionary<string, string>() { { CustomInfoKeys.Details, _username } }
            );

            return Content(MessageTag.Success + "Account <code>" + _username + "</code> has been created with default password.", MediaTypeNames.Text.Plain);
        }

        // End: AJAX Handler
        // ==================================================
        #endregion


        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly ApplicationDbContext m_DbContext;
        private readonly ILogger<CreateModel> m_Logger;
    }

}

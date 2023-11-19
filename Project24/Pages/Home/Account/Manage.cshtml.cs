/*  Home/Account/List.cshtml
 *  Version: v1.0 (2023.11.19)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.App;
using Project24.App.Utils;
using Project24.App.Utils.Identity;
using Project24.Data;
using Project24.Model;
using Project24.Model.Identity;
using Project24.Serializer;

namespace Project24.Pages.Home.Account
{
    [Authorize(Roles = PageCollection.Home.Account.Manage)]// TODO: role = Manage;
    public class ManageModel : PageModel
    {
        internal class UserManageViewModel
        {
            public P24IdentityUserViewModel User { get; set; }

            public Dictionary<string, bool> Roles { get; set; }

            public UserManageViewModel()
            { }
        }

        internal class P24IdentityUserViewModel
        {
            public string Id { get; set; }
            public string Username { get; set; }
            public DateTime? AddedDate { get; set; }
            public DateTime? RemovedDate { get; set; }

            public string DisplayName { get; set; }
            public string Email { get; set; }
            public bool EmailConfirmed { get; set; }
            public string PhoneNumber { get; set; }
            public bool PhoneNumberConfirmed { get; set; }

            public P24IdentityUserViewModel()
            { }
        }


        public ManageModel(UserManager<P24IdentityUser> _userManager, ApplicationDbContext _dbContext, ILogger<ManageModel> _logger)
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

        // ajax handler;
        public IActionResult OnGetFetchPageData(string _id)
        {
            if (string.IsNullOrWhiteSpace(_id))
                return Content(MessageTag.Error, MediaTypeNames.Text.Plain);

            P24IdentityUser user = m_UserManager.FindByIdAsync(_id).Result;
            if (user == null)
                return Content(MessageTag.Error + _id, MediaTypeNames.Text.Plain);

            P24IdentityUserViewModel userView = new()
            {
                Id = _id,
                Username = user.UserName,
                AddedDate = user.AddedDateTime != DateTime.MaxValue ? user.AddedDateTime : null,
                RemovedDate = user.RemovedDateTime != DateTime.MaxValue ? user.RemovedDateTime : null,
                DisplayName = user.FullName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            };

            List<string> userRoles = (List<string>)m_UserManager.GetRolesAsync(user).Result;

            Dictionary<string, bool> roles = new();
            foreach (string role in P24RoleUtils.AllRoleNames)
            {
                roles[role] = userRoles.Contains(role);
            }

            UserManageViewModel model = new()
            {
                User = userView,
                Roles = roles,
            };

            JsonSerializerOptionsFactory.Options options = JsonSerializerOptionsFactory.Options.UseFullRangeUnicode;

            string jsonData = JsonSerializer.Serialize(model, JsonSerializerOptionsFactory.Get(options));
            return Content(MessageTag.Success + jsonData, MediaTypeNames.Text.Plain);
        }

        public IActionResult OnPostUpdateRole([FromBody] Dictionary<string, bool> _modifiedList)
        {
            P24IdentityUser user = m_UserManager.FindByNameAsync(User.Identity.Name).Result;
            if (!this.ValidateModelState(m_DbContext, user, UserAction.Operation_.Home_Account_Manage_UpdateRole))
            {
                return Content(MessageTag.Error + "Invalid ModelState.", MediaTypeNames.Text.Plain);
            }

            Dictionary<string, bool> resultsList = new(_modifiedList.Count);
            string details = "";
            bool hasRoleChanges = false;
            foreach (var pair in _modifiedList)
            {
                if (!P24RoleUtils.AllRoleNames.Contains(pair.Key))
                {
                    m_Logger.LogWarning("Client sent invalid role name: {_name}", pair.Key);
                    // TODO: add into error;
                    continue;
                }

                if (pair.Value && !m_UserManager.IsInRoleAsync(user, pair.Key).Result)
                {
                    var result = m_UserManager.AddToRoleAsync(user, pair.Key).Result;
                    if (result.Succeeded)
                    {
                        resultsList[pair.Key] = true;
                        details += pair.Key + ": " + !pair.Value + " -> " + pair.Value;
                        hasRoleChanges = true;
                    }
                    else
                    {
                        resultsList[pair.Key] = false;
                        details += pair.Key + ": " + !pair.Value + " --";
                    }
                }

                if (!pair.Value && m_UserManager.IsInRoleAsync(user, pair.Key).Result)
                {
                    var result = m_UserManager.RemoveFromRoleAsync(user, pair.Key).Result;
                    if (result.Succeeded)
                    {
                        resultsList[pair.Key] = true;
                        details += pair.Key + ": " + !pair.Value + " -> " + pair.Value;
                        hasRoleChanges = true;
                    }
                    else
                    {
                        resultsList[pair.Key] = false;
                        details += pair.Key + ": " + !pair.Value + " --";
                    }
                }
            }

            Dictionary<string, string> customInfo = new()
            {
                { CustomInfoKeys.Details, details }
            };
            m_DbContext.RecordUserAction(user.UserName, UserAction.Operation_.Home_Account_Manage_UpdateRole, UserAction.OperationStatus_.Success, customInfo);

            if (hasRoleChanges)
            {
                _ = P24RoleUtils.RolesDirtyUser.Add(user.UserName);
            }

            string jsonData = JsonSerializer.Serialize(resultsList);
            return Content(MessageTag.Success + jsonData, MediaTypeNames.Text.Plain);
        }

        // End: AJAX Handler
        // ==================================================
        #endregion






        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly ApplicationDbContext m_DbContext;
        private readonly ILogger<ManageModel> m_Logger;
    }

}

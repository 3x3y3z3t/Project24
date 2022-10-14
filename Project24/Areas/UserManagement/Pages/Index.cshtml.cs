/*  Index.cshtml.cs
 *  Version: 1.1 (2022.09.07)
 *
 *  Contributor
 *      Arime-chan
 */
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.Data;
using Project24.Identity;

namespace Project24.Pages.UserManagement
{
    [Authorize(Roles = Constants.ROLE_EMPLOYEE)]
    public class IndexModel : PageModel
    {
        public class DataModel
        {
            public class P24UserViewModel
            {
                public string Username { get; set; }
                public string FullName { get; set; }
                public string AttendanceProfileId { get; set; }
                public string Role { get; set; }

                public P24UserViewModel()
                { }
            }

            public List<P24UserViewModel> EmployeeList { get; set; }

            //public List<P24UserViewModel> ManagerList { get; set; }

            public List<string> RawStrings { get; set; }

            public DataModel()
            {
                EmployeeList = new List<P24UserViewModel>();
                //ManagerList = new List<P24UserViewModel>();
            }
        }

        [BindProperty]
        public DataModel Data { get; private set; }

        public string ReturnUrl { get; set; }

        public IndexModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager, ILogger<IndexModel> _logger)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
            m_Logger = _logger;

            m_UsersAndRoles = new Dictionary<string, Role>();
        }

        public async Task OnGetAsync()
        {
            var usersAndRoles = from _users in m_DbContext.P24Users
                                join _userRoles in m_DbContext.UserRoles on _users.Id equals _userRoles.UserId
                                join _roles in m_DbContext.P24Roles on _userRoles.RoleId equals _roles.Id
                                select new
                                {
                                    _users.UserName,
                                    _users.FamilyName,
                                    _users.MiddleName,
                                    _users.LastName,
                                    _users.AttendanceProfileId,
                                    _roles.Name,
                                    _roles.Level,
                                };

            foreach (var user in usersAndRoles)
            {
                if (!m_UsersAndRoles.ContainsKey(user.UserName) || m_UsersAndRoles[user.UserName].Level > user.Level)
                    m_UsersAndRoles[user.UserName] = new Role(user.Name, user.Level);
            }

            Data = new DataModel();
            Data.EmployeeList = new List<DataModel.P24UserViewModel>();

            foreach (var user in usersAndRoles)
            {
                if (!m_UsersAndRoles.ContainsKey(user.UserName))
                    continue;

                Data.EmployeeList.Add(new DataModel.P24UserViewModel()
                {
                    Username = user.UserName,
                    FullName = Utils.ConstructFullName(user.FamilyName, user.MiddleName, user.LastName),
                    AttendanceProfileId = user.AttendanceProfileId,
                    Role = m_UsersAndRoles[user.UserName].Name,
                });
                m_UsersAndRoles.Remove(user.UserName);
            }



            //var allUser = m_AppDbContext.P24User
            //    .Join(m_AppDbContext.UserRoles, _p24User => _p24User.Id, _userRole => _userRole.UserId, (_p24User, _userRole) => new { _p24User, _userRole })
            //    .Join(m_AppDbContext.Roles, _userAndRole => _userAndRole._userRole.RoleId, _role => _role.Id, (_userAndRole, _role) => new { _userAndRole, _role })
            //    .ToList()
            //    .GroupBy(_item => new { _item._userAndRole._p24User.UserName, _item._userAndRole })
            //    .Select(_item => new DataModel.P24UserViewModel()
            //    {
            //        Username = _item.Key._userAndRole._p24User.UserName,
            //        FullName = _item.Key._userAndRole._p24User.GetDisplayName(),
            //        AttendanceProfileId = _item.Key._userAndRole._p24User.AttendanceProfileId,
            //        Role = string.Join(',', _item.Select(_iitem => _iitem._role.Name).ToArray())
            //    })
            //    .ToList();


            //Data = new DataModel()
            //{
            //    EmployeeList = allUser,
            //};


        }




        private struct Role
        {
            public string Name;
            public int Level;
            public Role(string _name, int _level)
            {
                Name = _name;
                Level = _level;
            }
        }

        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly ILogger<IndexModel> m_Logger;

        private Dictionary<string, Role> m_UsersAndRoles;


        //[BindProperty]
        //public InputModel Input { get; set; }

        //public string ReturnUrl { get; set; }

        //public async Task OnGetAsync(string _returnUrl = null)
        //{
        //    ReturnUrl = _returnUrl;

        //}

        //public async Task<IActionResult> OnPostAsync(string _returnUrl = null)
        //{
        //    if (string.IsNullOrEmpty(_returnUrl))
        //        ReturnUrl = Url.Content("~/");

        //    if (ModelState.IsValid)
        //    {
        //        var user = new P24IdentityUser(Input.Username);
        //        var result = await m_UserManager.CreateAsync(user, Constants.DEFAULT_PASSWORD);
        //        if (result.Succeeded)
        //        {
        //            m_Logger.LogInformation("New account created with default password.");

        //            return LocalRedirect(ReturnUrl);
        //        }

        //        foreach (var error in result.Errors)
        //        {
        //            ModelState.AddModelError(string.Empty, error.Description);
        //        }

        //    }

        //    // If we got this far, something failed, redisplay form
        //    return Page();
        //}





    }

}

/*  Home/ServerAnnouncement.cshtml.cs
 *  Version: v1.1 (2023.12.26)
 *  Spec:    v0.1
 *  
 *  Contributor
 *      Arime-chan (Author)
 */

using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.App;
using Project24.App.Services;
using Project24.Data;
using Project24.Model.Identity;

namespace Project24.Pages
{
    public class ServerAnnouncementModel : PageModel
    {
        public ServerAnnouncementModel(UserManager<P24IdentityUser> _userManager, ServerAnnouncementSvc _announcementSvc)
        //public ServerAnnouncementModel(ServerAnnouncementSvc _announcementSvc, ILogger<ServerAnnouncementModel> _logger)
        {
            m_UserManager = _userManager;
            m_AnnouncementSvc = _announcementSvc;
            //m_Logger = _logger;
        }


        public IActionResult OnGet()
        {
            return NotFound();
        }

        #region AJAX Handler
        // ==================================================
        // AJAX Handler

        public async Task<IActionResult> OnGetFetchAnnouncementDataAsync()
        {
            string jsonData = m_AnnouncementSvc.GetAllJsonSerializedAnnouncements();

            int flag = 0;
            P24IdentityUser user = await m_UserManager.GetUserAsync(User);
            if (user != null)
            {
                // TODO: explore different approach;
                // this call cause "Invalid password for user" out of context warning logging every page refresh;
                if (await m_UserManager.CheckPasswordAsync(user, Constants.DefaultPassword))
                    flag = 1;
            }

            #region Test TimeSpan utils (front-end)
#if false
            TimeSpanTest[] spans = new TimeSpanTest[]
            {
                new (new TimeSpan(1)),
                new (new TimeSpan(123)),
                new (new TimeSpan(1230000)),
                new (new TimeSpan(1234567)),
                new (new TimeSpan(123456789)),

                new (new TimeSpan(1, 0, 0)),
                new (new TimeSpan(1, 1, 1, 1)),
                new (new TimeSpan(0, 1, 1, 1, 123)),
                new (new TimeSpan(1, 1, 1, 1, 123)),
                new (new TimeSpan(1, 25, 1, 1, 123)),
                new (new TimeSpan(99999, 1, 1, 1, 123)),
                new (new TimeSpan(99999, 999, 999, 999, 9999999)),
                new (new TimeSpan(9999999, 9999999, 9999999, 9999999, 9999999)),
            };
            jsonData = JsonSerializer.Serialize(spans);
#endif
            #endregion

            return Content(MessageTag.Success + flag + jsonData, MediaTypeNames.Text.Plain);
        }

        // End: AJAX Handler
        // ==================================================
        #endregion

        #region Test TimeSpan util class (front-end)
#if false
        public class TimeSpanTest
        {
            public TimeSpan TimeSpan { get; set; }
            public double TotalMillis { get; set; }

            public TimeSpanTest(TimeSpan _ts)
            {
                TimeSpan = _ts;
                TotalMillis = _ts.TotalMilliseconds;
            }
        }
#endif
        #endregion


        private readonly SignInManager<P24IdentityUser> m_SignInManager;
        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly ApplicationDbContext m_DbContext;
        private readonly ServerAnnouncementSvc m_AnnouncementSvc;
        //private readonly ILogger<ServerAnnouncementModel> m_Logger;
    }

}

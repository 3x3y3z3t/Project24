/*  Home/ServerAnnouncement.cshtml.cs
 *  Version: v1.0 (2023.09.15)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.App;
using Project24.App.Services;

namespace Project24.Pages
{
    public class ServerAnnouncementModel : PageModel
    {
        public ServerAnnouncementModel(ServerAnnouncementSvc _announcementSvc)
        //public ServerAnnouncementModel(ServerAnnouncementSvc _announcementSvc, ILogger<ServerAnnouncementModel> _logger)
        {
            m_AnnouncementSvc = _announcementSvc;
            //m_Logger = _logger;
        }


        public IActionResult OnGet()
        {
            return NotFound();
        }

        // ajax call only;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<IActionResult> OnGetFetchAnnouncementDataAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            string jsonData = m_AnnouncementSvc.GetAllJsonSerializedAnnouncements();

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

            return Content(MessageTag.Success + jsonData, MediaTypeNames.Text.Plain);
        }

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


        private readonly ServerAnnouncementSvc m_AnnouncementSvc;
        //private readonly ILogger<ServerAnnouncementModel> m_Logger;
    }

}

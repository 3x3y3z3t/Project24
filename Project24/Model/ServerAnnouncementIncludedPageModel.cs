/*  Model/ServerAnnouncementIncludedPageModel.cs
 *  Version: v1.0 (2023.06.29)
 *  
 *  Contributor
 *      Arime-chan
 */

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Project24.Model
{
    public class ServerAnnouncementIncludedPageModel : PageModel
    {
        public string ServerMessage { get; set; }

        public ServerAnnouncementIncludedPageModel()
        {
            ServerMessage = "ServerMessage Here";
        }
    }

}

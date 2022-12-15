/*  Upload.cshtml.cs
 *  Version: 1.1 (2022.12.16)
 *
 *  Contributor
 *      Arime-chan
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project24.App;
using Project24.Models.Nas;

namespace Project24.Pages.Nas
{
    [Authorize(Roles = P24RoleName.NasUser)]
    public class UploadModel : PageModel
    {
        public NasBrowserViewModel Data { get; private set; }


        public UploadModel()
        { }


        public IActionResult OnGet()
        {
            string currentLocation = "";
            if (TempData.ContainsKey("CurrentLocation"))
            {
                currentLocation = (string)TempData["CurrentLocation"];
            }

            NasBrowserUtils.RequestResult result = NasBrowserUtils.HandleBrowseRequest(currentLocation, true);
            Data = result.Data;

            return Page();
        }

        public IActionResult OnGetBrowse(string _path)
        {
            if (_path == null)
                _path = "";

            NasBrowserUtils.RequestResult result = NasBrowserUtils.HandleBrowseRequest(_path, true);
            return Partial("_NasBrowser", result.Data);
        }

        public void OnPost() => BadRequest();
    }

}

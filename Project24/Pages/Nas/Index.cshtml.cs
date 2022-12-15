/*  Index.cshtml.cs
 *  Version: 1.6 (2022.12.16)
 *
 *  Contributor
 *      Arime-chan
 */

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Project24.App;
using Project24.Models.Nas;

namespace Project24.Pages.Nas
{
    [Authorize(Roles = P24RoleName.NasUser)]
    public class IndexModel : PageModel
    {
        public NasBrowserViewModel Data { get; private set; }


        public IndexModel(ILogger<IndexModel> _logger)
        {
            m_Logger = _logger;
        }


        public async Task<IActionResult> OnGetAsync(string _path)
        {
            if (_path == null)
                _path = "";
            else
            {
                _path = _path.Trim('/');
            }

            NasBrowserUtils.RequestResult result = NasBrowserUtils.HandleBrowseRequest(_path);
            if (result.IsFileRequested)
            {
                await ProcessDownloadRequest(result.RequestedFilePath);
            }

            Data = result.Data;

            TempData["CurrentLocation"] = result.Data.Path;
            return Page();
        }

        public void OnPost() => BadRequest();

        private async Task ProcessDownloadRequest(string _absPath)
        {
            FileInfo fi = new FileInfo(_absPath);

            string encoded = System.Web.HttpUtility.UrlPathEncode(fi.Name);

            Response.Clear();

            Response.Headers.Add("Content-Disposition", string.Format("attachment; filename={0}", encoded));
            Response.Headers.Add("Content-Length", fi.Length.ToString());
            Response.ContentType = System.Net.Mime.MediaTypeNames.Application.Octet;

            var physFileInfo = new PhysicalFileProvider(fi.DirectoryName).GetFileInfo(fi.Name);

            await Response.SendFileAsync(physFileInfo);
        }


        private const string s_PrefixUpload = "<upload>";
        private readonly ILogger<IndexModel> m_Logger;
    }

}

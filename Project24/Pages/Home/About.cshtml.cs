/*  About.cshtml.cs
 *  Version: 1.2 (2022.10.16)
 *
 *  Contributor
 *      Arime-chan
 */

using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project24.App;

namespace Project24.Pages
{
    public class AboutModel : PageModel
    {
        public AboutModel(IWebHostEnvironment _webHostEnv)
        {
            m_WebHostEnv = _webHostEnv;

        }

        public async Task OnGetAsync()
        {
            await Utils.UpdateCurrentVersion(m_WebHostEnv);
        }

        public async Task<IActionResult> OnGetReleaseNoteAsync()
        {
            string webRootPath = m_WebHostEnv.WebRootPath;

            string markdown = await System.IO.File.ReadAllTextAsync(webRootPath + "/ReleaseNote.md", Encoding.UTF8);
            string html = MarkdownParser.ToHtml(markdown);

            return new ContentResult()
            {
                Content = html,
                ContentType = MediaTypeNames.Text.Html,
                StatusCode = StatusCodes.Status200OK
            };
        }

        private readonly IWebHostEnvironment m_WebHostEnv;
    }

}

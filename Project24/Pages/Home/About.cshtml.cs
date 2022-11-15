/*  About.cshtml.cs
 *  Version: 1.4 (2022.11.13)
 *
 *  Contributor
 *      Arime-chan
 */

using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
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


        public void OnGet()
        { }

        public async Task<IActionResult> OnGetReleaseNoteAsync()
        {
            string webRootPath = m_WebHostEnv.WebRootPath;

            string markdown = await System.IO.File.ReadAllTextAsync(webRootPath + "/ReleaseNote.md", Encoding.UTF8);
            HtmlString htmlString = new HtmlString(MarkdownParser.ToHtml(markdown));

            return Partial("_ReleaseNote", htmlString);
        }


        private readonly IWebHostEnvironment m_WebHostEnv;
    }

}

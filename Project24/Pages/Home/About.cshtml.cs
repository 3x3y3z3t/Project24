/*  About.cshtml.cs
 *  Version: 1.1 (2022.10.16)
 *
 *  Contributor
 *      Arime-chan
 */

using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project24.App;

namespace Project24.Pages
{
    public class AboutModel : PageModel
    {

        public string CurrentVersion { get; set; }
        public HtmlString ChangelogContentAsHtmlString { get; set; }

        public AboutModel(IWebHostEnvironment _webHostEnv)
        {
            m_WebHostEnv = _webHostEnv;

        }

        public async void OnGetAsync()
        {
            string webRootPath = m_WebHostEnv.WebRootPath;

            string markdown = await System.IO.File.ReadAllTextAsync(webRootPath + "/ReleaseNote.md", Encoding.UTF8);
            string html = MarkdownParser.ToHtml(markdown);

            ChangelogContentAsHtmlString = new HtmlString(html);

            Regex regex = new Regex(@"\A(#+ v[0-9]+\.[0-9]+\.[0-9]+)");

            Match match = regex.Match(markdown);
            if (match.Success)
            {
                CurrentVersion = match.Value[4..]; // equivalent to .Substring(4);
            }
            else
            {
                CurrentVersion = "Unknown";
            }
        }

        private readonly IWebHostEnvironment m_WebHostEnv;
    }

}

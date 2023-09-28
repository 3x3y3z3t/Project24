/*  Changelog.cshtml.cs
 *  Version: v1.1 (2023.05.16)
 *  
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.App;

namespace Project24.Pages.Home
{
    public class ChangelogModel : PageModel
    {
        public ChangelogModel(IWebHostEnvironment _webHostEnv, ILogger<ChangelogModel> _logger)
        {
            m_WebHostEnv = _webHostEnv;
            m_Logger = _logger;
        }


        // ajax handler;
        public async Task<IActionResult> OnGetChangelogAsync()
        {
            string webRootPath = m_WebHostEnv.WebRootPath;

            try
            {
                string markdown = await System.IO.File.ReadAllTextAsync(webRootPath + "/Changelog.md", Encoding.UTF8);
                return Content(MessageTag.Success + markdown);
            }
            catch (Exception _e)
            {
                m_Logger.LogWarning("" + _e);
                return Content(MessageTag.Exception + _e);
            }
        }

        // ajax handler;
        /// <summary> Returns all Update notes. </summary>
        public async Task<IActionResult> OnGetFullUpdateNotesAsync()
        {
            return Content("<done>OnGetFullUpdateNotes()");

        }

        // ajax handler;
        /// <summary> Returns the Update Note with the specified tag, or latest Update Notes starting from the specified tag. </summary>
        /// <param name="_tag">The tag of the Update Note to retrieve</param>
        /// <param name="_latest">If false, retrieve the Note with the tag. If true, retrieve all lattest Notes starting from the tag</param>
        public async Task<IActionResult> OnGetUpdateNoteByTagAsync(string _tag, bool _latest = false)
        {
            if (string.IsNullOrEmpty(_tag))
                return BadRequest();

            // TODO: fetch from db;

            string msg = string.Format(Project24.App.Localization.Message.UpdateNotes_Failed_Tagged, _tag);
            return Content("<" + _tag + "><div>" + msg + "</div><div>OnGetUpdatenoteByTag(" + _tag + ", " + _latest + ")</div>");

        }


        private readonly IWebHostEnvironment m_WebHostEnv;
        private readonly ILogger<ChangelogModel> m_Logger;
    }

}

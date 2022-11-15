/*  Delete.cshtml.cs
 *  Version: 1.0 (2022.10.25)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project24.App;
using Project24.Data;
using Project24.Identity;
using Project24.Models;

namespace Project24.Pages.Nas
{
    [Authorize(Roles = P24RoleName.NasTester)]
    public class DeleteModel : PageModel
    {
        public class DataModel
        {
            public string File { get; set; }
        }


        public DeleteModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
        }


        public void OnGet() => BadRequest();

        public async Task<IActionResult> OnPostAsync([FromBody] DataModel _data)
        {
            #region Common Validation
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Content(ErrorMessage.CurrentUserIsNull, MediaTypeNames.Text.Plain);
            }

            if (!ModelState.IsValid)
            {
                await m_DbContext.RecordChanges(
                    currentUser.UserName,
                    ActionRecord.Operation_.CreateNasFolder,
                    ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.Error, ErrorMessage.InvalidModelState }
                    }
                );

                return Content(ErrorMessage.InvalidModelState, MediaTypeNames.Text.Plain);
            }
            #endregion

            string absPath = Path.GetFullPath(DriveUtils.NasRootPath + "/" + _data.File);

            FileAttributes attrib = System.IO.File.GetAttributes(absPath);
            if (attrib.HasFlag(FileAttributes.Directory))
            {
                Directory.Delete(absPath, true);
            }
            else
            {
                System.IO.File.Delete(absPath);
            }

            int pos = _data.File.LastIndexOf("/");
            string parentDir = _data.File.Remove(pos);

            IndexModel.DataModel data = new IndexModel.DataModel();
            data.IsUploadMode = true;

            List<NasUtils.FileModel> files = NasUtils.GetDirectoryContent(parentDir);
            if (files == null)
            {
                return Partial("_NasBrowser", data);
            }

            List<string> pathLayers = new List<string>(parentDir.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries));

            data.Path = parentDir.TrimStart('/');
            data.PathLayers = pathLayers;
            data.Files = files;

            return Partial("_NasBrowser", data);
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
    }

}

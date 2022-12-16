/*  Manage.cshtml.cs
 *  Version: 1.1 (2022.12.16)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.App;
using Project24.Data;
using Project24.Models;
using Project24.Models.Identity;

namespace Project24.Pages.Nas
{
    public class ManageModel : PageModel
    {
        public class OperationData
        {
            [Required(AllowEmptyStrings = true)]
            public string Item1 { get; set; }
            [Required(AllowEmptyStrings = true)]
            public string Item2 { get; set; }
        }


        public ManageModel(ApplicationDbContext _dbContext, UserManager<P24IdentityUser> _usermanager, ILogger<ManageModel> _logger)
        {
            m_DbContext = _dbContext;
            m_UserManager = _usermanager;
            m_Logger = _logger;
        }


        public void OnGet() => BadRequest();
        public void OnPost() => BadRequest();

        public async Task<IActionResult> OnPostCreateFolderAsync([FromBody] OperationData _data)
        {
            if (!await ValidateModelState(ActionRecord.Operation_.Nas_CreateFolder))
            {
                string html = string.Format(c_ErrorDivElementFormatString, ErrorMessage.InvalidModelState);
                return Content(html, MediaTypeNames.Text.Html);
            }

            try
            {
                string absPath = Path.GetFullPath(DriveUtils.NasRootPath + "/" + _data.Item1 + "/" + _data.Item2);

                Directory.CreateDirectory(absPath);
            }
            catch (Exception _e)
            {
                m_Logger.LogError("Create Folder error: " + _e.ToString());
                string html = string.Format(c_ErrorCodeElementFormatString, _e);
                return Content(html, MediaTypeNames.Text.Html);
            }

            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.Nas_CreateFolder,
                ActionRecord.OperationStatus_.Success,
                new Dictionary<string, string>()
                {
                    { CustomInfoKey.Path, _data.Item1 },
                    { CustomInfoKey.Filename, _data.Item2 }
                }
            );

            NasBrowserUtils.RequestResult result = NasBrowserUtils.HandleBrowseRequest(_data.Item1, true);
            return Partial("_NasBrowser", result.Data);
        }

        public async Task<IActionResult> OnPostRenameAsync([FromBody] OperationData _data)
        {
            if (!await ValidateModelState(ActionRecord.Operation_.Nas_RenameFile))
            {
                string html = string.Format(c_ErrorDivElementFormatString, ErrorMessage.InvalidModelState);
                return Content(html, MediaTypeNames.Text.Html);
            }

            string parentDir = "";
            int pos = _data.Item1.LastIndexOf("/");
            if (pos > 0)
                parentDir = _data.Item1[0..pos];

            try
            {
            string srcDir = Path.GetFullPath(DriveUtils.NasRootPath + "/" + _data.Item1);
            string dstDir = Path.GetFullPath(DriveUtils.NasRootPath + "/" + parentDir + "/" + _data.Item2);

                FileAttributes attrib = System.IO.File.GetAttributes(srcDir);
                if (attrib.HasFlag(FileAttributes.Directory))
                {
                    Directory.Move(srcDir, dstDir);
                }
                else
                {
                    System.IO.File.Move(srcDir, dstDir);
                }
            }
            catch (Exception _e)
            {
                m_Logger.LogError("Rename error: " + _e.ToString());
                string html = string.Format(c_ErrorCodeElementFormatString, _e);
                return Content(html, MediaTypeNames.Text.Html);
            }

            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.Nas_RenameFile,
                ActionRecord.OperationStatus_.Success,
                new Dictionary<string, string>()
                {
                    { CustomInfoKey.Path, _data.Item1 },
                    { CustomInfoKey.Filename, _data.Item2 }
                }
            );

            NasBrowserUtils.RequestResult result = NasBrowserUtils.HandleBrowseRequest(parentDir, true);
            return Partial("_NasBrowser", result.Data);
        }

        public async Task<IActionResult> OnPostCopyToAsync([FromBody] OperationData _data) => BadRequest();
        public async Task<IActionResult> OnPostMoveToAsync([FromBody] OperationData _data) => BadRequest();

        public async Task<IActionResult> OnPostDeleteAsync([FromBody] OperationData _data)
        {
            if (!await ValidateModelState(ActionRecord.Operation_.DeleteNasFile))
            {
                string html = string.Format(c_ErrorDivElementFormatString, ErrorMessage.InvalidModelState);
                return Content(html, MediaTypeNames.Text.Html);
            }

            try
            {
            string absPath = Path.GetFullPath(DriveUtils.NasRootPath + "/" + _data.Item1);

                FileAttributes attrib = System.IO.File.GetAttributes(absPath);
                if (attrib.HasFlag(FileAttributes.Directory))
                {
                    Directory.Delete(absPath, true);
                }
                else
                {
                    System.IO.File.Delete(absPath);
                }
            }
            catch (Exception _e)
            {
                m_Logger.LogError("Delete error: " + _e.ToString());
                string html = string.Format(c_ErrorCodeElementFormatString, _e);
                return Content(html, MediaTypeNames.Text.Html);
            }

            string path = "";
            int pos = _data.Item1.LastIndexOf("/");
            if (pos > 0)
                path = _data.Item1[0..pos];

            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.Nas_DeleteFile,
                ActionRecord.OperationStatus_.Success,
                new Dictionary<string, string>()
                {
                    { CustomInfoKey.Path, _data.Item1 }
                }
            );

            NasBrowserUtils.RequestResult result = NasBrowserUtils.HandleBrowseRequest(path, true);
            return Partial("_NasBrowser", result.Data);
        }

        private async Task<bool> ValidateModelState(string _operation)
        {
            if (ModelState.IsValid)
                return true;

            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            await m_DbContext.RecordChanges(
                currentUser.UserName,
                _operation,
                ActionRecord.OperationStatus_.Failed,
                new Dictionary<string, string>()
                {
                    { CustomInfoKey.Error, ErrorMessage.InvalidModelState }
                }
            );

            return false;
        }


        private const string c_BackToRootElementString = "<a href=\"/Nas/Upload\">Back to <i>root/</i></a>";
        private const string c_ErrorDivElementFormatString = "<div class=\"text-danger\">{0}</div>" + c_BackToRootElementString;
        private const string c_ErrorCodeElementFormatString = "<pre><code class=\"text-danger\">{0}</code></pre>" + c_BackToRootElementString;

        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly ApplicationDbContext m_DbContext;
        private readonly ILogger<ManageModel> m_Logger;
    }

}

/*  P24b/Manage.cshtml.cs
 *  Version: 1.2 (2023.01.30)
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
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Project24.App;
using Project24.App.Extension;
using Project24.Data;
using Project24.Models;
using Project24.Models.Identity;
using Project24.Models.Nas;

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
                return Content(CustomInfoTag.Error + ErrorMessage.InvalidModelState, MediaTypeNames.Text.Plain);

            if (!AppUtils.IsFileNameValid(_data.Item2, true))
            {
                return Content(CustomInfoTag.Warning + c_HtmlInvalidFileNameString);
            }

            try
            {
                string absPath = Path.GetFullPath(DriveUtils.NasRootPath + "/" + _data.Item1 + "/" + _data.Item2);

                if (Directory.Exists(absPath))
                {
                    m_Logger.LogWarning("Folder " + _data.Item2 + " will not be created:\r\nDirectory " + absPath + " is already existed.");
                    return Content(CustomInfoTag.Warning + "Folder \"" + _data.Item2 + "\" is already existed.", MediaTypeNames.Text.Plain);
                }

                Directory.CreateDirectory(absPath);
            }
            catch (Exception _e)
            {
                m_Logger.LogError("" + _e);
                return Content(CustomInfoTag.Error + CustomInfoTag.Exception + _e, MediaTypeNames.Text.Plain);
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

            ViewData["NewFolder"] = _data.Item2;

            NasBrowserUtils.RequestResult result = NasBrowserUtils.HandleBrowseRequest(_data.Item1, true);
            //return Partial("_NasBrowser", result.Data);
            return new PartialViewResult()
            {
                ViewName = "_NasBrowser",
                ViewData = new ViewDataDictionary<NasBrowserViewModel>(ViewData, result.Data)
            };
        }

        public async Task<IActionResult> OnPostRenameAsync([FromBody] OperationData _data)
        {
            if (!await ValidateModelState(ActionRecord.Operation_.Nas_CreateFolder))
                return Content(CustomInfoTag.Error + ErrorMessage.InvalidModelState, MediaTypeNames.Text.Plain);

            string srcDir = Path.GetFullPath(DriveUtils.NasRootPath + "/" + _data.Item1);
            FileAttributes srcDirAttrib = System.IO.File.GetAttributes(srcDir);
            bool isFolder = srcDirAttrib.HasFlag(FileAttributes.Directory);

            if (!AppUtils.IsFileNameValid(_data.Item2, isFolder))
            {
                return Content(CustomInfoTag.Warning + c_HtmlInvalidFileNameString);
            }

            string parentDir = "";
            int pos = _data.Item1.LastIndexOf("/");
            if (pos > 0)
                parentDir = _data.Item1[0..pos];

            try
            {
                string dstDir = Path.GetFullPath(DriveUtils.NasRootPath + "/" + parentDir + "/" + _data.Item2);

                if (Directory.Exists(dstDir))
                {
                    FileAttributes attrib = System.IO.File.GetAttributes(dstDir);
                    string dirType = attrib.HasFlag(FileAttributes.Directory) ? "Folder " : "File ";
                    m_Logger.LogWarning(dirType + _data.Item1 + " will not be renamed to " + _data.Item2 + ":\r\nDirectory " + dstDir + " is already existed.");
                    return Content(CustomInfoTag.Warning + dirType + "\"" + _data.Item2 + "\" is already existed (no rename happened).", MediaTypeNames.Text.Plain);
                }

                if (isFolder)
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
                m_Logger.LogError("" + _e);
                return Content(CustomInfoTag.Error + CustomInfoTag.Exception + _e, MediaTypeNames.Text.Plain);
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

            ViewData["RenamedItem"] = _data.Item2;

            NasBrowserUtils.RequestResult result = NasBrowserUtils.HandleBrowseRequest(parentDir, true);
            //return Partial("_NasBrowser", result.Data);
            return new PartialViewResult()
            {
                ViewName = "_NasBrowser",
                ViewData = new ViewDataDictionary<NasBrowserViewModel>(ViewData, result.Data)
            };
        }

        public async Task<IActionResult> OnPostCopyToAsync([FromBody] OperationData _data)
        {
            if (!await ValidateModelState(ActionRecord.Operation_.Nas_CopyTo))
                return Content(CustomInfoTag.Error + ErrorMessage.InvalidModelState, MediaTypeNames.Text.Plain);

            string path = "";
            string name = _data.Item1;
            int pos = _data.Item1.LastIndexOf("/");
            if (pos > 0)
            {
                path = _data.Item1[0..pos];
                name = _data.Item1[(pos + 1)..];
            }

            NasUtils.NasOperationStatus status;
            string newName = null;
            try
            {
                if (_data.Item2.Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    status = NasUtils.MakeCopy(path, name, out newName);
                }
                else
                {
                    string dstPath = Path.GetFullPath(DriveUtils.NasRootPath + "/" + _data.Item2);
                    if (!Directory.Exists(dstPath))
                    {
                        status = NasUtils.NasOperationStatus.OperationCopyTo | NasUtils.NasOperationStatus.DestinationNotExisted;
                    }
                    else
                    {
                        string dstDir = dstPath + "/" + name;
                        string srcDir = Path.GetFullPath(DriveUtils.NasRootPath + "/" + _data.Item1);
                        status = NasUtils.CopyTo(srcDir, dstDir);
                    }
                }
            }
            catch (Exception _e)
            {
                m_Logger.LogError("" + _e);
                return Content(CustomInfoTag.Error + CustomInfoTag.Exception + _e, MediaTypeNames.Text.Plain);
            }

            string operationStatus = ActionRecord.OperationStatus_.Failed;
            Dictionary<string, string> customInfo = new Dictionary<string, string>()
            {
                { "src", _data.Item1 }
            };

            if (status.HasFlag(NasUtils.NasOperationStatus.OperationCopyTo))
            {
                customInfo.Add("dst", _data.Item2);
            }

            if (status.HasFlag(NasUtils.NasOperationStatus.OK))
            {
                operationStatus = ActionRecord.OperationStatus_.Success;
            }
            else if (status.HasFlag(NasUtils.NasOperationStatus.MaximumDuplicationReached))
            {
                customInfo.Add("status", "Maximum Duplication Reached");
            }
            else if (status.HasFlag(NasUtils.NasOperationStatus.FileExisted))
            {
                customInfo.Add("status", "File Existed");
            }
            else if (status.HasFlag(NasUtils.NasOperationStatus.DestinationNotExisted))
            {
                customInfo.Add("status", "Destination Not Existed");
            }

            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.Nas_CopyTo,
                operationStatus,
                customInfo
            );

            string html = "";
            if (status.HasFlag(NasUtils.NasOperationStatus.OperationMakeCopy))
            {
                if (status.HasFlag(NasUtils.NasOperationStatus.OK))
                {
                    ViewData["DuplicatedItem"] = newName;

                    NasBrowserUtils.RequestResult result = NasBrowserUtils.HandleBrowseRequest(path, true);
                    return new PartialViewResult()
                    {
                        ViewName = "_NasBrowser",
                        ViewData = new ViewDataDictionary<NasBrowserViewModel>(ViewData, result.Data)
                    };
                }

                if (status.HasFlag(NasUtils.NasOperationStatus.MaximumDuplicationReached))
                {
                    html = "Project24 NAS only allowed " + Constants.MaxFileDuplication + " item duplications in the same directory.";
                    return Content(CustomInfoTag.Warning + html, MediaTypeNames.Text.Plain);
                }

                html = GetUnknownErrorAsHtml("Unknown error", status);
                return Content(CustomInfoTag.Error + html, MediaTypeNames.Text.Plain);
            }

            if (status.HasFlag(NasUtils.NasOperationStatus.OperationCopyTo))
            {
                if (status.HasFlag(NasUtils.NasOperationStatus.OK))
                    return Content(CustomInfoTag.Success, MediaTypeNames.Text.Plain);

                if (status.HasFlag(NasUtils.NasOperationStatus.FileExisted))
                {
                    html = "<div>An item with the same name is already existed in destiation directory.<div>" +
                        "<div class=\"ml-3\">Name: <code>" + name + "</code></div>" +
                        "<div class=\"ml-3\">Destination: <code>" + _data.Item2 + "</code></div>";
                    return Content(CustomInfoTag.Warning + html, MediaTypeNames.Text.Plain);
                }

                if (status.HasFlag(NasUtils.NasOperationStatus.DestinationNotExisted))
                {
                    html = "<div>Destination directory does not existed.</div>" +
                        "<div class=\"ml-3\">Destination: <code>" + _data.Item2 + "</code></div>";
                    return Content(CustomInfoTag.Error + html, MediaTypeNames.Text.Plain);
                }

                html = GetUnknownErrorAsHtml("Unknown error", status);
                return Content(CustomInfoTag.Error + html, MediaTypeNames.Text.Plain);
            }

            html = GetUnknownErrorAsHtml("Invalid operation", status);
            return Content(CustomInfoTag.Error + html, MediaTypeNames.Text.Plain);
        }

        public async Task<IActionResult> OnPostMoveToAsync([FromBody] OperationData _data)
        {
            if (!await ValidateModelState(ActionRecord.Operation_.Nas_MoveTo))
                return Content(CustomInfoTag.Error + ErrorMessage.InvalidModelState, MediaTypeNames.Text.Plain);

            string path = "";
            string name = _data.Item1;
            int pos = _data.Item1.LastIndexOf("/");
            if (pos > 0)
            {
                path = _data.Item1[0..pos];
                name = _data.Item1[(pos + 1)..];
            }

            NasUtils.NasOperationStatus status;
            string html = "";
            try
            {
                if (_data.Item2.Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    return Content(CustomInfoTag.Warning + "You can't \"move\" item into the same directory.", MediaTypeNames.Text.Plain);
                }

                string dstPath = Path.GetFullPath(DriveUtils.NasRootPath + "/" + _data.Item2);
                if (!Directory.Exists(dstPath))
                {
                    html = "<div>Destination directory does not existed.</div>" +
                        "<div class=\"ml-3\">Destination: <code>" + _data.Item2 + "</code></div>";
                    return Content(CustomInfoTag.Error + html, MediaTypeNames.Text.Plain);
                }

                string srcDir = Path.GetFullPath(DriveUtils.NasRootPath + "/" + _data.Item1);
                string dstDir = dstPath + "/" + name;

                status = NasUtils.MoveTo(srcDir, dstDir);
            }
            catch (Exception _e)
            {
                m_Logger.LogError("" + _e);
                return Content(CustomInfoTag.Error + CustomInfoTag.Exception + _e, MediaTypeNames.Text.Plain);
            }

            string operationStatus = ActionRecord.OperationStatus_.Failed;
            Dictionary<string, string> customInfo = new Dictionary<string, string>()
            {
                { "src", _data.Item1 }
            };

            if (status.HasFlag(NasUtils.NasOperationStatus.OperationCopyTo))
            {
                customInfo.Add("dst", _data.Item2);
            }

            if (status.HasFlag(NasUtils.NasOperationStatus.OK))
            {
                operationStatus = ActionRecord.OperationStatus_.Success;
            }
            else if (status.HasFlag(NasUtils.NasOperationStatus.FileExisted))
            {
                customInfo.Add("status", "File Existed");
            }

            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.Nas_MoveTo,
                operationStatus,
                customInfo
            );

            if (status.HasFlag(NasUtils.NasOperationStatus.OK))
            {
                NasBrowserUtils.RequestResult result = NasBrowserUtils.HandleBrowseRequest(path, true);
                return Partial("_NasBrowser", result.Data);
            }

            if (status.HasFlag(NasUtils.NasOperationStatus.FileExisted))
            {
                html = "<div>An item with the same name is already existed in destiation directory.<div>" +
                    "<div class=\"ml-3\">Name: <code>" + name + "</code></div>" +
                    "<div class=\"ml-3\">Destination: <code>" + _data.Item2 + "</code></div>";
                return Content(CustomInfoTag.Warning + html, MediaTypeNames.Text.Plain);
            }

            html = GetUnknownErrorAsHtml("Invalid operation", status);
            return Content(CustomInfoTag.Error + html, MediaTypeNames.Text.Plain);
        }

        public async Task<IActionResult> OnPostDeleteAsync([FromBody] OperationData _data)
        {
            if (!await ValidateModelState(ActionRecord.Operation_.Nas_CreateFolder))
                return Content(CustomInfoTag.Error + ErrorMessage.InvalidModelState, MediaTypeNames.Text.Plain);

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
                m_Logger.LogError("" + _e);
                return Content(CustomInfoTag.Error + CustomInfoTag.Exception + _e, MediaTypeNames.Text.Plain);
            }

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

            string parentDir = "";
            int pos = _data.Item1.LastIndexOf("/");
            if (pos > 0)
                parentDir = _data.Item1[0..pos];

            NasBrowserUtils.RequestResult result = NasBrowserUtils.HandleBrowseRequest(parentDir, true);
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

        private string GetUnknownErrorAsHtml(string _title, NasUtils.NasOperationStatus _status)
        {
            string html = "<div>" + _title + ":<div>";
            foreach (Enum flag in Enum.GetValues(typeof(NasUtils.NasOperationStatus)))
            {
                if (_status.HasFlag(flag))
                    html += "<div class=\"ml-3\"><code>" + flag.ToString() + "</code></div>";
            }

            return html;
        }


        private const string c_HtmlInvalidFileNameString = "Invalid name (<a href=\"#\" data-toggle=\"modal\" data-target=\"#modal-naming-rules\">more details</a>).";

        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly ApplicationDbContext m_DbContext;
        private readonly ILogger<ManageModel> m_Logger;
    }

}

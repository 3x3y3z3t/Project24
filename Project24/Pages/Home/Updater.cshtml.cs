/*  Updater.cshtml.cs
 *  Version: 1.2 (2022.12.06)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.App;
using Project24.App.Services;
using Project24.App.Utils;
using Project24.Data;
using Project24.Models;
using Project24.Models.Identity;

namespace Project24.Pages.Home
{
    [Authorize(Roles = P24RoleName.Power)]
    public class UpdaterModel : PageModel
    {
        public List<NasUtils.FileModel> LocalFiles { get; set; }

        public string StatusMessage { get; set; } = "";


        public UpdaterModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager, UpdaterService _updaterSvc, ILogger<UpdaterModel> _logger)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
            m_UpdaterService = _updaterSvc;
            m_Logger = _logger;

            LocalFiles = new List<NasUtils.FileModel>();
        }


        public void OnGet()
        {
            LocalFiles = NasUtils.GetAllFilesInDirectory("", NasUtils.NasLocation.AppNextRoot);
        }

        public async Task<IActionResult> OnPostInitUploadAsync()
        {
            if (!await ValidateModelState(ActionRecord.Operation_.Updater_UploadNextFiles))
                return BadRequest();

            string json = Request.Headers["TotalFiles"];
            int.TryParse(json, out int totalFiles);
            AppUtils.UpdaterStats.TotalFilesToUpload = totalFiles;
            AppUtils.UpdaterStats.TotalUploadedFiles = 0;

            return StatusCode(StatusCodes.Status200OK);
        }

        public async Task<IActionResult> OnPostFinalizeUploadAsync()
        {
            if (!await ValidateModelState(ActionRecord.Operation_.Updater_UploadNextFiles))
                return BadRequest();

            AppUtils.UpdaterStats.TotalFilesToUpload = 0;
            AppUtils.UpdaterStats.TotalUploadedFiles = 0;

            return StatusCode(StatusCodes.Status200OK);
        }

        public async Task<IActionResult> OnPostUploadAsync(IList<IFormFile> _files)
        {
            if (!await ValidateModelState(ActionRecord.Operation_.Updater_UploadNextFiles))
                return Content(ErrorMessage.InvalidModelState, MediaTypeNames.Text.Plain);

            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);

            string absPath = DriveUtils.AppNextRootPath;
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            string json = Request.Headers["LastModifiedDates"];
            Dictionary<string, long> lastModDates = JsonSerializer.Deserialize<Dictionary<string, long>>(json);

            int successCount = 0;
            int errorCount = 0;

            foreach (var file in _files)
            {
                UpdaterUtils.UploadFileInfo fi = UpdaterUtils.ComputeUploadFileInfo(file.FileName[8..]);

                DateTime dt = epoch;
                if (lastModDates.ContainsKey(fi.HashCode))
                {
                    long millis = lastModDates[fi.HashCode];
                    dt = epoch.AddMilliseconds(millis).ToLocalTime();
                }

                if (fi.Path != "")
                {
                    Directory.CreateDirectory(absPath + "/" + fi.Path);
                }

                string fileFullname = absPath + "/" + fi.Path + fi.Name;

                try
                {
                    Stream stream = System.IO.File.Create(fileFullname, 8 * 1024);
                    await file.CopyToAsync(stream);
                    stream.Close();

                    System.IO.File.SetLastWriteTime(fileFullname, dt);

                    ++successCount;
                }
                catch (Exception _e)
                {
                    ++errorCount;
                    m_Logger.LogError("Error during upload file " + fileFullname + ":\r\n" + _e.ToString());
                }
            }

            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.Updater_UploadNextFiles,
                ActionRecord.OperationStatus_.Success,
                new Dictionary<string, string>()
                {
                    { CustomInfoKey.SuccessCount, "" + successCount },
                    { CustomInfoKey.ErrorCount, "" + errorCount },
                }
            );

            AppUtils.UpdaterStats.TotalUploadedFiles += _files.Count;

            LocalFiles = NasUtils.GetAllFilesInDirectory("", NasUtils.NasLocation.AppNextRoot);
            StatusMessage = AppUtils.UpdaterStats.TotalUploadedFiles + "/" + AppUtils.UpdaterStats.TotalFilesToUpload + " files uploaded succesfully.";
            return Partial("_LocalFilePanel", this);
        }

        public async Task<IActionResult> OnPostVersionUp()
        {
            if (!await ValidateModelState(ActionRecord.Operation_.Updater_PurgeNextFiles))
                return Content(ErrorMessage.InvalidModelState, MediaTypeNames.Text.Plain);

            m_UpdaterService.Start();

            LocalFiles = NasUtils.GetAllFilesInDirectory("", NasUtils.NasLocation.AppNextRoot);
            StatusMessage = "Warning: Version Up initialized. T-minus " + m_UpdaterService.PreparationTime + " minutes";
            return Partial("_LocalFilePanel", this);
        }

        public async Task<IActionResult> OnPostAbortVersionUp()
        {
            if (!await ValidateModelState(ActionRecord.Operation_.Updater_PurgeNextFiles))
                return Content(ErrorMessage.InvalidModelState, MediaTypeNames.Text.Plain);

            m_UpdaterService.Abort();

            LocalFiles = NasUtils.GetAllFilesInDirectory("", NasUtils.NasLocation.AppNextRoot);
            StatusMessage = "Version Up aborted.";
            return Partial("_LocalFilePanel", this);
        }

        public async Task<IActionResult> OnPostPurgeNext()
        {
            if (!await ValidateModelState(ActionRecord.Operation_.Updater_PurgeNextFiles))
                return Content(ErrorMessage.InvalidModelState, MediaTypeNames.Text.Plain);

            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);

            LocalFiles = NasUtils.GetAllFilesInDirectory("", NasUtils.NasLocation.AppNextRoot);

            //if (!System.IO.File.Exists(DriveUtils.AppNextRootPath + "/version.xml"))
            //{
            //    StatusMessage = "Warning: Version file (version.xml) not found!";
            //    return Partial("_LocalFilePanel", this);
            //}

            DirectoryInfo dirInfo = new DirectoryInfo(DriveUtils.AppNextRootPath);
            bool isEmpty = false;

            try
            {
                var files = dirInfo.GetFiles();
                foreach (FileInfo fi in files)
                {
                    System.IO.File.Delete(fi.FullName);
                }

                var dirs = dirInfo.GetDirectories();
                foreach (DirectoryInfo di in dirs)
                {
                    Directory.Delete(di.FullName, true);
                }

                if (files.Length == 0 && dirs.Length == 0)
                {
                    isEmpty = true;
                }
            }
            catch (Exception _e)
            {
                m_Logger.LogError("Error during purging next version's files:\r\n" + _e.ToString());
            }

            if (!isEmpty)
            {
                await m_DbContext.RecordChanges(
                    currentUser.UserName,
                    ActionRecord.Operation_.Updater_PurgeNextFiles,
                    ActionRecord.OperationStatus_.Success
                );
            }

            LocalFiles = NasUtils.GetAllFilesInDirectory("", NasUtils.NasLocation.AppNextRoot);
            StatusMessage = "Next version files purged.";
            return Partial("_LocalFilePanel", this);
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


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly UpdaterService m_UpdaterService;
        private readonly ILogger<UpdaterModel> m_Logger;
    }

}

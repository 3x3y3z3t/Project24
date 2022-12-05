/*  Updater.cshtml.cs
 *  Version: 1.1 (2022.12.04)
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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.App;
using Project24.App.Services;
using Project24.Data;
using Project24.Identity;
using Project24.Models;

namespace Project24.Pages.Home
{
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

        public async Task<IActionResult> OnPostUploadAsync(IList<IFormFile> _files)
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
                    ActionRecord.Operation_.Updater_UploadNextFiles,
                    ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.Error, ErrorMessage.InvalidModelState }
                    }
                );

                return Content(ErrorMessage.InvalidModelState, MediaTypeNames.Text.Plain);
            }
            #endregion

            string absPath = DriveUtils.AppNextRootPath;
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            string json = Request.Headers["LastModifiedDates"];
            Dictionary<string, long> lastModDates = JsonSerializer.Deserialize<Dictionary<string, long>>(json);

            int successCount = 0;
            int errorCount = 0;

            foreach (var file in _files)
            {
                string path = file.FileName[7..];
                int pos = path.LastIndexOf('/');
                if (pos >= 0)
                {
                    path = path[0..pos];
                }


                string filename = file.FileName[(pos + 8)..];
                string fileFullname = absPath + "/" + filename;

                if (path != "")
                { 
                    Directory.CreateDirectory(absPath + "/" + path);
                }

                string hashCode = AppUtils.ComputeCyrb53HashCode(filename);

                DateTime dt = epoch;
                if (lastModDates.ContainsKey(hashCode))
                {
                    long millis = lastModDates[hashCode];
                    dt = epoch.AddMilliseconds(millis).ToLocalTime();
                }

                try
                {
                    Stream stream = System.IO.File.Create(fileFullname, 4 * 1024);
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

            LocalFiles = NasUtils.GetAllFilesInDirectory("", NasUtils.NasLocation.AppNextRoot);
            StatusMessage = _files.Count + " files uploaded succesfully.";
            return Partial("_LocalFilePanel", this);
        }

        public async Task<IActionResult> OnPostVersionUp()
        {
            m_UpdaterService.Start();

            LocalFiles = NasUtils.GetAllFilesInDirectory("", NasUtils.NasLocation.AppNextRoot);
            StatusMessage = "Warning: Version Up initialized. T-minus " + m_UpdaterService.PreparationTime + " minutes";
            return Partial("_LocalFilePanel", this);
        }

        public async Task<IActionResult> OnPostAbortVersionUp()
        {
            m_UpdaterService.Abort();

            LocalFiles = NasUtils.GetAllFilesInDirectory("", NasUtils.NasLocation.AppNextRoot);
            StatusMessage = "Version Up aborted.";
            return Partial("_LocalFilePanel", this);
        }

        public async Task<IActionResult> OnPostPurgeNext()
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
                    ActionRecord.Operation_.Updater_PurgeNextFiles,
                    ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.Error, ErrorMessage.InvalidModelState }
                    }
                );

                return Content(ErrorMessage.InvalidModelState, MediaTypeNames.Text.Plain);
            }
            #endregion

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

            StatusMessage = "Next version files purged.";
            return Partial("_LocalFilePanel", this);
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly UpdaterService m_UpdaterService;
        private readonly ILogger<UpdaterModel> m_Logger;
    }

}

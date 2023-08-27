/*  Home/Maintenance/Updater.cshtml.cs
 *  Version: v1.1 (2023.08.25)
 *  
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.App;
using Project24.App.BackendData;
using Project24.App.Services;
using Project24.Model;
using Project24.Model.Home.Maintenance;
using Project24.SerializerContext;

namespace Project24.Pages.Home.Maintenance
{
    public class UpdaterModel : ServerAnnouncementIncludedPageModel
    {
        public UpdaterModel(UpdaterSvc _updaterSvc, FileSystemSvc _fileSystemSvc, ILogger<UpdaterModel> _logger)
        {
            m_UpdaterSvc = _updaterSvc;
            m_FileSystemSvc = _fileSystemSvc;
            m_Logger = _logger;
        }


        public void OnGet()
        {

            ServerMessage = "" +
                " | Status: " + m_UpdaterSvc.Status.ToString() +
                " | QueuedAction: " + m_UpdaterSvc.QueuedAction +
                " | DueTime: "+ m_UpdaterSvc.QueuedActionDueTime.ToString() +
                " | Remaining: " + m_UpdaterSvc.QueueActionRemainingTime.ToString() +
                " | AppSide: " + Program.AppSide;


        }

        // ajax call only;
        public IActionResult OnGetFetchPageData()
        {
            UpdaterPageDataModel data = new()
            {
                Status = m_UpdaterSvc.Status,
            };

            if (m_UpdaterSvc.LastErrorMessage != null)
                data.Message = m_UpdaterSvc.LastErrorMessage;

            try
            {
                VersionInfo curVerInfo = new(Assembly.GetEntryAssembly().GetName().Version);
                data.Main = curVerInfo.ToString();

                string versionFilePath = m_FileSystemSvc.AppNextRoot + "/version.dat";
                if (System.IO.File.Exists(versionFilePath))
                {
                    string nextVer = System.IO.File.ReadAllText(versionFilePath);
                    var nextVerInfo = Utils.ParseVersionInfo(nextVer);

                    if (nextVerInfo != null && Utils.CompareVersion(curVerInfo, nextVerInfo) < 0)
                    {
                        data.Next = nextVer;
                        //data.Files = GetAllFilesInNext();
                    }
                }

                versionFilePath = m_FileSystemSvc.AppPrevRoot + "/version.dat";
                if (System.IO.File.Exists(versionFilePath))
                {
                    string prevVer = System.IO.File.ReadAllText(versionFilePath);
                    var prevVerInfo = Utils.ParseVersionInfo(prevVer);

                    if (prevVerInfo != null && Utils.CompareVersion(curVerInfo, prevVerInfo) > 0)
                    {
                        data.Prev = prevVer;
                    }
                }
            }
            catch (Exception) { }

            try
            {
                // TODO: change this to Main on release;
                var fileinfos = m_FileSystemSvc.GetFilesInNext();
                List<FileInfoModel> list = new(fileinfos.Count);

                foreach (var file in fileinfos)
                {
                    list.Add(new FileInfoModel(file, m_FileSystemSvc.AppNextRoot));
                }

                data.Files = list;
            }
            catch (Exception _ex)
            {
                return Content(MessageTag.Exception + _ex, MediaTypeNames.Text.Plain);
            }

            string dataJson = JsonSerializer.Serialize(data, P24JsonSerializerContext.Default.UpdaterPageDataModel);
            return Content(MessageTag.Success + dataJson, MediaTypeNames.Text.Plain);
        }

        // ajax call only;
        public IActionResult OnPostClearInternalError()
        {
            if (!ModelState.IsValid)
                return Content(MessageTag.Error + "Invalid ModelState (clear internal error)", MediaTypeNames.Text.Plain);

            m_UpdaterSvc.ClearErrorMessage();
            return Content(MessageTag.Success, MediaTypeNames.Text.Plain);
        }

        // ajax call only;
        public IActionResult OnPostUploadMetadataAsync([FromBody] UpdaterMetadata _data)
        {
            if (!ModelState.IsValid)
            {
                return Content(MessageTag.Error + "Invalid ModelState (metadata upload)", MediaTypeNames.Text.Plain);
            }

            m_UpdaterSvc.StartMonitoringFileUpload(_data);

            return Content(MessageTag.Success, MediaTypeNames.Text.Plain);
        }

        // ajax call only;
        public async Task<IActionResult> OnPostUploadBatchAsync(IList<IFormFile> _files)
        {
            int batchId = int.Parse(Request.Headers["Id"]);
            long batchSize = long.Parse(Request.Headers["Size"]);
            int batchCount = int.Parse(Request.Headers["Count"]);

            long requestLength = (long)Request.ContentLength;
            if (requestLength > Constants.MaxRequestSize)
            {
                string msg = "[" + batchId + ", \""
                    + string.Format(P24Localization.Active[LOCL.DESC_UPDATER_ERR_BATCH_OVERSIZE], Constants.MaxRequestSize)
                    + "\"]";
                return Content(MessageTag.Error + msg, MediaTypeNames.Text.Plain);
            }

            if (!ModelState.IsValid)
            {
                string msg = "[" + batchId + ", \"Invalid ModelState.\"]";
                return Content(MessageTag.Error + msg, MediaTypeNames.Text.Plain);
            }

            if (batchId < 0 || batchId > m_UpdaterSvc.GetBatchesCount())
            {
                //TODO: do some unnecessary check;
            }

            if (_files.Count != batchCount)
            {
                string msg = "[" + batchId + ", \""
                    + string.Format(P24Localization.Active[LOCL.DESC_UPDATER_ERR_BATCH_COUNT_MISMATCH], _files.Count, batchCount)
                    + "\"]";
                return Content(MessageTag.Error + msg, MediaTypeNames.Text.Plain);
            }

            long totalSize = 0L;
            foreach (var file in _files)
                totalSize += file.Length;

            if (totalSize != batchSize)
            {
                string msg = "[" + batchId + ", \""
                    + string.Format(P24Localization.Active[LOCL.DESC_UPDATER_ERR_BATCH_SIZE_MISMATCH], totalSize, batchSize)
                    + "\"]";
                return Content(MessageTag.Error + msg, MediaTypeNames.Text.Plain);
            }

            // ==================================================

            DateTime epoch = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            List<long> errors = new();

            foreach (var file in _files)
            {
                var fi = ComputeFileInfo(file);

                if (fi == null)
                    return Content(MessageTag.Error + "[" + batchId + ", \"Bad File: <code>" + file.FileName + "</code>.\"", MediaTypeNames.Text.Plain);

                DateTime dt;
                if (m_UpdaterSvc.IsFileLastModTracked(fi.Item3))
                {
                    long millis = m_UpdaterSvc.GetFileLastMod(fi.Item3);
                    dt = epoch.AddMilliseconds(millis).ToLocalTime();
                }
                else
                {
                    dt = DateTime.Now;
                }

                // TODO: async this;

                if (await SaveFile(file, fi.Item1, fi.Item2, dt) < 0)
                {
                    errors.Add(fi.Item3);
                }
            }

            if (errors.Count > 0)
                return Content(MessageTag.Error + "[" + batchId + "," + errors.Count + "]", MediaTypeNames.Text.Plain);

            m_UpdaterSvc.SetBatchStatus(batchId, true);
            if (m_UpdaterSvc.IsAllBatchesSuccess())
            {
                m_UpdaterSvc.StopMonitoringFileUpload();
            }

            return Content(MessageTag.Success + batchId, MediaTypeNames.Text.Plain);
        }

        public IActionResult OnPostPurgePrev()
        {
            return PurgeCommon(UpdaterSide.Prev);
        }

        public IActionResult OnPostPurgeNext()
        {
            return PurgeCommon(UpdaterSide.Next);
        }

        public IActionResult OnPostAbortPrev()
        {
            return AbortCommon(UpdaterSide.Prev);
        }

        public IActionResult OnPostAbortNext()
        {
            return AbortCommon(UpdaterSide.Next);
        }

        public IActionResult OnPostApplyPrev()
        {
            return ApplyCommon(UpdaterSide.Prev);
        }

        public IActionResult OnPostApplyNext()
        {
            return ApplyCommon(UpdaterSide.Next);
        }


        private IActionResult PurgeCommon(UpdaterSide _side)
        {
            UpdaterStatus codePQ;
            UpdaterStatus codePR;

            if (_side == UpdaterSide.Prev)
            {
                codePQ = UpdaterStatus.PrevPurgeQueued;
                codePR = UpdaterStatus.PrevPurgeRunning;
            }
            else if (_side == UpdaterSide.Next)
            {
                codePQ = UpdaterStatus.NextPurgeQueued;
                codePR = UpdaterStatus.NextPurgeRunning;
            }
            else
            {
                return Content(MessageTag.Error + ErrCode.InvalidBlockName, MediaTypeNames.Text.Plain);
            }

            if (m_UpdaterSvc.Status == codePQ)
                return Content(MessageTag.Success + (int)ErrorFlagBit.Error + ":" + (int)codePQ, MediaTypeNames.Text.Plain);

            if (m_UpdaterSvc.Status == codePR)
                return Content(MessageTag.Success + (int)ErrorFlagBit.Error + ":" + (int)codePR, MediaTypeNames.Text.Plain);

            if (_side == UpdaterSide.Prev)
                m_UpdaterSvc.StartPurgePrev();
            else if (_side == UpdaterSide.Next)
                m_UpdaterSvc.StartPurgeNext();
            else
                return Content(MessageTag.Error + ErrCode.InvalidBlockName, MediaTypeNames.Text.Plain);

            return Content(MessageTag.Success + (int)ErrorFlagBit.NoError + ":" + (int)codePQ, MediaTypeNames.Text.Plain);
        }

        private IActionResult ApplyCommon(UpdaterSide _side)
        {
            UpdaterStatus codeAQ;
            UpdaterStatus codeAR;
            UpdaterStatus codePQ;
            UpdaterStatus codePR;

            if (_side == UpdaterSide.Prev)
            {
                codeAQ = UpdaterStatus.PrevApplyQueued;
                codeAR = UpdaterStatus.PrevApplyRunning;
                codePQ = UpdaterStatus.PrevPurgeQueued;
                codePR = UpdaterStatus.PrevPurgeRunning;
            }
            else if (_side == UpdaterSide.Next)
            {
                codeAQ = UpdaterStatus.NextApplyQueued;
                codeAR = UpdaterStatus.NextApplyRunning;
                codePQ = UpdaterStatus.NextPurgeQueued;
                codePR = UpdaterStatus.NextPurgeRunning;
            }
            else
                return Content(MessageTag.Error + ErrCode.InvalidBlockName, MediaTypeNames.Text.Plain);

            if (m_UpdaterSvc.Status == codeAQ || m_UpdaterSvc.Status == codeAR
                || m_UpdaterSvc.Status == codePQ || m_UpdaterSvc.Status == codePR)
                return Content(MessageTag.Success + (int)ErrorFlagBit.Error + ":" + (int)m_UpdaterSvc.Status, MediaTypeNames.Text.Plain);

            if (_side == UpdaterSide.Prev)
                m_UpdaterSvc.StartApplyPrev();
            else if (_side == UpdaterSide.Next)
                m_UpdaterSvc.StartApplyNext();
            else
                return Content(MessageTag.Error + ErrCode.InvalidBlockName, MediaTypeNames.Text.Plain);

            return Content(MessageTag.Success + (int)ErrorFlagBit.NoError + ":" + (int)codeAQ, MediaTypeNames.Text.Plain);
        }

        private IActionResult AbortCommon(UpdaterSide _side)
        {
            if (_side == UpdaterSide.Prev)
            { }
            else if (_side == UpdaterSide.Next)
            { }
            else
            {
                return Content(MessageTag.Error + ErrCode.InvalidBlockName, MediaTypeNames.Text.Plain);
            }

            if (m_UpdaterSvc.Status == UpdaterStatus.None)
                return Content(MessageTag.Success + "0:" + (int)UpdaterStatus.None, MediaTypeNames.Text.Plain);

            if (m_UpdaterSvc.Status == UpdaterStatus.PrevPurgeRunning || m_UpdaterSvc.Status == UpdaterStatus.NextPurgeRunning)
                return Content(MessageTag.Warning + "0:" + (int)m_UpdaterSvc.Status, MediaTypeNames.Text.Plain);

            m_UpdaterSvc.Abort();
            return Content(MessageTag.Success + "1:" + (int)UpdaterStatus.None, MediaTypeNames.Text.Plain);
        }

        /// <summary>
        /// Saves the file to disk at <c>AppNextRoot</c> location. NOTE that the file path should contains a trailing space if not empty.<br />
        /// (Sees <c>ComputeFileInfo()</c> for more information.
        /// </summary>
        /// <param name="_file">The uploaded file to be saved</param>
        /// <param name="_path">The file's path AFTER <c>AppNextRoot</c></param>
        /// <param name="_name">The file's name</param>
        /// <param name="_lastModified">The file's last modified timestamp</param>
        /// <returns>The length of saved file, or -1 if error occures.</returns>
        private async Task<long> SaveFile(IFormFile _file, string _path, string _name, DateTime _lastModified)
        {
            if (_path != "")
                Directory.CreateDirectory(m_FileSystemSvc.AppNextRoot + "/" + _path);

            string fileFullname = m_FileSystemSvc.AppNextRoot + "/" + _path + _name;    // again, _path contains a trailing slash;
            try
            {
                Stream stream = System.IO.File.Create(fileFullname, 8 * 1024);
                await _file.CopyToAsync(stream);
                stream.Close();

                System.IO.File.SetLastWriteTime(fileFullname, _lastModified);
            }
            catch (Exception _e)
            {
                m_Logger.LogError("Error during upload file {FileFullname}:\r\n{ExceptionMsg}", fileFullname, _e.ToString());
                return -1;
            }

            return _file.Length;
        }

#if false
        private List<FileInfoModel> GetAllFilesInNext()
        {
            var fileinfos = m_FileSystemSvc.GetFilesInNext();
            List<FileInfoModel> result = new(fileinfos.Count);

            foreach (var file in fileinfos)
            {
                //    FileInfoModel fm = new()
                //    {
                //        Name = file.Name,
                //        Path = file.Path.Replace(m_FileSystemSvc.AppNextRoot, "").Replace('\\', '/').TrimStart('/'),
                //        LastMod = Utils.FormatDateTimeString_EndsWithMinute(file.LastModified),
                //        Size = file.Size
                //    };

                //    if (file.Path == "")
                //        fm.HashCode = Utils.ComputeCyrb53HashCode(file.Name);
                //    else
                //        fm.HashCode = Utils.ComputeCyrb53HashCode(file.Path + "/" + file.Name);

                //    result.Add(fm);
                result.Add(new FileInfoModel(file, m_FileSystemSvc.AppNextRoot));
            }

            return result;
        }
#endif

        /// <summary>
        /// Extracts the file's metadata consists of its path, file name, and hash code.<br />
        /// NOTE that the if the file path is not empty, it will contain a trailing slash.<br />
        /// For example, `` (empty path) will return `` (empty path), but <c>dir1</c> will return <c>dir1/</c>.
        /// </summary>
        /// <param name="_file">The uploaded file</param>
        /// <returns>The uploaded file's metadata consists of its path, file name and hash code.</returns>
        private static Tuple<string, string, long> ComputeFileInfo(IFormFile _file)
        {
            int prefixPos = _file.FileName.IndexOf('/');
            if (prefixPos < 0)
                return null;

            string name;
            string path = "";

            int pos = _file.FileName.LastIndexOf('/');
            if (pos > prefixPos)
            {
                path = _file.FileName[prefixPos..(pos + 1)];
                name = _file.FileName[(pos + 1)..];
            }
            else
            {
                name = _file.FileName[(prefixPos + 1)..];
            }

            long hashCode = Utils.ComputeCyrb53HashCode(path + name);

            return Tuple.Create(path, name, hashCode);
        }


        private readonly UpdaterSvc m_UpdaterSvc = null;
        private readonly FileSystemSvc m_FileSystemSvc = null;
        private readonly ILogger<UpdaterModel> m_Logger = null;
    }

}

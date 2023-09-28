/*  Home/Management/Updater.cshtml.cs
 *  Version: v1.3 (2023.09.27)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.App;
using Project24.App.BackendData;
using Project24.App.Services;
using Project24.Data;
using Project24.Model;

namespace Project24.Pages.Home.Maintenance
{
    public class UpdaterModel : PageModel
    {
        #region View Model
        // ==================================================
        // View Model

        public class UpdaterPageDataModel
        {
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public UpdaterStatus Status { get; set; } = UpdaterStatus.None;
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public UpdaterQueuedAction QueuedAction { get; set; } = UpdaterQueuedAction.None;
            public DateTime DueTime { get; set; } = DateTime.Now;
            public TimeSpan Remaining { get; set; } = TimeSpan.Zero;
            public string AppSide { get; private set; } = Program.AppSide;

            public string MainVer { get; set; } = null;
            public string PrevVer { get; set; } = null;
            public string NextVer { get; set; } = null;

            public List<FileInfoPageDataModel> Files { get; set; } = null;

            public string Message { get; set; } = "";


            public UpdaterPageDataModel()
            { }
        }

        public class FileInfoPageDataModel
        {
            public string Path { get; set; }
            public string LastMod { get; set; }
            public JavascriptFile File { get; set; }
            public long HashCode { get; set; }

            [JsonIgnore]
            public string Name { get => File.name; set => File.name = value; }
            [JsonIgnore]
            public long Size { get => File.size; set => File.size = value; }


            public FileInfoPageDataModel(P24FileInfo _fileInfo, string _maskPath = "")
            {
                File = new();

                Name = _fileInfo.Name;
                Path = _fileInfo.Path.Replace(_maskPath, "").Replace('\\', '/').Trim('/');
                LastMod = MiscUtils.FormatDateTimeString_EndsWithMinute(_fileInfo.LastModified);
                Size = _fileInfo.Size;

                if (Path == "")
                    HashCode = MiscUtils.ComputeCyrb53HashCode(_fileInfo.Name);
                else
                    HashCode = MiscUtils.ComputeCyrb53HashCode(Path + "/" + _fileInfo.Name);
                
            }
        }

        public class JavascriptFile
        {
            [JsonInclude] public string name;
            [JsonInclude] public long size;
        }

        // End: View Model
        // ==================================================
        #endregion

        //private class UploadedFileInfo
        //{
        //    public string FullName { get; set; }
        //    public long Length { get; set; }


        //    public UploadedFileInfo(string _fullName, long _length)
        //    {
        //        FullName = _fullName;
        //        Length = _length;
        //    }
        //}


        public UpdaterModel(ApplicationDbContext _dbContext, UpdaterSvc _updaterSvc, FileSystemSvc _fileSystemSvc, ILogger<UpdaterModel> _logger)
        {
            m_DbContext = _dbContext;

            m_UpdaterSvc = _updaterSvc;
            m_FileSystemSvc = _fileSystemSvc;
            m_Logger = _logger;
        }


        public void OnGet()
        { }

        #region AJAX Handler
        // ==================================================
        // AJAX Handler

        // ajax handler;
        public IActionResult OnGetFetchPageData()
        {
            UpdaterPageDataModel data = new()
            {
                Status = m_UpdaterSvc.Status,
                QueuedAction = m_UpdaterSvc.QueuedAction,
                DueTime = m_UpdaterSvc.QueuedActionDueTime,
                Remaining = m_UpdaterSvc.QueueActionRemainingTime,
            };

            if (m_UpdaterSvc.LastErrorMessage != null)
                data.Message = m_UpdaterSvc.LastErrorMessage;

            try
            {
                VersionInfo curVerInfo = new(Assembly.GetEntryAssembly().GetName().Version);
                data.MainVer = curVerInfo.ToString();

                string versionFilePath = m_FileSystemSvc.AppNextRoot + "/version.dat";
                if (System.IO.File.Exists(versionFilePath))
                {
                    string nextVer = System.IO.File.ReadAllText(versionFilePath);
                    var nextVerInfo = MiscUtils.ParseVersionInfo(nextVer);

                    if (nextVerInfo != null)
                    {
                        int compare = MiscUtils.CompareVersion(curVerInfo, nextVerInfo);
                        if (compare > 0)
                            nextVer += " (outdated)";
                        else if (compare == 0)
                            nextVer += " (same)";

                        data.NextVer = nextVer;
                    }
                }

                versionFilePath = m_FileSystemSvc.AppPrevRoot + "/version.dat";
                if (System.IO.File.Exists(versionFilePath))
                {
                    string prevVer = System.IO.File.ReadAllText(versionFilePath);
                    var prevVerInfo = MiscUtils.ParseVersionInfo(prevVer);

                    if (prevVerInfo != null)
                    {
                        int compare = MiscUtils.CompareVersion(curVerInfo, prevVerInfo);
                        if (compare == 0)
                            prevVer += " (same)";

                        data.PrevVer = prevVer;
                    }
                }
            }
            catch (Exception _ex)
            {
                m_Logger.LogError("Exception during reading avaiable prev/next version: {_ex}", _ex);
            }

            try
            {
                // TODO: change this to Main on release;
                var fileinfos = m_FileSystemSvc.GetFilesInNext();
                List<FileInfoPageDataModel> list = new(fileinfos.Count);

                foreach (var file in fileinfos)
                {
                    list.Add(new FileInfoPageDataModel(file, m_FileSystemSvc.AppNextRoot));
                }

                data.Files = list;
            }
            catch (Exception _ex)
            {
                m_Logger.LogError("Exception during reading next version file list: {_ex}", _ex);
                return Content(MessageTag.Exception + _ex, MediaTypeNames.Text.Plain);
            }

            //string dataJson = JsonSerializer.Serialize(data, P24JsonSerializerContext.Default.UpdaterPageDataModel);
            string dataJson = JsonSerializer.Serialize(data);
            return Content(MessageTag.Success + dataJson, MediaTypeNames.Text.Plain);
        }

        // ajax handler;
        public IActionResult OnPostClearInternalError()
        {
            if (!ModelState.IsValid)
                return Content(MessageTag.Error + "Invalid ModelState (clear internal error)", MediaTypeNames.Text.Plain);

            m_UpdaterSvc.ClearErrorMessage();
            return Content(MessageTag.Success, MediaTypeNames.Text.Plain);
        }

        // ajax handler;
        public IActionResult OnPostUploadMetadataAsync([FromBody] UpdaterMetadata _data)
        {
            if (!ModelState.IsValid)
            {
                return Content(MessageTag.Error + "Invalid ModelState (metadata upload)", MediaTypeNames.Text.Plain);
            }

            m_UpdaterSvc.StartMonitoringFileUpload(_data);

            return Content(MessageTag.Success, MediaTypeNames.Text.Plain);
        }

        // ajax handler;
        public async Task<IActionResult> OnPostUploadBatchAsync(IList<IFormFile> _files, CancellationToken _cancellationToken)
        {
            int batchId = int.Parse(Request.Headers["Id"]);
            long batchSize = long.Parse(Request.Headers["Size"]);
            int batchCount = int.Parse(Request.Headers["Count"]);

            long requestLength = (long)Request.ContentLength;
            if (requestLength > Constants.MaxRequestSize)
            {
                string msg = "[" + batchId + ", \""
                    + string.Format(P24Localization.Active[LOCL.DESC_UPDATER_ERR_BATCH_OVERSIZE], Constants.MaxRequestSize, ErrCode.Updater_BatchOversize)
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
                    + string.Format(P24Localization.Active[LOCL.DESC_UPDATER_ERR_BATCH_COUNT_MISMATCH], _files.Count, batchCount, ErrCode.Updater_BatchCountMismatch)
                    + "\"]";
                return Content(MessageTag.Error + msg, MediaTypeNames.Text.Plain);
            }

            long totalSize = 0L;
            foreach (var file in _files)
                totalSize += file.Length;

            if (totalSize != batchSize)
            {
                string msg = "[" + batchId + ", \""
                    + string.Format(P24Localization.Active[LOCL.DESC_UPDATER_ERR_BATCH_SIZE_MISMATCH], totalSize, batchSize, ErrCode.Updater_BatchSizeMismatch)
                    + "\"]";
                return Content(MessageTag.Error + msg, MediaTypeNames.Text.Plain);
            }

            // ==================================================

            List<long> errors = new();
            List<string> successFileNames = new();
            int successFileLength = 0;

            foreach (var file in _files)
            {
                var fi = ComputeFileInfo(file);

                if (fi == null)
                    return Content(MessageTag.Error + "[" + batchId + ", \"Bad File: <code>" + file.FileName + "</code>.\"", MediaTypeNames.Text.Plain);

                DateTime dt;
                if (m_UpdaterSvc.IsFileLastModTracked(fi.Item3))
                {
                    long millis = m_UpdaterSvc.GetFileLastMod(fi.Item3);
                    dt = Constants.Epoch.AddMilliseconds(millis).ToLocalTime();
                }
                else
                {
                    dt = DateTime.Now;
                }

                if (SaveFile(file, fi.Item1, fi.Item2, dt) < 0)
                {
                    errors.Add(fi.Item3);
                }
                else
                {
                    successFileNames.Add(fi.Item1 + fi.Item2);
                    successFileLength += (int)file.Length;
                }
            }

            if (successFileNames.Count > 0)
            {
                // TODO: add into user upload list;

                string jsonData = JsonSerializer.Serialize(successFileNames);
                UserUploadData uploadData = new("power", (short)successFileNames.Count, successFileLength, jsonData);

                m_DbContext.UserUploadDatas.Add(uploadData);
                await m_DbContext.SaveChangesAsync(_cancellationToken);
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

        // ajax handler;
        public IActionResult OnPostPurgePrev() => PurgeCommon(UpdaterSide.Prev);

        // ajax handler;
        public IActionResult OnPostPurgeNext() => PurgeCommon(UpdaterSide.Next);

        // ajax handler;
        public IActionResult OnPostAbortPrev() => AbortCommon(UpdaterSide.Prev);

        // ajax handler;
        public IActionResult OnPostAbortNext() => AbortCommon(UpdaterSide.Next);

        // ajax handler;
        public IActionResult OnPostApplyPrev() => ApplyCommon(UpdaterSide.Prev);

        // ajax handler;
        public IActionResult OnPostApplyNext() => ApplyCommon(UpdaterSide.Next);

        // End: AJAX Handler
        // ==================================================
        #endregion


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
        private long SaveFile(IFormFile _file, string _path, string _name, DateTime _lastModified)
        {
            if (_path != "")
                Directory.CreateDirectory(m_FileSystemSvc.AppNextRoot + "/" + _path);

            string fileFullname = m_FileSystemSvc.AppNextRoot + "/" + _path + _name;    // again, _path contains a trailing slash;
            try
            {
                Stream stream = System.IO.File.Create(fileFullname, 4 * 1024 * 1024);
                _file.CopyTo(stream);
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
                path = _file.FileName[(prefixPos + 1)..(pos + 1)];
                name = _file.FileName[(pos + 1)..];
            }
            else
            {
                name = _file.FileName[(prefixPos + 1)..];
            }

            long hashCode = MiscUtils.ComputeCyrb53HashCode(path + name);

            return Tuple.Create(path, name, hashCode);
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UpdaterSvc m_UpdaterSvc;
        private readonly FileSystemSvc m_FileSystemSvc;
        private readonly ILogger<UpdaterModel> m_Logger;
    }

}

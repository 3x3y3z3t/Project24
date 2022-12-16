/*  TusDotNetConfig.cs
 *  Version: 1.3 (2022.12.16)
 *
 *  Contributor
 *      Arime-chan
 */

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Project24.Data;
using Project24.Models;
using Project24.Models.Identity;
using Project24.Models.Nas;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Models.Configuration;
using tusdotnet.Stores;

namespace Project24.App
{
    public class TusDotNetConfig
    {
        private class RequestData
        {
            public HttpContext HttpContext = null;
            public ApplicationDbContext DbContext = null;
            public ILogger<TusDotNetConfig> Logger = null;

            public UploadedFileMetadata Metadata = null;
        }

        private class UploadedFileMetadata
        {
            public string Path = null;
            public string Name = null;
            public long Length = 0L;
            public DateTime LastModifiedDate = DateTime.Now;
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static DefaultTusConfiguration ConfigureTusDotNet(HttpContext _httpContext)
        {
            string nasCacheAbsPath = Path.GetFullPath(AppUtils.AppRoot + "/" + AppConfig.TmpRoot);

            TusDiskBufferSize tusDiskBufferSize = new TusDiskBufferSize(8 * 1024 * 1024);

            // This method is called on each request so different configurations can be returned per user, domain, path etc.
            // Return null to disable tusdotnet for the current request.

            return new DefaultTusConfiguration()
            {
                // c:\tusfiles is where to store files
                Store = new TusDiskStore(nasCacheAbsPath, true, tusDiskBufferSize),

                // On what url should we listen for uploads?
                UrlPath = "/Nas/Upload0",

                MaxAllowedUploadSizeInBytesLong = 10L * 1024L * 1024L * 1024L, /* allow 10GiB */

                Expiration = new tusdotnet.Models.Expiration.SlidingExpiration(new TimeSpan(3, 0, 0, 0)),

                Events = new Events()
                {
                    OnBeforeCreateAsync = TusDotNet_OnBeforeCreateAsync,
                    OnCreateCompleteAsync = TusDotNet_OnCreateCompleteAsync,
                    OnFileCompleteAsync = TusDotNet_OnFileCompleteAsync,
                },
            };
        }

        private static Task TusDotNet_OnBeforeCreateAsync(BeforeCreateContext _eventContext)
        {
            if (!_eventContext.Metadata.ContainsKey("fileName"))
            {
                _eventContext.FailRequest("fileName metadata must be specified. ");
            }

            if (!_eventContext.Metadata.ContainsKey("fileSize"))
            {
                _eventContext.FailRequest("fileSize metadata must be specified. ");
            }

            if (!_eventContext.Metadata.ContainsKey("filePath"))
            {
                _eventContext.FailRequest("filePath metadata must be specified. ");
            }

            if (!_eventContext.Metadata.ContainsKey("contentType"))
            {
                _eventContext.FailRequest("contentType metadata must be specified. ");
            }

            string filePath = _eventContext.Metadata["filePath"].GetString(Encoding.UTF8);

            try
            {
                string uploadLocationAbsPath = Path.GetFullPath(DriveUtils.TmpRootPath + "/" + filePath.Remove(0, 5));
                if (!uploadLocationAbsPath.Contains("nasTmp"))
                {
                    _eventContext.FailRequest("Invalid path '" + filePath + "'. ");
                }
            }
            catch (Exception)
            {
                _eventContext.FailRequest("Invalid path '" + filePath + "'. ");
            }

            return Task.CompletedTask;
        }

        private static Task TusDotNet_OnCreateCompleteAsync(CreateCompleteContext _eventContext)
        {
            return Task.CompletedTask;
        }

        private static UploadedFileMetadata GetUploadedFileMetadata(Dictionary<string, Metadata> _tusMetadata)
        {
            string name = _tusMetadata["fileName"].GetString(Encoding.UTF8);
            long length = long.Parse(_tusMetadata["fileSize"].GetString(Encoding.UTF8));
            string path = _tusMetadata["filePath"].GetString(Encoding.UTF8).Remove(0, 5);

            DateTime lastModDate = DateTime.Now;
            if (_tusMetadata.ContainsKey("fileDate"))
            {
                string datetimeString = _tusMetadata["fileDate"].GetString(Encoding.UTF8);
                lastModDate = DateTime.Parse(datetimeString);
            }

            return new UploadedFileMetadata()
            {
                Path = path,
                Name = name,
                Length = length,
                LastModifiedDate = lastModDate
            };
        }

        private static async Task TusDotNet_OnFileCompleteAsync(FileCompleteContext _eventContext)
        {
            var logger = _eventContext.HttpContext.RequestServices.GetRequiredService<ILogger<TusDotNetConfig>>();
            ApplicationDbContext dbContext = _eventContext.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

            ITusFile file = await _eventContext.GetFileAsync();
            UploadedFileMetadata metadata = GetUploadedFileMetadata(await file.GetMetadataAsync(_eventContext.CancellationToken));
            RequestData requestData = new RequestData()
            {
                HttpContext = _eventContext.HttpContext,
                DbContext = dbContext,
                Logger = logger,
                Metadata = metadata
            };

            Stream fileContent = await file.GetContentAsync(_eventContext.CancellationToken);
            if (!await ValidateFileContentLengthAsync(requestData, fileContent.Length, _eventContext.CancellationToken))
            {
                fileContent.Close();
                await DeleteTempFile(_eventContext);
                return;
            }

            if (! await WriteFileAsync(requestData, fileContent, _eventContext.CancellationToken))
            {
                fileContent.Close();
                await DeleteTempFile(_eventContext);

                await dbContext.RecordChanges(
                    _eventContext.HttpContext.User.Identity.Name,
                    ActionRecord.Operation_.UploadNasFile,
                    ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.Path, metadata.Path },
                        { CustomInfoKey.Filename, metadata.Name },
                        { CustomInfoKey.Size, metadata.Length.ToString() },
                        { CustomInfoKey.Error, "File creation failed" }
                    }
                );

                return;
            }

            fileContent.Close();

            string uploadLocation = Path.GetFullPath(DriveUtils.TmpRootPath + "/" + metadata.Path);
            string fileFullname = Path.GetFullPath(DriveUtils.TmpRootPath + "/" + metadata.Path + "/" + metadata.Name);

            FileInfo fi = new FileInfo(fileFullname);
            if (!await ValidateFileContentLengthAsync(requestData, fi.Length, _eventContext.CancellationToken))
            {
                await DeleteTempFile(_eventContext);
                return;
            }

            await DeleteTempFile(_eventContext);

            P24IdentityUser user = await (from _user in dbContext.P24Users
                                          where _user.UserName == _eventContext.HttpContext.User.Identity.Name
                                          select _user)
                                   .FirstOrDefaultAsync();

            await dbContext.AddAsync(new UserUpload(user, AppModule.P24b_Nas, 1, metadata.Length));
            await dbContext.AddAsync(new NasCachedFile()
            {
                AddedDate = DateTime.Now,
                Path = metadata.Path,
                Name = metadata.Name,
                Length = metadata.Length,
                LastModDate = metadata.LastModifiedDate
            });

            await dbContext.RecordChanges(
                _eventContext.HttpContext.User.Identity.Name,
                ActionRecord.Operation_.UploadNasFile,
                ActionRecord.OperationStatus_.Success,
                new Dictionary<string, string>()
                {
                    { CustomInfoKey.Path, metadata.Path },
                    { CustomInfoKey.Filename, metadata.Name },
                    { CustomInfoKey.Size, metadata.Length.ToString() },
                }
            );
        }

        private static async Task DeleteTempFile(FileCompleteContext _eventContext)
        {
            var terminationStore = (ITusTerminationStore)_eventContext.Store;
            await terminationStore.DeleteFileAsync(_eventContext.FileId, _eventContext.CancellationToken);
        }

        private static async Task<bool> ValidateFileContentLengthAsync(RequestData _data, long _actualLength, CancellationToken _cancellationToken = default)
        {
            if (_actualLength == _data.Metadata.Length)
                return true;

            await _data.DbContext.RecordChanges(
                _data.HttpContext.User.Identity.Name,
                ActionRecord.Operation_.UploadNasFile,
                ActionRecord.OperationStatus_.Failed,
                new Dictionary<string, string>()
                {
                        { CustomInfoKey.Path, _data.Metadata.Path },
                        { CustomInfoKey.Filename, _data.Metadata.Name },
                        { CustomInfoKey.Size, _data.Metadata.Length.ToString() },
                        { CustomInfoKey.Error, "Uploaded size: " + _actualLength.ToString() }
                }
            );
            LogErrorFileSizeMismatch(_data, _actualLength);

            return false;
        }

        private static async Task<bool> WriteFileAsync(RequestData _data, Stream _content, CancellationToken _cancellationToken = default)
        {
            try
            {
                string uploadLocation = Path.GetFullPath(DriveUtils.TmpRootPath + "/" + _data.Metadata.Path);

                Directory.CreateDirectory(uploadLocation);

                DateTime start = DateTime.Now;

                FileStream fileStream = File.Create(uploadLocation + "/" + _data.Metadata.Name, c_WriteFileBufferSize, FileOptions.Asynchronous);
                await _content.CopyToAsync(fileStream, c_WriteFileBufferSize, _cancellationToken);
                fileStream.Close();

                TimeSpan elapsed = DateTime.Now - start;
            }
            catch (Exception _e)
            {
                _data.Logger.LogError("Error during write file: " + _e);
                return false;
            }

            return true;
        }

        private static void LogErrorFileSizeMismatch(RequestData _data, long _actualLength)
        {
            string fullname = (_data.Metadata.Path + "/" + _data.Metadata.Name).Trim('/');

            string log = "File size mismatch on file " + fullname + ":\r\n";
            log += "Expected size: " + _data.Metadata.Length + ", actual size: " + _actualLength;

            _data.Logger.LogError(log);
        }


        private const int c_WriteFileBufferSize = 4 * 1024 * 1024;

        //private static readonly string s_NasCacheAbsPath = Path.GetFullPath(AppUtils.AppRoot + "/" + AppConfig.TmpRoot);
    }

}

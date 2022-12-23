/*  TusDotNetConfig.cs
 *  Version: 1.5 (2022.12.24)
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
                string uploadLocationAbsPath = Path.GetFullPath(DriveUtils.TmpRootPath + "/" + filePath);
                if (!uploadLocationAbsPath.Contains("nasTmp"))
                {
                    _eventContext.FailRequest("Invalid path '" + filePath + "'. ");
                }
            }
            catch (Exception)
            {
                _eventContext.FailRequest("Invalid path '" + filePath + "'. ");
            }

            DateTime lastModDate = DateTime.Now;
            if (_eventContext.Metadata.ContainsKey("fileDate"))
            {
                string datetimeString = _eventContext.Metadata["fileDate"].GetString(Encoding.UTF8);
                if (long.TryParse(datetimeString, out long millis))
                {
                    DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    lastModDate = epoch.AddMilliseconds(millis).ToLocalTime();
                }
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
            string path = _tusMetadata["filePath"].GetString(Encoding.UTF8);

            DateTime lastModDate = DateTime.Now;
            if (_tusMetadata.ContainsKey("fileDate"))
            {
                string datetimeString = _tusMetadata["fileDate"].GetString(Encoding.UTF8);
                if (long.TryParse(datetimeString, out long millis))
                {
                    DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    lastModDate = epoch.AddMilliseconds(millis).ToLocalTime();
                }
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
            //return;


            var logger = _eventContext.HttpContext.RequestServices.GetRequiredService<ILogger<TusDotNetConfig>>();
            ApplicationDbContext dbContext = _eventContext.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

            ITusFile file = await _eventContext.GetFileAsync();
            UploadedFileMetadata metadata = GetUploadedFileMetadata(await file.GetMetadataAsync(_eventContext.CancellationToken));

            string nasCacheAbsPath = Path.GetFullPath(AppUtils.AppRoot + "/" + AppConfig.TmpRoot);
            try
            {
                File.Move(nasCacheAbsPath + "/" + _eventContext.FileId, nasCacheAbsPath + "/" + metadata.Name);
            }
            catch (Exception _e)
            {
                logger.LogError("TusDotNet_OnFileCompleteAsync: " + _e);
            }

            var terminationStore = (ITusTerminationStore)_eventContext.Store;
            await terminationStore.DeleteFileAsync(_eventContext.FileId, _eventContext.CancellationToken);

            await dbContext.AddAsync(new NasCachedFile()
            {
                AddedDate = DateTime.Now,
                Path = metadata.Path,
                Name = metadata.Name,
                Length = metadata.Length,
                LastModDate = metadata.LastModifiedDate
            });

            P24IdentityUser user = null;
            if (_eventContext.HttpContext.User.Identity.IsAuthenticated)
            {
                user = await (from _user in dbContext.P24Users
                                              where _user.UserName == _eventContext.HttpContext.User.Identity.Name
                                              select _user)
                                       .FirstOrDefaultAsync();

                await dbContext.AddAsync(new UserUpload(user, AppModule.P24b_Nas, 1, metadata.Length));
            }

            await dbContext.RecordChanges(
                user.UserName,
                ActionRecord.Operation_.UploadNasFile,
                ActionRecord.OperationStatus_.Success,
                new Dictionary<string, string>()
                {
                    { CustomInfoKey.Path, metadata.Path },
                    { CustomInfoKey.Filename, metadata.Name },
                    { CustomInfoKey.Size, metadata.Length.ToString() },
                }
            );

            return;
        }
    }

}

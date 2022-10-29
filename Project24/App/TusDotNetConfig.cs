/*  TusDotNetConfig.cs
 *  Version: 1.1 (2022.10.29)
 *
 *  Contributor
 *      Arime-chan
 */

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Project24.Data;
using Project24.Models.Nas;
using System;
using System.Collections.Generic;
using System.IO;
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
        public static DefaultTusConfiguration ConfigureTusDotNet(HttpContext _httpContext)
        {
            string nasCacheAbsPath = Path.GetFullPath(Project24.Utils.AppRoot + "/" + AppConfig.TmpRoot);

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
                string uploadLocationAbsPath = Path.GetFullPath(filePath.Remove(0, 5), s_NasCacheAbsPath);
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
            //string filePath = "";
            //if (_eventContext.Metadata.ContainsKey("filePath"))
            //{
            //    filePath = _eventContext.Metadata["filePath"].GetString(Encoding.UTF8);
            //}

            //string uploadLocationAbsPath = Path.GetFullPath(filePath, nasRootAbsPath);
            //TusDotNetUtils.AddOrReplace(_eventContext.FileId, new TusDotNetUtils.FileMetadata()
            //{
            //    Path = uploadLocationAbsPath,
            //    Filename = _eventContext.Metadata["fileName"].GetString(Encoding.UTF8)
            //});

            return Task.CompletedTask;
        }

        private static async Task TusDotNet_OnFileCompleteAsync(FileCompleteContext _eventContext)
        {
            var logger = _eventContext.HttpContext.RequestServices.GetRequiredService<ILogger<TusDotNetConfig>>();
            ApplicationDbContext dbContext = _eventContext.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

            ITusFile file = await _eventContext.GetFileAsync();
            Dictionary<string, Metadata> metadata = await file.GetMetadataAsync(_eventContext.CancellationToken);

            string fileName = metadata["fileName"].GetString(Encoding.UTF8);
            long fileSize = long.Parse(metadata["fileSize"].GetString(Encoding.UTF8));
            string filePath = metadata["filePath"].GetString(Encoding.UTF8).Remove(0, 5);

            string uploadLocationAbsPath = Path.GetFullPath(filePath, s_NasCacheAbsPath);

            Stream content = await file.GetContentAsync(_eventContext.CancellationToken);
            if (content.Length != fileSize)
            {
                long actualSize = content.Length;
                content.Close();

                await LogErrorFileSizeMismatch(logger, uploadLocationAbsPath + "/" + fileName, fileSize, actualSize);

                await DeleteTempFile(_eventContext);

                await dbContext.RecordChanges(
                    _eventContext.HttpContext.User.Identity.Name,
                    Models.ActionRecord.Operation_.UploadNasFile,
                    Models.ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.Path, filePath },
                        { CustomInfoKey.Filename, fileName },
                        { CustomInfoKey.Size, fileSize.ToString() },
                        { CustomInfoKey.Error, "Uploaded size: " + actualSize.ToString() }
                    }
                );

                return;
            }

            if (!await WriteFile(logger, content, uploadLocationAbsPath, fileName))
            {
                content.Close();

                await DeleteTempFile(_eventContext);

                await dbContext.RecordChanges(
                    _eventContext.HttpContext.User.Identity.Name,
                    Models.ActionRecord.Operation_.UploadNasFile,
                    Models.ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.Path, filePath },
                        { CustomInfoKey.Filename, fileName },
                        { CustomInfoKey.Size, fileSize.ToString() },
                        { CustomInfoKey.Error, "File creation failed" }
                    }
                );

                return;
            }

            FileInfo fi = new FileInfo(uploadLocationAbsPath + "/" + fileName);
            if (fi.Length != fileSize)
            {
                content.Close();

                await LogErrorFileSizeMismatch(logger, uploadLocationAbsPath + "/" + fileName, fileSize, fi.Length);

                await DeleteTempFile(_eventContext);

                await dbContext.RecordChanges(
                    _eventContext.HttpContext.User.Identity.Name,
                    Models.ActionRecord.Operation_.UploadNasFile,
                    Models.ActionRecord.OperationStatus_.Failed,
                    new Dictionary<string, string>()
                    {
                        { CustomInfoKey.Path, filePath },
                        { CustomInfoKey.Filename, fileName },
                        { CustomInfoKey.Size, fileSize.ToString() },
                        { CustomInfoKey.Error, "Created file size: " + fi.Length.ToString() }
                    }
                );

                return;
            }

            content.Close();

            await DeleteTempFile(_eventContext);

            NasCachedFile cachedFile = new NasCachedFile()
            {
                Name = fileName,
                Path = filePath,
                AddedDate = DateTime.Now
            };
            await dbContext.NasCachedFiles.AddAsync(cachedFile);

            //var username = _eventContext.HttpContext.User.Identity.Name;
            await dbContext.RecordChanges(
                _eventContext.HttpContext.User.Identity.Name,
                Models.ActionRecord.Operation_.UploadNasFile,
                Models.ActionRecord.OperationStatus_.Success,
                new Dictionary<string, string>()
                {
                    { CustomInfoKey.Path, filePath },
                    { CustomInfoKey.Filename, fileName },
                    { CustomInfoKey.Size, fileSize.ToString() }
                }
            );
        }

        private static async Task DeleteTempFile(FileCompleteContext _eventContext)
        {
            var terminationStore = (ITusTerminationStore)_eventContext.Store;
            await terminationStore.DeleteFileAsync(_eventContext.FileId, _eventContext.CancellationToken);
        }

        private static async Task LogErrorFileSizeMismatch(ILogger<TusDotNetConfig> _logger, string _fullname, long _expectedSize, long _actualSize)
        {
            string log = "File size mismatch on file " + _fullname + ":\r\n";
            log += "Expected size: " + _expectedSize + ", actual size: " + _actualSize;

            _logger.LogError(log);
        }

        private static async Task<bool> WriteFile(ILogger<TusDotNetConfig> _logger, Stream _content, string _path, string _fileName)
        {
            try
            {
                Directory.CreateDirectory(_path);

                DateTime start = DateTime.Now;

                FileStream fileStream = File.Create(_path + "/" + _fileName, c_WriteFileBufferSize, FileOptions.Asynchronous);
                await _content.CopyToAsync(fileStream, c_WriteFileBufferSize);

                TimeSpan elapsed = DateTime.Now - start;

                fileStream.Close();
            }
            catch (Exception _e)
            {
                _logger.LogError("Could not create file " + _path + "/" + _fileName + ":\r\n" + _e.ToString());
                return false;
            }

            return true;
        }

        private const int c_WriteFileBufferSize = 4 * 1024 * 1024;

        private static string s_NasCacheAbsPath = Path.GetFullPath(Project24.Utils.AppRoot + "/" + AppConfig.TmpRoot);
    }

}

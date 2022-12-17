/*  NasDiskService.cshtml
 *  Version: 1.4 (2022.12.18)
 *
 *  Contributor
 *      Arime-chan
 */

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Project24.Data;
using Project24.Models.Nas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Project24.App.Services
{
    public class NasDiskService : IHostedService, IDisposable
    {
        private class RequestData
        {
            public ApplicationDbContext DbContext = null;

            public string NasRootAbsPath = null;
            public string NasCacheAbsPath = null;
        }


        public NasDiskService(IServiceProvider _serviceProvider, ILogger<NasDiskService> _logger)
        {
            m_ServiceProvider = _serviceProvider;
            m_Logger = _logger;

            m_TransferInProgress = new HashSet<int>();
        }


        public Task StartAsync(CancellationToken _cancellationToken)
        {
            m_Timer = new Timer(DoWork, null, TimeSpan.FromSeconds(20.0), TimeSpan.FromSeconds(60.0));

            m_Logger.LogInformation("NasDiskService started.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken _cancellationToken)
        {
            if (m_Timer != null)
            {
                m_Timer.Change(Timeout.Infinite, 0);
            }

            if (m_TransferInProgress.Count > 0)
                m_Logger.LogWarning("There are still " + m_TransferInProgress + " transfer(s) in progress, arbort anyway.");

            m_Logger.LogInformation("NasDiskService stopped.");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (m_Timer != null)
                m_Timer.Dispose();
        }

        private void DoWork(object? _state)
        {
            var count = Interlocked.Increment(ref m_ExecutionCount);
            var scope = m_ServiceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (dbContext.NasCachedFiles.Count() <= 0)
                return;

            string nasRootAbsPath = Path.GetFullPath(AppUtils.AppRoot + "/" + AppConfig.NasRoot);
            string nasCacheAbsPath = Path.GetFullPath(AppUtils.AppRoot + "/" + AppConfig.TmpRoot);
            RequestData requestData = new RequestData()
            {
                DbContext = dbContext,
                NasRootAbsPath = nasRootAbsPath,
                NasCacheAbsPath = nasCacheAbsPath
            };

            int fileCount = 0;
            long fileLength = 0L;
            DateTime start = DateTime.Now;

            List<NasCachedFile> list = dbContext.NasCachedFiles.OrderBy(_file => _file.AddedDate).ToList();
            foreach (NasCachedFile file in list)
            {
                if (CheckIfFileNotExists(requestData, file))
                {
                    continue;
                }

                if (CheckIfTransferInProgress(requestData, file))
                {
                    continue;
                }

                // all check pass, perform file moving;
                if (MoveFile(requestData, file))
                {
                    ++fileCount;
                    fileLength += file.Length;
                }
            }

            TimeSpan elapsed = DateTime.Now - start;

            string log = "NasDiskService cycle " + m_ExecutionCount + "\r\n";
            log += "    Moved " + fileCount + " files (" + AppUtils.FormatDataSize(fileLength) + ")";
            log += ", avg " + AppUtils.FormatDataSize((long)(fileLength / elapsed.TotalSeconds)) + "/s";

            m_Logger.LogInformation(log);
        }

        private bool CheckIfFileNotExists(RequestData _data, NasCachedFile _file)
        {
            string src = _data.NasCacheAbsPath + "/" + _file.Name;

            if (File.Exists(src))
                return false;

            string logStr = "NasDiskService cycle " + m_ExecutionCount + ":\r\n";
            logStr += "File " + _file.Path + "/" + _file.Name + " doesn't exist anymore";
            m_Logger.LogWarning(logStr);

            lock (this)
            {
                m_TransferInProgress.Remove(_file.Id);
            }
            _data.DbContext.Remove(_file);
            _data.DbContext.SaveChanges();

            return true;
        }

        private bool CheckIfTransferInProgress(RequestData _data, NasCachedFile _file)
        {
            bool isTransfering;
            lock (this)
            {
                isTransfering = m_TransferInProgress.Contains(_file.Id);
            }

            return isTransfering;
        }

        private bool MoveFile(RequestData _data, NasCachedFile _file)
        {
            string src = _data.NasCacheAbsPath + "/" + _file.Path + "/" + _file.Name;
            string dst = _data.NasRootAbsPath + "/" + _file.Path + "/" + _file.Name;

            lock (this)
            {
                m_TransferInProgress.Add(_file.Id);
            }

            try
            {
                File.Move(src, dst, true);

                if (File.Exists(src))
                    File.Delete(src);
            }
            catch (Exception _e)
            {
                ++_file.FailCount;
                _data.DbContext.Update(_file);
                _data.DbContext.SaveChanges();

                string logStr = "NasDiskService cycle " + m_ExecutionCount + ":\r\n";
                logStr += "    path: \"" + _file.Path + "\"\r\n";
                logStr += "    name: \"" + _file.Name + "\"\r\n";
                logStr += "    src: \"" + src + "\"\r\n";
                logStr += "    dst: \"" + dst + "\"\r\n";
                logStr += "    Fail count: " + _file.FailCount + "\r\n";
                logStr += _e;

                m_Logger.LogError(logStr);
                return false;
            }

            lock (this)
            {
                m_TransferInProgress.Remove(_file.Id);
            }
            _data.DbContext.Remove(_file);
            _data.DbContext.SaveChanges();

            return true;
        }


        private long m_ExecutionCount = 0;
        private Timer m_Timer = null;
        private HashSet<int> m_TransferInProgress = null;

        private readonly IServiceProvider m_ServiceProvider;
        private readonly ILogger<NasDiskService> m_Logger;
    }

}

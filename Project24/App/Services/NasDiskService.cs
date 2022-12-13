/*  NasDiskService.cshtml
 *  Version: 1.1 (2022.12.13)
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
        public NasDiskService(IServiceProvider _serviceProvider, ILogger<NasDiskService> _logger)
        {
            m_ServiceProvider = _serviceProvider;
            m_Logger = _logger;
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

            string nasRootAbsPath = Path.GetFullPath(AppUtils.AppRoot + "/" + AppConfig.NasRoot);
            string nasCacheAbsPath = Path.GetFullPath(AppUtils.AppRoot + "/" + AppConfig.TmpRoot);

            using (var scope = m_ServiceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                if (dbContext.NasCachedFiles.Count() <= 0)
                    return;

                int processed = 0;
                DateTime start = DateTime.Now;
                long length = 0L;

                List<NasCachedFile> list = dbContext.NasCachedFiles.OrderBy(_file => _file.AddedDate).ToList();
                foreach (NasCachedFile file in list)
                {
                    string src = nasCacheAbsPath + "/" + file.Path + "/" + file.Name;
                    string dst = nasRootAbsPath + "/" + file.Path + "/" + file.Name;

                    if (!File.Exists(src))
                    {
                        string logStr = "NasDiskService cycle " + m_ExecutionCount + ":\r\n";
                        logStr += "File " + file.Path + "/" + file.Name + " doesn't exist anymore";
                        m_Logger.LogWarning(logStr);

                        dbContext.Remove(file);
                        continue;
                    }

                    try
                    {
                        File.Copy(src, dst, true);

                        if (File.Exists(dst))
                        {
                            FileInfo fi = new FileInfo(src);
                            length += fi.Length;
                            ++processed;

                            dbContext.Remove(file);

                            File.Delete(src);
                        }

                    }
                    catch (Exception _e)
                    {
                        string logStr = "NasDiskService cycle " + m_ExecutionCount + ":\r\n";
                        logStr += _e.ToString();

                        m_Logger.LogError(logStr);
                    }
                }

                TimeSpan elapsed = DateTime.Now - start;

                dbContext.SaveChanges();

                string log = "NasDiskService cycle " + m_ExecutionCount + "\r\n";
                log += "    Moved " + processed + " files (" + AppUtils.FormatDataSize(length) + ")";
                log += ", avg " + AppUtils.FormatDataSize((long)(length / elapsed.TotalSeconds)) + "/s";

                m_Logger.LogInformation(log);
            }







        }


        private long m_ExecutionCount = 0;
        private Timer m_Timer = null;

        private readonly IServiceProvider m_ServiceProvider;
        private readonly ILogger<NasDiskService> m_Logger;
    }

}

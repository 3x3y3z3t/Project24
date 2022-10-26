/*  NasDiskService.cshtml
 *  Version: 1.0 (2022.10.27)
 *
 *  Contributor
 *      Arime-chan
 */

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

            string nasRootAbsPath = Path.GetFullPath(Project24.Utils.AppRoot + "/" + AppConfig.NasRoot);
            string nasCacheAbsPath = Path.GetFullPath(Project24.Utils.AppRoot + "/" + AppConfig.TmpRoot);

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

                    if (File.Exists(src))
                    {
                        FileInfo fi = new FileInfo(src);
                        length += fi.Length;
                        ++processed;

                        File.Copy(src, dst, true);
                        if (File.Exists(dst))
                        {
                            File.Delete(src);
                        }
                    }

                    dbContext.Remove(file);
                }

                TimeSpan elapsed = DateTime.Now - start;

                dbContext.SaveChanges();

                string log = "NasDiskService cycle " + m_ExecutionCount + "\n";
                log += "    Moved " + processed + " files (" + Project24.Utils.FormatDataSize(length) + ")";
                log += ", avg " + Project24.Utils.FormatDataSize((long)(length / elapsed.TotalSeconds)) + "/s";

                m_Logger.LogInformation(log);
            }







        }


        private int m_ExecutionCount = 0;
        private Timer m_Timer = null;

        private readonly IServiceProvider m_ServiceProvider;
        private readonly ILogger<NasDiskService> m_Logger;
    }

}

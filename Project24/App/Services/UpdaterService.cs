/*  UpdaterService.cshtml
 *  Version: 1.0 (2022.11.15)
 *
 *  Contributor
 *      Arime-chan
 */

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Project24.App.Services
{
    public class UpdaterService : IHostedService, IDisposable
    {
        public bool IsUpdateInProgress { get; private set; } = false;
        public int PreparationTime { get; set; } = 10;
        public DateTime LaunchTime { get; private set; } = DateTime.MaxValue;
        public TimeSpan RemainingTime { get { return LaunchTime - DateTime.Now; } }


        public UpdaterService(IServiceProvider _serviceProvider, ILogger<NasDiskService> _logger)
        {
            m_ServiceProvider = _serviceProvider;
            m_Logger = _logger;

            UpdatePreparationTime();
        }

        public Task StartAsync(CancellationToken _cancellationToken)
        {
            m_Logger.LogInformation("UpdaterService started.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken _cancellationToken)
        {
            if (m_Timer != null)
            {
                m_Timer.Change(Timeout.Infinite, 0);
            }

            m_Logger.LogInformation("UpdaterService stopped.");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (m_Timer != null)
                m_Timer.Dispose();
        }

        public void Start()
        {
            if (IsUpdateInProgress)
                return;

            UpdatePreparationTime();

            LaunchTime = DateTime.Now.AddMinutes(PreparationTime);
            IsUpdateInProgress = true;

            m_Timer = new Timer(PerformUpdate, null, TimeSpan.FromMinutes(PreparationTime), TimeSpan.Zero);
            m_LoggerTimer = new Timer(LogProgress, null, TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.0));

            m_Logger.LogWarning("Version Up initialized. Version Up will commence in " + PreparationTime + " minutes (expected launch at " + LaunchTime.ToString() + ")");
        }

        public void Abort()
        {
            if (IsUpdateInProgress)
            {
                m_Logger.LogWarning("Version Up aborted.");
            }

            IsUpdateInProgress = false;
            LaunchTime = DateTime.MaxValue;

            if (m_Timer != null)
            {
                m_Timer.Change(Timeout.Infinite, 0);
                m_Timer.Dispose();
            }

            if (m_LoggerTimer != null)
            {
                m_LoggerTimer.Change(Timeout.Infinite, 0);
                m_LoggerTimer.Dispose();
            }
        }

        private void PerformUpdate(object? _state)
        {
            m_Logger.LogWarning("Timeout. Commencing update..");

            string nextAbsPath = DriveUtils.AppNextRootPath.Replace(" ", " ");
            string appAbsPath = DriveUtils.AppRootPath.Replace(" ", " ");

            Process proc = new Process();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.Arguments = $"/K \"\"{nextAbsPath}\\update.bat\" \"{nextAbsPath}\" \"{appAbsPath}\"";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.FileName = "/bin/bash";
                proc.StartInfo.Arguments = $"\"{nextAbsPath}/update.sh\" \"{nextAbsPath}\" \"{appAbsPath}\"";
            }
            else
            {
                // Unsupported platform, but IDC because I never deploy this app on this "platform".
            }

            proc.Start();

            IsUpdateInProgress = false;
            LaunchTime = DateTime.MaxValue;

            if (m_Timer != null)
            {
                m_Timer.Change(Timeout.Infinite, 0);
                m_Timer.Dispose();
            }

            if (m_LoggerTimer != null)
            {
                m_LoggerTimer.Change(Timeout.Infinite, 0);
                m_LoggerTimer.Dispose();
            }
        }

        private void LogProgress(object? _state)
        {
            if (!IsUpdateInProgress)
                return;

            if (RemainingTime.Minutes <= 0)
                return;

            m_Logger.LogWarning(RemainingTime.Minutes + " minutes left until update.");
        }

        private void UpdatePreparationTime()
        {
            using (var scope = m_ServiceProvider.CreateScope())
            {
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                PreparationTime = configuration.GetValue("Updater:PreparationTime", 10);
            }
        }

        private Timer m_Timer = null;
        private Timer m_LoggerTimer = null;


        private readonly IServiceProvider m_ServiceProvider;
        private readonly ILogger<NasDiskService> m_Logger;
    }

}

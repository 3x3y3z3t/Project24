/*  UpdaterService.cshtml
 *  Version: 1.2 (2022.12.18)
 *
 *  Contributor
 *      Arime-chan
 */

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Project24.App.Utils;
using System;
using System.Diagnostics;
using System.IO;
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


        public UpdaterService(IConfiguration _configuration, IHostApplicationLifetime _appLifetime, ILogger<UpdaterService> _logger)
        {
            m_Configuration = _configuration;
            m_ApplicationLifetime = _appLifetime;
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

        public void UpdateStaticFiles()
        {
            if (IsUpdateInProgress)
                return;

            string nextAbsPath = DriveUtils.AppNextRootPath;
            string appAbsPath = DriveUtils.AppRootPath;
            int processId = Process.GetCurrentProcess().Id;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string command = $"'{nextAbsPath}/update.sh' -quick {processId} '{appAbsPath}/wwwroot/' '{nextAbsPath}/wwwroot/' >> '{appAbsPath}/../updater_q.log'";
                PlatformUtils.ExecUnixCommand(command);
            }
        }

        private void PerformUpdate(object? _state)
        {
            m_Logger.LogWarning("Timeout. Commencing update..");

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

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                InvokeUpdatereOnLinux();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            { }
            else
            {
                // Unsupported platform, but IDC because I never deploy this app on this "platform".
                throw new PlatformNotSupportedException("Your platform is not supported.");
            }

            Task.Delay(2000).Wait(); // wait 2 seconds for the launcher to start up;
            m_Logger.LogInformation("Exiting app..");

            m_ApplicationLifetime.StopApplication();
        }

        private void LogProgress(object? _state)
        {
            if (!IsUpdateInProgress)
                return;

            if (RemainingTime.Minutes <= 0)
                return;

            m_Logger.LogWarning(RemainingTime.Minutes + " minutes left until update.");
        }

        private void InvokeUpdatereOnLinux()
        {
            m_Logger.LogInformation("Performing update on Linux..");

            string nextAbsPath = DriveUtils.AppNextRootPath;
            string appAbsPath = DriveUtils.AppRootPath;

            // create updater argument file;
            WriteArgumentsFile();

            // modify file permission;
            PlatformUtils.ExecUnixCommand($"chmod 777 \"{nextAbsPath}/Project24\"");
            PlatformUtils.ExecUnixCommand($"chmod 777 \"{nextAbsPath}/Updater\"");
            PlatformUtils.ExecUnixCommand($"chmod 777 \"{nextAbsPath}/update.sh\"");
            m_Logger.LogInformation("chmod command sent.");

            // call script to start updater service;
            PlatformUtils.ExecUnixCommand($"{nextAbsPath}/update.sh -launchUpdater");
            m_Logger.LogInformation("Launcher start command sent.");
        }

        private void UpdatePreparationTime()
        {
            PreparationTime = m_Configuration.GetValue("Updater:PreparationTime", 10);
        }

        private void WriteArgumentsFile()
        {
            string nextAbsPath = DriveUtils.AppNextRootPath;
            string appAbsPath = DriveUtils.AppRootPath;
            int processId = Process.GetCurrentProcess().Id;

            string filename = Path.GetFullPath(nextAbsPath + "/updater_args.cfg");

            StreamWriter writer = File.CreateText(filename);
            writer.WriteLine(processId);
            writer.WriteLine(appAbsPath);
            writer.WriteLine(nextAbsPath);

            writer.Close();

            m_Logger.LogInformation("Argument file written.");
        }


        private Timer m_Timer = null;
        private Timer m_LoggerTimer = null;

        private readonly IConfiguration m_Configuration;
        private readonly IHostApplicationLifetime m_ApplicationLifetime;
        private readonly ILogger<UpdaterService> m_Logger;
    }

}

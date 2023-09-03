/*  App/Services/UpdaterSvc.cs
 *  Version: v1.1 (2023.08.31)
 *  
 *  Author
 *      Arime-chan
 */

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using Project24.App.BackendData;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AppHelper;

namespace Project24.App.Services
{
    public sealed class UpdaterSvc : TrackableSvc
    {
        public UpdaterStatus Status
        {
            get => m_Status;
            set
            {
                m_Status = value;
                m_TrackerSvc[InternalTrackedKeys.STATE_UPDATER_STATUS] = value.ToString();
            }
        }

        public UpdaterQueuedAction QueuedAction
        {
            get => m_QueuedAction;
            set
            {
                m_QueuedAction = value;
                m_TrackerSvc[InternalTrackedKeys.STATE_UPDATER_QUEUED_ACTION] = value.ToString();
            }
        }
        public DateTime QueuedActionDueTime
        {
            get => m_QueuedActionDueTime;
            private set
            {
                m_QueuedActionDueTime = value;
                m_TrackerSvc[InternalTrackedKeys.STATE_UPDATER_QUEUED_ACTION_DUE_TIME] = value.ToString();
            }
        }
        public TimeSpan QueueActionRemainingTime { get { return QueuedActionDueTime - DateTime.Now; } }

        public string LastErrorMessage { get; private set; } = "";


        public UpdaterSvc(InternalTrackerSvc _trackerSvc, IServiceProvider _serviceProvider, FileSystemSvc _fileSystemSvc, ILogger<UpdaterSvc> _logger)
            : base(_trackerSvc, _serviceProvider, _logger)
        {
            m_FileSystemSvc = _fileSystemSvc;

            m_Timer = new Timer(Update, null, Timeout.Infinite, Timeout.Infinite);
        }


        public override void StartService()
        {
            m_Logger.LogDebug("AppSide = {_appSide}", Program.AppSide);

            // ==================================================;

            if (!Enum.TryParse(m_TrackerSvc[InternalTrackedKeys.STATE_UPDATER_STATUS], out m_Status))
                m_Status = UpdaterStatus.None;

            if (!Enum.TryParse(m_TrackerSvc[InternalTrackedKeys.STATE_UPDATER_QUEUED_ACTION], out m_QueuedAction))
                m_QueuedAction = UpdaterQueuedAction.None;

            if (!DateTime.TryParse(m_TrackerSvc[InternalTrackedKeys.STATE_UPDATER_QUEUED_ACTION_DUE_TIME], out m_QueuedActionDueTime))
                m_QueuedActionDueTime = DateTime.MaxValue;

            if (m_Status == UpdaterStatus.None && m_QueuedAction != UpdaterQueuedAction.None)
            {
                QueuedAction = UpdaterQueuedAction.None;
                SaveChangesAsync().Wait();
            }

            // ==================================================;

            if (Status == UpdaterStatus.PrevApplyQueued || Status == UpdaterStatus.NextApplyQueued
                || Status == UpdaterStatus.PrevPurgeQueued || Status == UpdaterStatus.NextPurgeQueued)
            {
                if (QueuedAction != UpdaterQueuedAction.Countdown)
                {
                    m_Logger.LogWarning("StartService(): State mismatch, all state will be reset (Status = {_status}, QueuedAction = {_queuedAction}).", Status.ToString(), QueuedAction.ToString());
                    Status = UpdaterStatus.None;
                    QueuedAction = UpdaterQueuedAction.None;
                    QueuedActionDueTime = DateTime.MaxValue;
                    return;
                }

                m_Timer.Change(0, 200);
                return;
            }

            if (Status == UpdaterStatus.PrevApplyRunning)
            {
#pragma warning disable CS4014
                ApplyPrevAsync();
#pragma warning restore CS4014
            }
            else if (Status == UpdaterStatus.NextApplyRunning)
            {
#pragma warning disable CS4014
                ApplyNextAsync();
#pragma warning restore CS4014
            }

        }

        public void StartMonitoringFileUpload(UpdaterMetadata _metadata)
        {
            InvokeSvc();

            m_Metadata = _metadata;
            for (int i = 0; i < _metadata.BatchesCount; ++i)
            {
                var batch = m_Metadata.BatchesMetadata[i];
                foreach (var pair in batch.FilesMetadata)
                {
                    m_Metadata.LastMods[pair.Key] = pair.Value.LastMod;
                }
            }
        }

        public void StopMonitoringFileUpload()
        {
            InvokeSvc();

            m_Metadata = null;
        }

        public void StartPurgePrev()
        {
            InvokeSvc();

            Status = UpdaterStatus.PrevPurgeQueued;
            QueuedAction = UpdaterQueuedAction.Countdown;
            ComputeUpdaterActionDueTime();
            SaveChangesAsync().Wait();

            m_Timer.Change(0, 200);
        }

        public void StartPurgeNext()
        {
            InvokeSvc();

            Status = UpdaterStatus.NextPurgeQueued;
            QueuedAction = UpdaterQueuedAction.Countdown;
            ComputeUpdaterActionDueTime();
            SaveChangesAsync().Wait();

            m_Timer.Change(0, 200);
        }

        public void Abort()
        {
            InvokeSvc();

            Status = UpdaterStatus.None;
            QueuedAction = UpdaterQueuedAction.None;
            QueuedActionDueTime = DateTime.MaxValue;
            SaveChangesAsync().Wait();
        }

        public void StartApplyPrev()
        {
            InvokeSvc();

            Status = UpdaterStatus.PrevApplyQueued;
            QueuedAction = UpdaterQueuedAction.Countdown;
            ComputeUpdaterActionDueTime();
            SaveChangesAsync().Wait();

            m_Timer.Change(0, 200);
        }

        public void StartApplyNext()
        {
            InvokeSvc();

            Status = UpdaterStatus.NextApplyQueued;
            QueuedAction = UpdaterQueuedAction.Countdown;
            ComputeUpdaterActionDueTime();
            SaveChangesAsync().Wait();

            m_Timer.Change(0, 200);
        }

        public void ClearErrorMessage() => LastErrorMessage = null;

        #region Helper Methods
        public int GetBatchesCount() => m_Metadata.BatchesCount;
        public int GetTotalFilesCount() => m_Metadata.FilesCount;
        public long GetTotalFilesSize() => m_Metadata.FilesSize;
        public int GetFilesCountInBatch(int _batchId) => m_Metadata.BatchesMetadata[_batchId].FilesCount;
        public long GetFilesSizeInBatch(int _batchId) => m_Metadata.BatchesMetadata[_batchId].FilesSize;
        public long GetFileLastMod(long _hashCode) => m_Metadata.LastMods[_hashCode];
        public bool IsFileLastModTracked(long _hashCode) => m_Metadata.LastMods.ContainsKey(_hashCode);
        public bool IsAllBatchesSuccess() { foreach (var batch in m_Metadata.BatchesMetadata) if (!batch.IsSuccess) return false; return true; }

        public void SetBatchStatus(int _batchId, bool _isSuccess) => m_Metadata.BatchesMetadata[_batchId].IsSuccess = _isSuccess;
        #endregion


        private async Task PurgePrevAsync()
        {
            InvokeSvc();

            m_Logger.LogInformation("Purge Prev started.");

            try
            {
                FileSystemSvc.DeleteFiles(m_FileSystemSvc.AppPrevRoot, s_ExcludeFilesList);
            }
            catch (Exception _ex)
            {
                m_Logger.LogError("PurgePrevAsync(): {_exception}", _ex);

                Status = UpdaterStatus.None;
                QueuedAction = UpdaterQueuedAction.None;
                await SaveChangesAsync();
                return;
            }

            Status = UpdaterStatus.None;
            QueuedAction = UpdaterQueuedAction.None;
            await SaveChangesAsync();

            m_Logger.LogInformation("Purge Prev done.");
        }

        private async Task PurgeNextAsync()
        {
            InvokeSvc();

            m_Logger.LogInformation("Purge Next started.");

            try
            {
                FileSystemSvc.DeleteFiles(m_FileSystemSvc.AppNextRoot, s_ExcludeFilesList);
            }
            catch (Exception _ex)
            {
                m_Logger.LogError("PurgeNextAsync(): {_exception}", _ex);

                Status = UpdaterStatus.None;
                QueuedAction = UpdaterQueuedAction.None;
                await SaveChangesAsync();
                return;
            }

            Status = UpdaterStatus.None;
            QueuedAction = UpdaterQueuedAction.None;
            await SaveChangesAsync();

            m_Logger.LogInformation("Purge Next done.");
        }

        private async Task ApplyPrevAsync()
        {
            InvokeSvc();

            if (QueuedAction == UpdaterQueuedAction.None)
            {
                m_Logger.LogWarning("ApplyPrevAsync(): QueuedAction is None, resetting all state.");
                LastErrorMessage += "<div>ApplyPrevAsync(): QueuedAction is <code>None</code>, resetting all state.</div>";

                Status = UpdaterStatus.None;
                await SaveChangesAsync();
                return;
            }

            /* Apply from Prev to Main process:
             *  1. Switch app to Prev
             *  2. Delete files in Main
             *  3. Copy files from Prev to Main
             *  4. Switch app back to Main
             *  5. Delete files in Prev.
             */

            m_Logger.LogInformation("Apply Prev started.");
            if (QueuedAction == UpdaterQueuedAction.SwitchExecutableToPrev)
            {
                m_Logger.LogDebug("  QueuedAction = {_action}", QueuedAction.ToString());
                if (Program.AppSide != AppSide_.PREV)
                {
                    SwitchAppExecutableToPrev();
                    return;
                }

                KillAppInstanceFromMain();

                QueuedAction = UpdaterQueuedAction.DeleteFilesInMain;
                await SaveChangesAsync();

                m_Logger.LogDebug("  QueuedAction = {_action} (saved)", QueuedAction.ToString());
            }

            if (QueuedAction == UpdaterQueuedAction.DeleteFilesInMain)
            {
                m_Logger.LogDebug("  QueuedAction = {_action}", QueuedAction.ToString());
                try
                {
                    FileSystemSvc.DeleteFiles(m_FileSystemSvc.AppMainRoot, s_ExcludeFilesList);

                    QueuedAction = UpdaterQueuedAction.CopyFilesFromPrevToMain;
                    await SaveChangesAsync();

                    m_Logger.LogDebug("  QueuedAction = {_action} (saved)", QueuedAction.ToString());
                }
                catch (Exception _ex)
                {
                    m_Logger.LogError("ApplyPrevAsync(): Exception during DeleteFilesInMain: {Exception}", _ex);
                    LastErrorMessage += "<div>ApplyPrevAsync(): Exception during DeleteFilesInMain:</div><div><pre>" + _ex + "</pre></div>";

                    QueuedAction = UpdaterQueuedAction.SwitchExecutableToMain;
                    await SaveChangesAsync();

                    m_Logger.LogDebug("  QueuedAction = {_action} (saved)", QueuedAction.ToString());
                }
            }

            if (QueuedAction == UpdaterQueuedAction.CopyFilesFromPrevToMain)
            {
                m_Logger.LogDebug("  QueuedAction = {_action}", QueuedAction.ToString());
                try
                {
                    FileSystemSvc.CopyFiles(m_FileSystemSvc.AppPrevRoot, m_FileSystemSvc.AppMainRoot, s_ExcludeFilesList);

                    QueuedAction = UpdaterQueuedAction.SwitchExecutableToMain;
                    await SaveChangesAsync();

                    m_Logger.LogDebug("  QueuedAction = {_action} (saved)", QueuedAction.ToString());
                }
                catch (Exception _ex)
                {
                    m_Logger.LogError("ApplyPrevAsync(): Exception during CopyFilesFromPrevToMain: {Exception}", _ex);
                    LastErrorMessage += "<div>ApplyPrevAsync(): Exception during CopyFilesFromPrevToMain:</div><div><pre>" + _ex + "</pre></div>";

                    QueuedAction = UpdaterQueuedAction.SwitchExecutableToMain;
                    await SaveChangesAsync();

                    m_Logger.LogDebug("  QueuedAction = {_action} (saved)", QueuedAction.ToString());
                }
            }

            if (QueuedAction == UpdaterQueuedAction.SwitchExecutableToMain)
            {
                m_Logger.LogDebug("  QueuedAction = {_action}", QueuedAction.ToString());
                if (Program.AppSide != AppSide_.MAIN)
                {
                    SwitchAppExecutableToMain();
                    return;
                }

                KillAppInstanceFromPrev();

                QueuedAction = UpdaterQueuedAction.None;
                Status = UpdaterStatus.None;
                await SaveChangesAsync();

                m_Logger.LogDebug("  QueuedAction = {_action} (saved)", QueuedAction.ToString());

                m_Logger.LogInformation("Apply Prev done.");
            }

            m_Logger.LogInformation("Apply Prev done.");
        }

        private async Task ApplyNextAsync()
        {
            InvokeSvc();

            if (QueuedAction == UpdaterQueuedAction.None)
            {
                m_Logger.LogWarning("ApplyNextAsync(): QueuedAction is None, resetting all state.");
                LastErrorMessage += "<div>ApplyNextAsync(): QueuedAction is <code>None</code>, resetting all state.</div>";

                Status = UpdaterStatus.None;
                await SaveChangesAsync();
                return;
            }

            /* Apply from Next to Main process:
             *  1. Delete files in Prev
             *  2. Copy files from Main to Prev
             *  3. Switch app to Prev
             *  4. Copy files from Next to Main
             *  5. Switch app back to Main
             */

            m_Logger.LogInformation("Apply Next started.");

            if (QueuedAction == UpdaterQueuedAction.DeleteFilesInPrev)
            {
                m_Logger.LogDebug("  QueuedAction = {_action}", QueuedAction.ToString());
                try
                {
                    FileSystemSvc.DeleteFiles(m_FileSystemSvc.AppPrevRoot, s_ExcludeFilesList);

                    QueuedAction = UpdaterQueuedAction.CopyFilesFromMainToPrev;
                    await SaveChangesAsync();

                    m_Logger.LogDebug("  QueuedAction = {_action} (saved)", QueuedAction.ToString());
                }
                catch (Exception _ex)
                {
                    m_Logger.LogError("ApplyNextAsync(): Exception during DeleteFilesInPrev: {Exception}", _ex);
                    LastErrorMessage += "<div>ApplyNextAsync(): Exception during DeleteFilesInPrev:</div><div><pre>" + _ex + "</pre></div>";

                    QueuedAction = UpdaterQueuedAction.None;
                    await SaveChangesAsync();

                    m_Logger.LogDebug("  QueuedAction = {_action} (saved)", QueuedAction.ToString());
                    return;
                }
            }

            if (QueuedAction == UpdaterQueuedAction.CopyFilesFromMainToPrev)
            {
                m_Logger.LogDebug("  QueuedAction = {_action}", QueuedAction.ToString());
                try
                {
                    FileSystemSvc.CopyFiles(m_FileSystemSvc.AppMainRoot, m_FileSystemSvc.AppPrevRoot, s_ExcludeFilesList);

                    SystemCaller.ExecUnixCommand("sudo chmod 777 " + m_FileSystemSvc.AppPrevRoot + "/AppHelper");
                    SystemCaller.ExecUnixCommand("sudo chmod 777 " + m_FileSystemSvc.AppPrevRoot + "/Project24");

                    QueuedAction = UpdaterQueuedAction.SwitchExecutableToPrev;
                    await SaveChangesAsync();

                    m_Logger.LogDebug("  QueuedAction = {_action} (saved)", QueuedAction.ToString());
                }
                catch (Exception _ex)
                {
                    m_Logger.LogError("ApplyNextAsync(): Exception during CopyFilesFromMainToPrev: {Exception}", _ex);
                    LastErrorMessage += "<div>ApplyNextAsync(): Exception during CopyFilesFromMainToPrev:</div><div><pre>" + _ex + "</pre></div>";

                    QueuedAction = UpdaterQueuedAction.None;
                    await SaveChangesAsync();

                    m_Logger.LogDebug("  QueuedAction = {_action} (saved)", QueuedAction.ToString());
                    return;
                }
            }

            if (QueuedAction == UpdaterQueuedAction.SwitchExecutableToPrev)
            {
                m_Logger.LogDebug("  QueuedAction = {_action}", QueuedAction.ToString());
                if (Program.AppSide != AppSide_.PREV)
                {
                    SwitchAppExecutableToPrev();
                    return;
                }

                KillAppInstanceFromMain();

                QueuedAction = UpdaterQueuedAction.CopyFilesFromNextToMain;
                await SaveChangesAsync();

                m_Logger.LogDebug("  QueuedAction = {_action} (saved)", QueuedAction.ToString());
            }

            if (QueuedAction == UpdaterQueuedAction.CopyFilesFromNextToMain)
            {
                m_Logger.LogDebug("  QueuedAction = {_action}", QueuedAction.ToString());
                try
                {
                    FileSystemSvc.CopyFiles(m_FileSystemSvc.AppNextRoot, m_FileSystemSvc.AppMainRoot, s_ExcludeFilesList);

                    SystemCaller.ExecUnixCommand("sudo chmod 777 " + m_FileSystemSvc.AppMainRoot + "/AppHelper");
                    SystemCaller.ExecUnixCommand("sudo chmod 777 " + m_FileSystemSvc.AppMainRoot + "/Project24");
                }
                catch (Exception _ex)
                {
                    m_Logger.LogError("ApplyNextAsync(): Exception during CopyFilesFromNextToMain: {Exception}", _ex);
                    LastErrorMessage += "<div>ApplyNextAsync(): Exception during CopyFilesFromNextToMain:</div><div><pre>" + _ex + "</pre></div>";
                }

                QueuedAction = UpdaterQueuedAction.SwitchExecutableToMain;
                await SaveChangesAsync();

                m_Logger.LogDebug("  QueuedAction = {_action} (saved)", QueuedAction.ToString());
            }

            if (QueuedAction == UpdaterQueuedAction.SwitchExecutableToMain)
            {
                m_Logger.LogDebug("  QueuedAction = {_action}", QueuedAction.ToString());
                if (Program.AppSide != AppSide_.MAIN)
                {
                    SwitchAppExecutableToMain();
                    return;
                }

                KillAppInstanceFromPrev();

                QueuedAction = UpdaterQueuedAction.None;
                Status = UpdaterStatus.None;
                await SaveChangesAsync();

                m_Logger.LogDebug("  QueuedAction = {_action} (saved)", QueuedAction.ToString());

                m_Logger.LogInformation("Apply Prev done.");
            }

            m_Logger.LogInformation("Apply Prev done.");
        }

        private void SwitchAppExecutableToPrev()
        {
            m_Logger.LogInformation("  Switching App to Prev (launching Prev)..");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                m_Logger.LogError("    Window is not supported yet.");
                return;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                OSUtils.Unix_EnableApp(AppSide_.PREV, true);
                return;
            }

            m_Logger.LogError("    Platform is not supported.");
        }

        private void SwitchAppExecutableToMain()
        {
            m_Logger.LogInformation("  Switching App to Main (launching Main)..");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                m_Logger.LogError("    Window is not supported yet.");
                return;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                OSUtils.Unix_EnableApp(AppSide_.MAIN, true);
                return;
            }

            m_Logger.LogError("    Platform is not supported.");
        }

        private void KillAppInstanceFromMain()
        {
            m_Logger.LogInformation("  Killing App Instance from Main..");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                m_Logger.LogError("    Window is not supported yet.");
                return;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                OSUtils.Unix_DisableApp(AppSide_.MAIN, true);
                return;
            }

            m_Logger.LogError("    Platform is not supported.");
        }

        private void KillAppInstanceFromPrev()
        {
            m_Logger.LogInformation("  Killing App Instance from Prev..");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                m_Logger.LogError("    Window is not supported yet.");
                return;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                OSUtils.Unix_DisableApp(AppSide_.PREV, true);
                return;
            }

            m_Logger.LogError("    Platform is not supported.");
        }

        private void Update(object? _state)
        {
            // put svc to sleep if needed;
            if (Environment.TickCount64 - m_LastActivitiesTick > c_IdleTimeMillis)
                m_Timer.Change(0, Timeout.Infinite);

            if (QueuedActionDueTime == DateTime.MaxValue || QueueActionRemainingTime.Ticks > 0)
            {
                if (QueuedAction == UpdaterQueuedAction.None && Status != UpdaterStatus.None)
                {
                    m_Logger.LogWarning("UpdaterSvc.Update(): QueuedAction is None but Status is {_status}, resetting all state.", Status);
                    LastErrorMessage += "<div>UpdaterSvc.Update(): QueuedAction is <code>None</code>, Status is <code>" + Status + "</code>, resetting all state.</div>";

                    Status = UpdaterStatus.None;
                    SaveChangesAsync().Wait();
                }
                return;
            }

            switch (Status)
            {
                case UpdaterStatus.PrevPurgeQueued:
                {
                    Status = UpdaterStatus.PrevPurgeRunning;
                    QueuedAction = UpdaterQueuedAction.DeleteFilesInPrev;
                    QueuedActionDueTime = DateTime.MaxValue;
                    SaveChangesAsync().Wait();

                    PurgePrevAsync().Wait();
                    return;
                }
                case UpdaterStatus.NextPurgeQueued:
                {
                    Status = UpdaterStatus.NextPurgeRunning;
                    QueuedAction = UpdaterQueuedAction.DeleteFilesInNext;
                    QueuedActionDueTime = DateTime.MaxValue;
                    SaveChangesAsync().Wait();

                    PurgeNextAsync().Wait();
                    return;
                }
                case UpdaterStatus.PrevApplyQueued:
                {
                    Status = UpdaterStatus.PrevApplyRunning;
                    QueuedAction = UpdaterQueuedAction.SwitchExecutableToPrev;
                    QueuedActionDueTime = DateTime.MaxValue;
                    SaveChangesAsync().Wait();

                    ApplyPrevAsync().Wait();
                    return;
                }
                case UpdaterStatus.NextApplyQueued:
                {
                    Status = UpdaterStatus.NextApplyRunning;
                    QueuedAction = UpdaterQueuedAction.DeleteFilesInPrev;
                    QueuedActionDueTime = DateTime.MaxValue;
                    SaveChangesAsync().Wait();

                    ApplyNextAsync().Wait();
                    return;
                }
                case UpdaterStatus.PrevPurgeRunning:
                    HandleInvalidStatusCaseRunning();
                    return;
                case UpdaterStatus.NextPurgeRunning:
                    HandleInvalidStatusCaseRunning();
                    return;
                case UpdaterStatus.PrevApplyRunning:
                    HandleInvalidStatusCaseRunning();
                    return;
                case UpdaterStatus.NextApplyRunning:
                    HandleInvalidStatusCaseRunning();
                    return;
                case UpdaterStatus.None:
                    HandleInvalidStatusCaseNone();
                    return;
            }

            HandleInvalidStatus();

        }

        private void HandleInvalidStatus()
        {
            LastErrorMessage += "<div>Invalid Status: <code>" + Status.ToString() + "</code> (DT = <code>" + QueuedActionDueTime + "</code>)</div>";

            Status = UpdaterStatus.None;
            QueuedActionDueTime = DateTime.MaxValue;
        }

        private void HandleInvalidStatusCaseNone()
        {
            LastErrorMessage += "<div>Invalid Status: <code>" + Status.ToString() + "</code> (None) (DT = <code>" + QueuedActionDueTime + "</code>)</div>";

            Status = UpdaterStatus.None;
            QueuedActionDueTime = DateTime.MaxValue;

            // TODO: status should not be None, log this;
        }

        private void HandleInvalidStatusCaseRunning()
        {
            LastErrorMessage += "<div>Invalid Status: <code>" + Status.ToString() + "</code> (Running) (DT = <code>" + QueuedActionDueTime + "</code>)</div>";

            Status = UpdaterStatus.None;
            QueuedActionDueTime = DateTime.MaxValue;

            // TODO: status should not be -Running, log this;
        }

        //private async Task HandleStateMismatchError(string _callerName)
        //{
        //    m_Logger.LogWarning(_callerName + ": State mismatch, all state will be reset (Status = " + Status.ToString() + ", InternalState = " + InternalState.ToString() + ").");
        //    Status = UpdaterStatus.None;
        //    //InternalState = UpdaterInternalState.None;
        //    QueuedActionDueTime = DateTime.MaxValue;

        //    await SaveChangesAsync();
        //}

        private void ComputeUpdaterActionDueTime() => QueuedActionDueTime = DateTime.Now.AddMinutes(int.Parse(m_TrackerSvc[InternalTrackedKeys.CONFIG_UPDATER_WAIT_TIME]));
        private void InvokeSvc() => m_LastActivitiesTick = Environment.TickCount64;


        // TODO: move this to AppConfigSvc;
        //public const int c_ActionDueTimeMillis = 2 * 60 * 1000;
        private const int c_IdleTimeMillis = 5 * 60 * 1000; // 5 minutes idle time;

        private static readonly List<string> s_ExcludeFilesList = new() { "appsettings.Production.json" };

        private UpdaterMetadata m_Metadata = null;

        private UpdaterStatus m_Status = UpdaterStatus.None;
        private UpdaterQueuedAction m_QueuedAction = UpdaterQueuedAction.None;
        private DateTime m_QueuedActionDueTime = DateTime.MaxValue;
        private long m_LastActivitiesTick = 0;
        private readonly Timer m_Timer = null;

        //private string m_AppSide = null;

        private readonly FileSystemSvc m_FileSystemSvc;
    }

}
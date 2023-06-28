/*  App/Services/WatchdogSvc.cs
 *  Version: v1.0 (2023.06.16)
 *  
 *  Contributor
 *      Arime-chan
 */

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Project24.Data;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.Extensions.DependencyInjection;
using Project24.App.Watchable;
using System.Linq;
using Project24.App.BackendData;

namespace Project24.App.Services
{
    public sealed class UpdaterSvc
    {
        public UpdaterStatus Status { get; private set; }


        public UpdaterSvc(IServiceProvider _serviceProvider, ILogger<WatchdogSvc> _logger)
        {
            m_ServiceProvider = _serviceProvider;
            m_Logger = _logger;

            m_Watchables = new Dictionary<Type, WatchableBase>();

            m_Timer = new Timer(Update, null, 0, 500);
        }


        public void StartMonitoringFileUpload(UpdaterMetadata _metadata)
        {
            m_LastActivitiesTick = Environment.TickCount;

            m_Metadata = _metadata;
            for (int i = 0; i < _metadata.BatchesCount; ++i)
            {
                var batch = m_Metadata.BatchesMetadata[i];
                foreach (var pair in batch.FilesMetadata)
                {
                    m_Metadata.LastMods[pair.Key] = pair.Value.LastMod;
                }
            }

            // TODO: set some flag;



        }

        public void StartPurgePrev()
        {
            m_LastActivitiesTick = Environment.TickCount;

            Status = UpdaterStatus.PrevPurgeQueued;
            PurgePrevAsync();
        }

        public void StartPurgeNext()
        {
            m_LastActivitiesTick = Environment.TickCount;

            Status = UpdaterStatus.NextPurgeQueued;
            PurgeNextAsync();
        }

        public void Abort()
        {
            m_LastActivitiesTick = Environment.TickCount;

            Status = UpdaterStatus.None;
        }

        public void StartApplyPrev()
        {
            m_LastActivitiesTick = Environment.TickCount;

            Status = UpdaterStatus.PrevApplyQueued;
        }

        public void StartApplyNext()
        {
            m_LastActivitiesTick = Environment.TickCount;

            Status = UpdaterStatus.NextApplyQueued;
        }






        private async void PurgePrevAsync()
        {
            Status = UpdaterStatus.PrevPurgeRunning;

            // TODO: purge files;



            Status = UpdaterStatus.None;
        }
        private async void PurgeNextAsync()
        {
            Status = UpdaterStatus.NextPurgeRunning;

            // TODO: purge files;



            Status = UpdaterStatus.None;
        }






        public void StopSvc(bool _sleepNow = false)
        {

            if (_sleepNow)
            {
                PutSvcToSleep();
            }

        }

        #region Helper Methods
        public int GetBatchesCount() => m_Metadata.BatchesCount;
        public int GetTotalFilesCount() => m_Metadata.FilesCount;
        public long GetTotalFilesSize() => m_Metadata.FilesSize;
        public int GetFilesCountInBatch(int _batchId) => m_Metadata.BatchesMetadata[_batchId].FilesCount;
        public long GetFilesSizeInBatch(int _batchId) => m_Metadata.BatchesMetadata[_batchId].FilesSize;
        public long GetFileLastMod(long _hashCode) => m_Metadata.LastMods[_hashCode];
        public bool IsFileLastModTracked(long _hashCode) => m_Metadata.LastMods.ContainsKey(_hashCode);
        #endregion

        private void Update(object? _state)
        {


            if (Environment.TickCount - m_LastActivitiesTick >= c_IdleTimeMillis)
            {
                PutSvcToSleep();
            }





            using (var scope = m_ServiceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // TODO: do DB thingy;




            }
            }

        private void PutSvcToSleep(ApplicationDbContext _dbContext = null)
        {
            m_Timer.Change(0, Timeout.Infinite);




        }


        private const int c_IdleTimeMillis = 5 * 60 * 1000; // 5 minutes idle time;

        private UpdaterMetadata m_Metadata = null;

        private Dictionary<Type, WatchableBase> m_Watchables = null;
        private int m_LastActivitiesTick = 0;

        private Timer m_Timer = null;

        private readonly IServiceProvider m_ServiceProvider;
        private readonly ILogger<WatchdogSvc> m_Logger;
    }

}

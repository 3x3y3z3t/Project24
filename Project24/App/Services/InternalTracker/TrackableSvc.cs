/*  App/Services/TrackableService.cs
 *  Version: v1.0 (2023.08.11)
 *  
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Project24.Data;

namespace Project24.App.Services
{
    public abstract class TrackableSvc
    {
        protected TrackableSvc(InternalTrackerSvc _trackerSvc, IServiceProvider _serviceProvider, ILogger<TrackableSvc> _logger)
        {
            m_TrackerSvc = _trackerSvc;
            m_ServiceProvider = _serviceProvider;
            m_Logger = _logger;
        }


        public abstract void StartService();

        protected async Task<int> SaveChangesAsync(CancellationToken _cancellationToken = default)
        {
            return await m_TrackerSvc.SaveChangesAsync(_cancellationToken);

            //if (m_TrackedValue.Count <= 0)
            //    return 0;

            //using var scope = m_ServiceProvider.CreateScope();
            //var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            //return await SaveChangesAsync(dbContext, _cancellationToken);
        }

        protected async Task<int> SaveChangesAsync(ApplicationDbContext _dbContext, CancellationToken _cancellationToken = default)
        {
            return await m_TrackerSvc.SaveChangesAsync(_dbContext, _cancellationToken);

            //if (_dbContext == null)
            //    return 0;
            //if (m_TrackedValue.Count <= 0)
            //    return 0;

            //List<InternalState> trackedStates = new();
            //foreach (var pair in m_TrackedValue)
            //{
            //    trackedStates.Add(new(pair.Key, pair.Value));
            //}

            //_dbContext.UpdateRange(trackedStates);
            //int changesCount =  await _dbContext.SaveChangesAsync(_cancellationToken);

            //return changesCount;
        }


        //protected Dictionary<string, string> m_TrackedValue = new();

        protected readonly InternalTrackerSvc m_TrackerSvc;
        protected readonly IServiceProvider m_ServiceProvider;
        protected readonly ILogger<TrackableSvc> m_Logger;
    }

}

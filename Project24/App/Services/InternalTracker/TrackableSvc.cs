/*  App/Services/InternalTracker/TrackableService.cs
 *  Version: v1.1 (2023.08.31)
 *  
 *  Author
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
            => await m_TrackerSvc.SaveChangesAsync(_cancellationToken);

        protected async Task<int> SaveChangesAsync(ApplicationDbContext _dbContext, CancellationToken _cancellationToken = default)
            => await m_TrackerSvc.SaveChangesAsync(_dbContext, _cancellationToken);


        protected readonly InternalTrackerSvc m_TrackerSvc;
        protected readonly IServiceProvider m_ServiceProvider;
        protected readonly ILogger<TrackableSvc> m_Logger;
    }

}

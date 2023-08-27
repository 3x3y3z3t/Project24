/*  App/Services/DbMaintenanceSvc.cs
 *  Version: v1.0 (2023.08.10)
 *  
 *  Contributor
 *      Arime-chan
 */

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Project24.Data;
using Project24.Model.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Project24.App.Services
{
    public class DBMaintenanceSvc
    {
        public DBMaintenanceSvc(IServiceProvider _serviceProvider, ILogger<DBMaintenanceSvc> _logger)
        {
            m_ServiceProvider = _serviceProvider;
            m_Logger = _logger;

            m_Timer = new Timer(DoWork, null, Timeout.Infinite, Timeout.Infinite);
        }


        public void StartService()
        {
            DoWork(null);
            m_Timer.Change(c_CleanupInterval, c_CleanupInterval);
        }

        private void DoWork(object? _state)
        {
            if (m_CleanupInProgress)
                return;

            m_Logger.LogInformation("Cleaning up DB..");

            lock (this)
            {
                m_CleanupInProgress = true;
            }

            // ==================================================;

            using var scope = m_ServiceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            bool shouldSave = false;
            shouldSave |= CleanupInternalStates(dbContext);

            if (shouldSave)
                dbContext.SaveChanges();

            // ==================================================;

            lock (this)
            {
                m_CleanupInProgress = false;
            }

            m_Logger.LogInformation("DB cleanup done.");
        }

        private bool CleanupInternalStates(ApplicationDbContext _dbContext)
        {
            m_Logger.LogInformation("Cleaning up `InternalStates`..");

            var removeListsList = (from _state in _dbContext.InternalStates
                                   join _dupe in (from _state in _dbContext.InternalStates
                                                  group _state by _state.Key into _group
                                                  select new { _group.Key, Count = _group.Count() })
                                                  on _state.Key equals _dupe.Key
                                   where _dupe.Count > 1
                                   group _state by _state.Key into _grState
                                   select new List<InternalState>(_grState.OrderByDescending(_x => _x.Id).Skip(1)))
                                  .ToList();

            return CleanupEntities(_dbContext, removeListsList);
        }

        private static bool CleanupEntities<T>(ApplicationDbContext _dbContext, List<List<T>> _removeListsList) where T : class
        {
            if (_removeListsList.Count <= 0)
                return false;

            List<T> removeList = new();
            foreach (List<T> value in _removeListsList)
            {
                removeList.AddRange(value);
            }

            if (removeList.Count <= 0)
                return false;

            _dbContext.RemoveRange(removeList);

            return true;
        }


        private const uint c_CleanupInterval = 24 * 60 * 60 * 1000; // cleanup will be performed every 24 hours;

        private bool m_CleanupInProgress = false;

        private readonly Timer m_Timer;

        protected readonly IServiceProvider m_ServiceProvider;
        protected readonly ILogger<DBMaintenanceSvc> m_Logger;
    }

}

/*  App/Services/InternalTrackerSvc.cs
 *  Version: v1.0 (2023.08.27)
 *  
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Project24.Data;
using Project24.Model.Home;

namespace Project24.App.Services
{
    public sealed class InternalTrackerSvc
    {
        public string this[string _key]
        {
            get
            {
                if (!m_TrackedValues.ContainsKey(_key))
                {
                    m_TrackedValues[_key] = "";
                    m_ChangedValues[_key] = "";
                }

                return m_TrackedValues[_key];
            }

            set
            {
                if (m_TrackedValues.ContainsKey(_key) && m_TrackedValues[_key] != value)
                {
                    m_ChangedValues[_key] = value;
                }

                m_TrackedValues[_key] = value;
            }
        }


        public InternalTrackerSvc(IServiceProvider _serviceProvider)
        {
            m_ServiceProvider = _serviceProvider;
        }


        public void StartService()
        {
            using var scope = m_ServiceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var values = (from _state in dbContext.InternalStates
                          group _state by _state.Key into _grStates
                          select new { _grStates.Key, _grStates.OrderByDescending(_x => _x.Id).Last().Value })
                         .ToDictionary(_x => _x.Key, _x => _x.Value);

            foreach (var pair in values)
                m_TrackedValues[pair.Key] = pair.Value;
        }

        public async Task<int> SaveChangesAsync(CancellationToken _cancellationToken = default)
        {
            if (m_ChangedValues.Count <= 0)
                return 0;

            using var scope = m_ServiceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return await SaveChangesAsync(dbContext, _cancellationToken);
        }

        public async Task<int> SaveChangesAsync(ApplicationDbContext _dbContext, CancellationToken _cancellationToken = default)
        {
            if (_dbContext == null)
                return 0;
            if (m_ChangedValues.Count <= 0)
                return 0;

            List<InternalState> trackedStates = new();
            foreach (var pair in m_ChangedValues)
            {
                trackedStates.Add(new(pair.Key, pair.Value));
            }

            _dbContext.UpdateRange(trackedStates);
            int changesCount = await _dbContext.SaveChangesAsync(_cancellationToken);

            m_ChangedValues.Clear();

            return changesCount;
        }


        private readonly Dictionary<string, string> m_TrackedValues = new();
        private readonly Dictionary<string, string> m_ChangedValues = new();

        private readonly IServiceProvider m_ServiceProvider;
    }

}

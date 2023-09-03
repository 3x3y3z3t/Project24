/*  App/Services/InternalTracker/InternalTrackerSvc.cs
 *  Version: v1.1 (2023.09.02)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Project24.Data;
using Project24.Model.Home;

namespace Project24.App.Services
{
    public sealed partial class InternalTrackerSvc
    {
        public string this[string _key]
        {
            get
            {
                if (!m_TrackedValues.ContainsKey(_key))
                {
                    m_TrackedValues[_key] = "";
                    m_AddedValues[_key] = "";
                }

                return m_TrackedValues[_key];
            }

            set
            {
                if (!m_TrackedValues.ContainsKey(_key))
                {
                    m_AddedValues[_key] = value;
                }
                else if (m_TrackedValues[_key] != value)
                {
                    if (m_AddedValues.ContainsKey(_key))
                    {
                        m_AddedValues[_key] = value;
                    }
                    else
                    {
                        m_ChangedValues[_key] = value;
                    }
                }

                m_TrackedValues[_key] = value;
            }
        }

        public Dictionary<string, string> TrackedValues { get { return new(m_TrackedValues); } }


        public InternalTrackerSvc(IServiceProvider _serviceProvider)
        {
            m_ServiceProvider = _serviceProvider;
        }


        public void StartService()
        {
            using var scope = m_ServiceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            bool hasNewTrackables = InitializeTrackables(dbContext);
            bool hasNewMetadatas = InitializeTrackableMetadatas(dbContext);

            if (hasNewTrackables || hasNewMetadatas)
                SaveChangesAsync(dbContext).Wait();
        }

        public async Task<int> SaveChangesAsync(CancellationToken _cancellationToken = default)
        {
            if (!IsDirty())
                return 0;

            using var scope = m_ServiceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return await SaveChangesAsync(dbContext, _cancellationToken);
        }

        /// <summary>
        /// NOTE: User is responsible to check if there is a need to invoke database save.
        /// </summary>
        /// <param name="_dbContext"></param>
        /// <param name="_cancellationToken"></param>
        /// <returns></returns>
        public async Task<int> SaveChangesAsync(ApplicationDbContext _dbContext, CancellationToken _cancellationToken = default)
        {
            if (_dbContext == null)
                return 0;

            if (m_AddedValues.Count > 0)
            {
                List<Trackable> addedValue = new();
                lock (this)
                {
                    foreach (var pair in m_AddedValues)
                        addedValue.Add(new(pair.Key, pair.Value));

                    m_ChangedValues.Clear();
                }
                _dbContext.AddRange(addedValue);
            }

            if (m_ChangedValues.Count > 0)
            {
                List<Trackable> changedValue = new();
                lock (this)
                {
                    foreach (var pair in m_ChangedValues)
                        changedValue.Add(new(pair.Key, pair.Value));

                    m_AddedValues.Clear();
                }
                _dbContext.UpdateRange(changedValue);
            }

            int changesCount = await _dbContext.SaveChangesAsync(_cancellationToken);

            return changesCount;
        }

        public bool ContainsKey(string _key) => m_TrackedValues.ContainsKey(_key);


        private bool IsDirty() => m_ChangedValues.Count > 0 || m_AddedValues.Count > 0;


        private readonly Dictionary<string, string> m_TrackedValues = new();
        private readonly Dictionary<string, string> m_ChangedValues = new();
        private readonly Dictionary<string, string> m_AddedValues = new();

        private readonly IServiceProvider m_ServiceProvider;
    }

}

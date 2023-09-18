/*  App/Services/ServerAnnouncement/ServerAnnouncementSvc.cs
 *  Version: v1.1 (2023.09.19)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Project24.App.Services.ServerAnnouncement;
using Project24.Data;
using Project24.Model;

namespace Project24.App.Services
{
    public class ServerAnnouncementSvc : IProject24HostedService
    {
        public ServerAnnouncementSvc(LocalizationSvc _localizationSvc, IServiceProvider _serviceProvider, ILogger<ServerAnnouncementSvc> _logger)
        {
            m_LocalizationSvc = _localizationSvc;
            m_ServiceProvider = _serviceProvider;
            m_Logger = _logger;

            m_Announcements = new();

            m_AddedAnnouncements = new();
            m_ExpiredAnnouncements = new();

            m_Timer = new(Update);
        }


        public async Task StartAsync(CancellationToken _cancellationToken = default)
        {
            using (var scope = m_ServiceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var announcements = (from _announcement in dbContext.Announcements
                                     select _announcement)
                                    .ToList();

                m_Announcements.AddRange(announcements);

                foreach (var announcement in m_Announcements)
                {
                    announcement.DeserializeArgsDbString();
                }





                // ==================================================;
                #region Add Dummy Announcement
                {
                    bool shouldAdd = true;
                    foreach (var item in announcements)
                    {
                        if (item.Tag == "ANNOUNCEMENT_TEST")
                        {
                            shouldAdd = false;
                            break;
                        }
                    }

                    if (shouldAdd)
                    {
                        AnnouncementArgData[] args = new AnnouncementArgData[]
                        {
                            new AnnouncementArgDataString("Server Time"),
                            new AnnouncementArgDataDateTime(DateTime.Now, _infiniteCount: true, _shouldUpdate: true)
                        };

                        Announcement announcement = new("{0}: {1:yyyy/MM/dd HH:mm:ss} <<", args)
                        {
                            Tag = "ANNOUNCEMENT_TEST"
                        };

                        Add(announcement);
                        await SaveChangesAsync(_cancellationToken);
                    }
                }
                #endregion
                // ==================================================;


            }

            InitialUpdate();

            m_LastUpdateTick = Environment.TickCount64;
            m_Timer.Change(0, c_UpdateIntervalMillis);
        }

        public void Add(Announcement _announcememt)
        {
            lock (m_Lock)
            {
                m_Announcements.Add(_announcememt);
                m_AddedAnnouncements.Add(_announcememt);
            }
        }

        public string GetAllJsonSerializedAnnouncements()
        {
            string jsonData;
            lock (m_Lock)
            {
                foreach (Announcement announcement in m_Announcements)
                {
                    announcement.ResolveFormatString(m_LocalizationSvc);
                }
            }
            jsonData = JsonSerializer.Serialize(m_Announcements);

            return jsonData;
        }
        
        //public bool ForceExpireAnnouncement(long _id)
        //{
        //    lock (m_Lock)
        //    {
        //        foreach (Announcement announcement in m_Announcements)
        //        {
        //            if (announcement.Id == _id)
        //            {
        //                announcement.ForceExpire();
        //                return true;
        //            }
        //        }
        //    }

        //    return false;
        //}

        public bool ForceExpireAnnouncement(string _tag)
        {
            lock (m_Lock)
            {
                foreach (Announcement announcement in m_Announcements)
                {
                    if (announcement.Tag == _tag)
                    {
                        announcement.ForceExpire();
                        return true;
                    }
                }
            }

            return false;
        }

        //public bool ContainsKey(long _key) => m_Announcements.ContainsKey(_key);
        public bool ContainsTag(string _tag)
        {
            lock (m_Lock)
            {
                foreach (Announcement announcement in m_Announcements)
                {
                    if (announcement.Tag == _tag)
                        return true;
                }
            }

            return false;
        }


        private async Task<int> SaveChangesAsync(CancellationToken _cancellationToken = default)
        {
            using var scope = m_ServiceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return await SaveChangesAsync(dbContext, _cancellationToken);
        }

        public async Task<int> SaveChangesAsync(ApplicationDbContext _dbContext, CancellationToken _cancellationToken = default)
        {
            bool shouldSave = false;

            lock (m_Lock)
            {
                if (m_AddedAnnouncements.Count > 0)
                {
                    _dbContext.AddRange(m_AddedAnnouncements);
                    m_AddedAnnouncements.Clear();
                    shouldSave = true;
                }
            }

            lock (m_Lock)
            {
                if (m_ExpiredAnnouncements.Count > 0)
                {
                    _dbContext.RemoveRange(m_ExpiredAnnouncements);
                    m_ExpiredAnnouncements.Clear();
                    shouldSave = true;
                }
            }

            if (shouldSave)
            {
                return await _dbContext.SaveChangesAsync(_cancellationToken);
            }

            return 0;
        }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        private void Update(object? _state)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        {
            long millis;

            lock (this)
            {
                if (m_IsUpdating)
                    return;

                m_IsUpdating = true;
                millis = Environment.TickCount64 - m_LastUpdateTick;
                m_LastUpdateTick = Environment.TickCount64;

                TimeSpan ts = new(millis * TimeSpan.TicksPerMillisecond);

                for (int i = 0; i < m_Announcements.Count; ++i)
                {
                    Announcement announcement = m_Announcements[i];
                    if (announcement.IsExpired)
                    {
                        m_ExpiredAnnouncements.Add(announcement);
                        m_Announcements.RemoveAt(i);
                        --i;
                    }
                    else
                    {
                        announcement.Update(ts);
                    }
                }
            }

            if (m_ExpiredAnnouncements.Count > 0)
            {
                Task.Run(async () => await SaveChangesAsync());
            }

            lock (this)
            {
                m_IsUpdating = false;
            }
        }

        private void InitialUpdate()
        {
            DateTime now = DateTime.Now;

            foreach (Announcement announcement in m_Announcements)
            {
                TimeSpan deltaTime = now - announcement.AddedDate;
                announcement.Update(deltaTime);
            }
        }


        private const int c_UpdateIntervalMillis = 250;


        private readonly List<Announcement> m_Announcements;
        private readonly List<Announcement> m_AddedAnnouncements;
        private readonly List<Announcement> m_ExpiredAnnouncements;


        private bool m_IsUpdating = false;
        private long m_LastUpdateTick = 0;

        private readonly Timer m_Timer = null;

        private readonly object m_Lock = new();

        private readonly LocalizationSvc m_LocalizationSvc;
        private readonly IServiceProvider m_ServiceProvider;
        private readonly ILogger<ServerAnnouncementSvc> m_Logger;
    }

}

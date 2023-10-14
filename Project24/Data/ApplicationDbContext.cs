/*  Data/ApplicationDbContext.cs
 *  Version: v1.6 (2023.10.06)
 *  
 *  Author
 *      Arime-chan
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Project24.Model;
using Project24.Model.Home;
using Project24.Model.Identity;
using Project24.Model.Simulator.FinancialManagement;

namespace Project24.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<P24IdentityUser> P24Users { get; set; }
        public DbSet<P24IdentityRole> P24Roles { get; set; }





        public DbSet<Announcement> Announcements { get; set; }




        public DbSet<Trackable> Trackables { get; set; }
        public DbSet<TrackableMetadata> InternalTrackableMetadatas { get; set; }





        public DbSet<UserAction> UserActions { get; set; }
        public DbSet<UserUploadData> UserUploadDatas { get; set; }

        #region Simulation
        public DbSet<Sim_MonthlyReport> Sim_MonthlyReports { get; set; }
        public DbSet<Sim_Transaction> Sim_Transactions { get; set; }
        public DbSet<Sim_TransactionCategory> Sim_TransactionCategories { get; set; }



        #endregion



        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }


        #region Custom Add Method Overloads
        public override EntityEntry Add(object _entity)
        {
            if (_entity is ISyncable syncable)
                _ = syncable.VersionUp();

            return base.Add(_entity);
        }

        public override EntityEntry<TEntity> Add<TEntity>(TEntity _entity)
        {
            if (_entity is ISyncable syncable)
                _ = syncable.VersionUp();

            return base.Add(_entity);
        }

        public override void AddRange(params object[] _entities)
        {
            foreach (var entity in _entities)
            {
                if (entity is ISyncable syncable)
                    _ = syncable.VersionUp();
            }

            base.AddRange(_entities);
        }

        public override void AddRange(IEnumerable<object> _entities)
        {
            foreach (var entity in _entities)
            {
                if (entity is ISyncable syncable)
                    _ = syncable.VersionUp();
            }

            base.AddRange(_entities);
        }

        public override ValueTask<EntityEntry> AddAsync(object _entity, CancellationToken _cancellationToken = default)
        {
            if (_entity is ISyncable syncable)
                _ = syncable.VersionUp();

            return base.AddAsync(_entity, _cancellationToken);
        }

        public override ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity _entity, CancellationToken _cancellationToken = default)
        {
            if (_entity is ISyncable syncable)
                _ = syncable.VersionUp();

            return base.AddAsync(_entity, _cancellationToken);
        }

        public override Task AddRangeAsync(params object[] _entities)
        {
            foreach (var entity in _entities)
            {
                if (entity is ISyncable syncable)
                    _ = syncable.VersionUp();
            }

            return base.AddRangeAsync(_entities);
        }

        public override Task AddRangeAsync(IEnumerable<object> _entities, CancellationToken _cancellationToken = default)
        {
            foreach (var entity in _entities)
            {
                if (entity is ISyncable syncable)
                    _ = syncable.VersionUp();
            }

            return base.AddRangeAsync(_entities, _cancellationToken);
        }
        #endregion

        #region Custom Update Method Overloads
        public override EntityEntry Update(object _entity)
        {
            if (_entity is ISyncable syncable)
                _ = syncable.VersionUp();

            return base.Update(_entity);
        }

        public override EntityEntry<TEntity> Update<TEntity>(TEntity _entity)
        {
            if (_entity is ISyncable syncable)
                _ = syncable.VersionUp();

            return base.Update(_entity);
        }

        public override void UpdateRange(params object[] _entities)
        {
            foreach (var entity in _entities)
            {
                if (entity is ISyncable syncable)
                    _ = syncable.VersionUp();
            }

            base.UpdateRange(_entities);
        }

        public override void UpdateRange(IEnumerable<object> _entities)
        {
            foreach (var entity in _entities)
            {
                if (entity is ISyncable syncable)
                    _ = syncable.VersionUp();
            }

            base.UpdateRange(_entities);
        }
        #endregion
    }

}

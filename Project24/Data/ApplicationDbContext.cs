/*  ApplicationDbContext.cs
 *  Version: 1.16 (2023.01.07)
 *
 *  Contributor
 *      Arime-chan
 */

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project24.Models;
using Project24.Models.ClinicManager;
using Project24.Models.Identity;
using Project24.Models.Inventory.ClinicManager;
using Project24.Models.Nas;
using System;
using System.Linq;

namespace Project24.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<P24IdentityUser> P24Users { get; set; }

        #region Clinic Manager
        public DbSet<CustomerProfile> CustomerProfiles { get; set; }
        public DbSet<CustomerImage> CustomerImages { get; set; }

        public DbSet<TicketProfile> TicketProfiles { get; set; }
        public DbSet<TicketImage> TicketImages { get; set; }

        public DbSet<Drug> Drugs { get; set; }

        public DbSet<DrugInRecord> DrugInRecords { get; set; }
        public DbSet<DrugInBatch> DrugInBatches { get; set; }

        public DbSet<DrugOutRecord> DrugOutRecords { get; set; }
        public DbSet<DrugOutBatch> DrugOutBatches { get; set; }
        #endregion

        #region Nas
        public DbSet<NasCachedFile> NasCachedFiles { get; set; }
        #endregion

        public DbSet<UserUpload> UserUploads { get; set; }

        public DbSet<ActionRecord> ActionRecords { get; set; }

        public DbSet<P24ObjectPreviousVersion> Changelogs { get; set; }


        /// <summary>
        /// NOTE: Cache this value as much as possible, because each reference fetchs from database again.
        /// </summary>
        public DailyIndexes DailyIndexes
        {
            get
            {
                DailyIndexes dind = (from _dind in DailyIndexesInternal
                                     where _dind.Date == DateTime.Today
                                     select _dind)
                                    .FirstOrDefault();

                if (dind == null)
                {
                    dind = new DailyIndexes();
                    DailyIndexesInternal.Add(dind);

                    SaveChanges();
                }

                return dind;
            }

            set
            {
                DailyIndexes dind = (from _dind in DailyIndexesInternal
                                     where _dind.Date == value.Date
                                     select _dind)
                                    .FirstOrDefault();

                if (dind == null)
                {
                    dind = new DailyIndexes(value.Date)
                    {
                        CustomerIndex = value.CustomerIndex,
                        VisitingIndex = value.VisitingIndex
                    };
                    DailyIndexesInternal.Add(dind);
                }
                else
                {
                    dind.CustomerIndex = value.CustomerIndex;
                    dind.VisitingIndex = value.VisitingIndex;

                    DailyIndexesInternal.Update(dind);
                }
            }
        }

        private DbSet<DailyIndexes> DailyIndexesInternal { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }


        protected override void OnConfiguring(DbContextOptionsBuilder _optionsBuilder)
        {
            //_optionsBuilder.UseSnakeCaseNamingConvention();
            _optionsBuilder.EnableSensitiveDataLogging();
            _optionsBuilder.EnableDetailedErrors();


        }

        protected override void OnModelCreating(ModelBuilder _builder)
        {
            base.OnModelCreating(_builder);

            _builder.Entity<CustomerProfile>()
                .HasIndex(_customer => _customer.Code)
                .IsUnique();

            _builder.Entity<CustomerProfile>()
                .HasIndex(_customer => _customer.PhoneNumber)
                .IsUnique();

            _builder.Entity<TicketProfile>()
                .HasIndex(_ticket => _ticket.Code)
                .IsUnique();

            _builder.Entity<P24ObjectPreviousVersion>()
                .HasIndex(_item => _item.ObjectType);

        }
    }

}

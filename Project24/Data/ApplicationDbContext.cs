/*  Data/ApplicationDbContext.cs
 *  Version: v1.4 (2023.09.23)
 *  
 *  Author
 *      Arime-chan
 */

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Project24.Model;
using Project24.Model.Home;
using Project24.Model.Simulator.FinancialManagement;

namespace Project24.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Announcement> Announcements { get; set; }




        public DbSet<Trackable> Trackables { get; set; }
        public DbSet<TrackableMetadata> InternalTrackableMetadatas { get; set; }





        public DbSet<UserUploadData> UserUploadDatas { get; set; }

        #region Simulation
        public DbSet<Sim_MonthlyReport> Sim_MonthlyReports { get; set; }
        public DbSet<Sim_Transaction> Sim_Transactions { get; set; }
        public DbSet<Sim_TransactionCategory> Sim_TransactionCategories { get; set; }



        #endregion



        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

    }

}

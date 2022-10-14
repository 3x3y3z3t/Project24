/*  ApplicationDbContext.cs
 *  Version: 1.6 (2022.10.08)
 *
 *  Contributor
 *      Arime-chan
 */
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project24.Identity;
using Project24.Models;

namespace Project24.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public static ApplicationDbContext Instance { get; protected set; }


        public DbSet<P24IdentityUser> P24Users { get; set; }
        public DbSet<P24IdentityRole> P24Roles { get; set; }
        public DbSet<Project24.Models.Customer> Customer { get; set; }
        public DbSet<ActionRecord> ActionRecords { get; set; }


        public DbSet<ServiceDev> ServicesDev { get; set; }

        public DbSet<CustomerProfileDev> CustomerProfilesDev { get; set; }
        public DbSet<CustomerProfileDev2> CustomerProfilesDev2 { get; set; }
        public DbSet<CustomerImageDev> CustomerImageDev { get; set; }
        public DbSet<VisitingProfileDev> VisitingProfilesDev { get; set; }
        public DbSet<ServiceUsageProfileDev> ServiceUsageProfilessDev { get; set; }



        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Instance = this;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder _optionsBuilder)
        {
            //_optionsBuilder.UseSnakeCaseNamingConvention();

        }

    }

}

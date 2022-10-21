/*  ApplicationDbContext.cs
 *  Version: 1.7 (2022.10.21)
 *
 *  Contributor
 *      Arime-chan
 */

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project24.Identity;
using Project24.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Project24.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {

        public DbSet<P24IdentityUser> P24Users { get; set; }
        public DbSet<CustomerProfile> CustomerProfiles { get; set; }
        public DbSet<CustomerImage> CustomerImages { get; set; }

        public DbSet<ActionRecord> ActionRecords { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        public async Task RecordChanges(
            string _username,
            string _operation,
            string _status,
            Dictionary<string, string> _customInfo = null)
        {
            string customInfo = null;
            if (_customInfo != null)
                JsonSerializer.Serialize(_customInfo);

            ActionRecord record = new ActionRecord()
            {
                Timestamp = DateTime.Now,
                Username = _username,
                Operation = _operation,
                OperationStatus = _status,
                CustomInfo = customInfo
            };
            await AddAsync(record);
            await SaveChangesAsync();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder _optionsBuilder)
        {
            //_optionsBuilder.UseSnakeCaseNamingConvention();
            _optionsBuilder.EnableSensitiveDataLogging();
            _optionsBuilder.EnableDetailedErrors();


        }


    }

}

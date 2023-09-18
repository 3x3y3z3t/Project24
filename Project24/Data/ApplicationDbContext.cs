/*  Data/ApplicationDbContext.cs
 *  Version: v1.3 (2023.09.12)
 *  
 *  Author
 *      Arime-chan
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project24.Model;
using Project24.Model.Home;

namespace Project24.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Announcement> Announcements { get; set; }




        public DbSet<Trackable> Trackables { get; set; }
        public DbSet<TrackableMetadata> InternalTrackableMetadatas { get; set; }





        public DbSet<UserUploadData> UserUploadDatas { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

    }

}

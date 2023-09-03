/*  Data/ApplicationDbContext.cs
 *  Version: v1.2 (2023.08.31)
 *  
 *  Author
 *      Arime-chan
 */

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project24.Model;
using Project24.Model.Home;

namespace Project24.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {





        public DbSet<Trackable> Trackables { get; set; }
        public DbSet<TrackableMetadata> InternalTrackableMetadatas { get; set; }





        public DbSet<UserUploadData> UserUploadDatas { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

    }

}

/*  Data/ApplicationDbContext.cs
 *  Version: v1.1 (2023.06.29)
 *  
 *  Contributor
 *      Arime-chan
 */

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project24.Model.Home;

namespace Project24.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<InternalState> InternalStates { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

    }

}

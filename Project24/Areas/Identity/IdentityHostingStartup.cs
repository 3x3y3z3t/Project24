/*  Identity/IdentityHostingStartup.cs
 *  Version: 1.0 (2022.12.11)
 *
 *  Contributor
 *      Arime-chan
 */

using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(Project24.Areas.Identity.IdentityHostingStartup))]
namespace Project24.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }

}

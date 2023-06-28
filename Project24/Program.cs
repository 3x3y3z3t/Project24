/*  Project24
 *  
 *  Program.cs
 *  Version: v1.1 (2023.06.19)
 *  
 *  Contributor
 *      Arime-chan
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Project24.App;
using Project24.App.Services;
using Project24.Data;

namespace Project24
{
    public class Program
    {
        public static void Main(string[] _args)
        {
            var builder = WebApplication.CreateBuilder(_args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddRazorPages();

            builder.Services.Configure<KestrelServerOptions>(_options =>
            {
                _options.Limits.MaxRequestBodySize = Constants.MaxRequestSize;
            });

            builder.Services.AddSingleton<FileSystemSvc>();
            builder.Services.AddSingleton<UpdaterSvc>();

            


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();

        }
    }

}

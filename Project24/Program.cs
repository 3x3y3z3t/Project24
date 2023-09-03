/*  Project24
 *  
 *  Program.cs
 *  Version: v1.2 (2023.09.02)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
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
        public static string WorkingDir { get; private set; }
        public static string AppSide { get; private set; }
        public static string LaunchUsername { get; private set; }

        public static string AppsettingsVersion { get; private set; }


        public static bool IsDevelopment { get; private set; }


        public static void Main(string[] _args)
        {
            var builder = WebApplication.CreateBuilder(_args);
            var configuration = builder.Configuration;
            var services = builder.Services;

            // ==================================================
            //
            // APP CONFIGURATION
            //
            // ==================================================

            WorkingDir = Directory.GetCurrentDirectory();

            _ = bool.TryParse(configuration["IsDevelopment"], out bool isDev);

            if (WorkingDir.Contains(AppSide_.PREV))
                AppSide = AppSide_.PREV;
            else if (WorkingDir.Contains(AppSide_.MAIN))
                AppSide = AppSide_.MAIN;
            else
            {
                AppSide = null;
                Console.WriteLine("Invalid App Side (app location: " + WorkingDir + ")");
                if (isDev)
                {
                    Console.WriteLine("Development mode (soft), app may continue.");
                }
                else
                {
                    Console.WriteLine("App will now exit due to invalid AppSide.");
                    Environment.Exit(0);
                }
            }

            LaunchUsername = Environment.UserName;

            AppsettingsVersion = configuration["AppsettingsVersion"] ?? string.Empty;

            Console.WriteLine();

            // ==================================================
            //
            // SERVICE CONFIGURATION
            //
            // ==================================================

            #region Database Connection
            /* Database Connection */
            var connectionString = configuration.GetConnectionString("MySQLConnection");
            var dbUsername = configuration["Credentials:DbCredential:Username"];
            var dbPassword = configuration["Credentials:DbCredential:Password"];
            connectionString += "; user=" + dbUsername + "; password=" + dbPassword + "";

            var serverVersion = ServerVersion.AutoDetect(connectionString);

            services.AddDbContext<ApplicationDbContext>(_options =>
            {
                _options.UseMySql(connectionString, serverVersion)
                //.EnableSensitiveDataLogging()
                //.EnableDetailedErrors()
                ;
            });
            #endregion

            #region Identity
            /* Identity */
            // TODO: Add Identity;
            #endregion

            #region Server Configuration
            /* Server Configuration */
            services.Configure<KestrelServerOptions>(_options =>
            {
                _options.Limits.MaxRequestBodySize = Constants.MaxRequestSize;
            });

            services.Configure<FormOptions>(_options =>
            {
                //_options.ValueCountLimit = 1024;
                _options.ValueLengthLimit = Constants.MaxRequestSize;
            });
            #endregion

            #region Hosted Services & Background Tasks
            /* Hosted Services & Background Tasks */
            services.AddSingleton<LocalizationSvc>();
            services.AddSingleton<InternalTrackerSvc>();
            //services.AddSingleton<DBMaintenanceSvc>();
            services.AddSingleton<FileSystemSvc>();
            services.AddSingleton<UpdaterSvc>();
            #endregion

            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddRazorPages().AddDataAnnotationsLocalization(_option =>
            {
                _option.DataAnnotationLocalizerProvider = (type, _factory) => _factory.Create(typeof(SharedLocalizationResource));
            });

            #region Misc
            #endregion

            // Add services to the container.
            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();



            var app = builder.Build();

            // ==================================================
            //
            // STARTUP INITIALIZATION
            //
            // ==================================================

            IsDevelopment = app.Environment.IsDevelopment();

            string usn = LaunchUsername;

            //ILogger<Program> logger = null;

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();

                //var maintenanceSvc = scope.ServiceProvider.GetRequiredService<DBMaintenanceSvc>();
                //maintenanceSvc.StartService();

                var localizationSvc = scope.ServiceProvider.GetRequiredService<LocalizationSvc>();
                localizationSvc.StartService();

                var trackerSvc = scope.ServiceProvider.GetRequiredService<InternalTrackerSvc>();
                trackerSvc.StartService();

















                var updaterSvc = scope.ServiceProvider.GetRequiredService<UpdaterSvc>();
                updaterSvc.StartService();

                //logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            }


            /*
             public readonly IPasswordHasher<ApplicationUser> _passwordHasher;
public HomeController(IPasswordHasher<ApplicationUser> passwordHasher )
{
    _passwordHasher = passwordHasher;
}

            var hasedPassword = _passwordHasher.HashPassword(null,"Password");

            var successResult = _passwordHasher.VerifyHashedPassword(null, hasedPassword , "Password");
             */



            // ==================================================
            //
            // HTTP REQUEST PIPELINE
            //
            // ==================================================

            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
                app.UseDeveloperExceptionPage();
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

            // ==================================================
            //
            // APP RUN
            //
            // ==================================================
            //logger.LogInformation("==================\r\n= App is running =\r\n==================");
            app.Run();


        }
    }

}

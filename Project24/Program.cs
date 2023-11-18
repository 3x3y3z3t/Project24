/*  Project24
 *  
 *  Program.cs
 *  Version: v1.9 (2023.11.19)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Project24.App;
using Project24.App.Services;
using Project24.App.Utils;
using Project24.Data;
using Project24.Model.Identity;

namespace Project24
{
    public class Program
    {
        public static string WorkingDir { get; private set; }
        public static string AppSide { get; private set; }
        public static string LaunchUsername { get; private set; }
        public static string CurrentSessionName { get; private set; }

        public static string AppsettingsVersion { get; private set; }
        public static bool IsDevelopment { get; private set; }




        public static async Task Main(string[] _args)
        {
            Console.WriteLine("\r\n" + LINE + "\r\n==    Project24 Started                         ==\r\n" + LINE + "\r\n");

            P24Stopwatch stopwatch = P24Stopwatch.StartNew();

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
                Console.WriteLine("Invalid App Side (app location: " + WorkingDir + ")");
                if (isDev)
                {
                    AppSide = "dev";
                    Console.WriteLine("Development mode (soft), app may continue.");
                }
                else
                {
                    Console.WriteLine("App will now exit due to invalid AppSide.");
                    Environment.Exit((int)ExitCodes.InvalidAppSide);
                }
            }

            LaunchUsername = Environment.UserName;
            CurrentSessionName = string.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);

            AppsettingsVersion = configuration["AppsettingsVersion"] ?? string.Empty;

            Console.WriteLine();

            stopwatch.Lap("[Cfg] App Config (global)");

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

            // TODO: consider pooling DB context instead;
            // https://learn.microsoft.com/en-us/ef/core/performance/advanced-performance-topics?tabs=with-di%2Cexpression-api-with-constant#dbcontext-pooling

            services.AddDbContext<ApplicationDbContext>(_options =>
            {
                _options.UseMySql(connectionString, serverVersion)
                //.EnableSensitiveDataLogging()
                //.EnableDetailedErrors()
                ;
            });

            // TODO: handle case in which db connection cannot be established;

            stopwatch.Lap("[Cfg] Db Connection config");
            #endregion

            #region Identity
            /* Identity */
            builder.Services.AddIdentity<P24IdentityUser, P24IdentityRole>((_options) =>
            {
                _options.SignIn.RequireConfirmedAccount = true;
                _options.SignIn.RequireConfirmedEmail = true;
                _options.SignIn.RequireConfirmedPhoneNumber = true;

                _options.Password.RequiredLength = 8;
                _options.Password.RequireNonAlphanumeric = false;
                _options.Password.RequireUppercase = false;
                _options.Password.RequireLowercase = false;

                _options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_";
            })
                .AddDefaultUI()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            stopwatch.Lap("[Cfg] Identity config");
            #endregion

            #region Server Configuration
            /* Server Configuration */
            services.Configure<KestrelServerOptions>(_options =>
            {
                _options.Limits.MaxRequestBodySize = Constants.MaxRequestSize;
                _options.AllowSynchronousIO = true;
            });

            services.Configure<FormOptions>(_options =>
            {
                //_options.ValueCountLimit = 1024;
                _options.ValueLengthLimit = Constants.MaxRequestSize;
            });

            stopwatch.Lap("[Cfg] Server config");
            #endregion

            #region Hosted Services & Background Tasks
            /* Hosted Services & Background Tasks */
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSingleton<DBMaintenanceSvc>();
            services.AddSingleton<FileSystemSvc>();
            services.AddSingleton<InternalTrackerSvc>();
            services.AddSingleton<LocalizationSvc>();
            services.AddSingleton<ServerAnnouncementSvc>();
            services.AddSingleton<UpdaterSvc>();

            stopwatch.Lap("[Cfg] Hosted Service registration");
            #endregion


            #region Misc
            services.AddLogging(LoggerConfig.ConfigureLogger);

            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddRazorPages();

            // for web api;
            services.AddControllers().ConfigureApiBehaviorOptions((_options) =>
            {

            });
            
            stopwatch.Lap("[Cfg] Misc config");
            #endregion

            var app = builder.Build();

            // ==================================================
            //
            // STARTUP INITIALIZATION
            //
            // ==================================================

            IsDevelopment = app.Environment.IsDevelopment();
            s_HostAppLifetime = app.Lifetime;

            app.Lifetime.ApplicationStarted.Register(() => app.Logger.LogInformation(LINE + "\r\n==    Application Started                       ==\r\n" + LINE));
            app.Lifetime.ApplicationStopping.Register(() => app.Logger.LogInformation(LINE + "\r\n==    Application Stopping                      ==\r\n" + LINE));
            app.Lifetime.ApplicationStopped.Register(() =>
            {
                app.Logger.LogInformation(LINE + "\r\n==    Application Stopped                       ==\r\n" + LINE);
                Console.WriteLine("\r\n" + LINE + "\r\n==    Project24 Stopped                         ==\r\n" + LINE + "\r\n");
            });

            stopwatch.Lap("[Init] Startup/Shutdown msg registration");

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
                stopwatch.Lap("[Init] DB migration");

                DbSeedDataValidator validator = new(dbContext, configuration, scope.ServiceProvider, app.Logger);
                _ = await validator.ValidateAsync();
                stopwatch.Lap("[Init] Validate DB seed data");

                var trackerSvc = scope.ServiceProvider.GetRequiredService<InternalTrackerSvc>();
                trackerSvc.StartService();
                stopwatch.Lap("[Init] Tracker svc init");

                var localizationSvc = scope.ServiceProvider.GetRequiredService<LocalizationSvc>();
                var svrAnnouncementSvc = scope.ServiceProvider.GetRequiredService<ServerAnnouncementSvc>();
                var dbMaintenanceSvc = scope.ServiceProvider.GetRequiredService<DBMaintenanceSvc>();

                await TaskExt.WhenAll(
                    localizationSvc.StartAsync(),
                    svrAnnouncementSvc.StartAsync(),
                    dbMaintenanceSvc.StartAsync()
                );
                stopwatch.Lap("[Init] Localize + Announcement + Db Maintenance svc init");

                var updaterSvc = scope.ServiceProvider.GetRequiredService<UpdaterSvc>();
                await updaterSvc.StartAsync();
                stopwatch.Lap("[Init] Updater svc init");






            }





            string str = "12345";
            int strLen = str.Length;
            int remainder = 4 - (strLen % 4);

            Console.WriteLine(">12341234123412341234<");
            Console.WriteLine(">" + string.Format("{0,-" + (strLen + remainder) + "}", str) + "<");




            string gatangoton = "asidufhnail kurhffghyvu834r57 yhgf8iuosegvyh dsnhbfkuy rsdg";

            byte[] data = Encoding.UTF8.GetBytes(gatangoton);

            SHA256 hasher = SHA256.Create();
            byte[] newHashAsBytesArray = hasher.ComputeHash(data);
            string newHash = "";

            foreach (byte b in newHashAsBytesArray)
            {
                newHash += b.ToString("x2");
            }

            




                string s = JsonSerializer.Serialize(DateTime.Now);
            DateTime dt = JsonSerializer.Deserialize<DateTime>("\"2023-09-24T03:32:16.238Z\"").ToLocalTime();

            //var ts = new TimeSpan(-1, 25, 59, 59,  9);
            var ts = new TimeSpan(1, 0, 0, 0);

            string tss = JsonSerializer.Serialize(ts);








            HttpClient client = new();

            var result = await client.GetAsync("http://192.168.10.111/Home/Changelog?handler=Changelog", HttpCompletionOption.ResponseContentRead);

            string content = await result.Content.ReadAsStringAsync();

            int k = 0;






            // TODO: create power user;








            /*
             public readonly IPasswordHasher<ApplicationUser> _passwordHasher;
public HomeController(IPasswordHasher<ApplicationUser> passwordHasher )
{
    _passwordHasher = passwordHasher;
}

            var hasedPassword = _passwordHasher.HashPassword(null,"Password");

            var successResult = _passwordHasher.VerifyHashedPassword(null, hasedPassword , "Password");
             */

            stopwatch.Lap("[Test] Dev Test code run");

            #region Http Request Pipeline
            // ==================================================
            //
            // HTTP REQUEST PIPELINE
            //
            // ==================================================

            //var localizationsOption = app.Services.GetService<IOptions<RequestLocalizationOptions>>().Value;
            //app.UseRequestLocalization(localizationsOption);

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

            app.UseP24Localization();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseP24PostAuthentication();

            app.UseAuthorization();

            app.MapRazorPages();

            stopwatch.Lap("[Pipeline] Http Request Pipeline setup");
            #endregion

            stopwatch.Stop();
            TimeSpan totalTime = stopwatch.Elapsed;

            string msg = string.Format("Total time: {0:0}s {1:000}ms\n", (int)totalTime.TotalSeconds, totalTime.Milliseconds) + stopwatch.GetLapsesAsLogString();
            app.Logger.LogInformation("{_msg}", msg);

            // ==================================================
            //
            // APP RUN
            //
            // ==================================================


            app.Run();
        }

        /// <summary>
        ///     Shuts down the app gracefully.<br />
        ///     Use <see cref="ShutdownGracefully(int)"/> overload when you want to pass your own Exit Code.
        /// </summary>
        /// <param name="_exitCode"></param>
        public static void ShutdownGracefully(ExitCodes _exitCode) => ShutdownGracefully((int)_exitCode);

        /// <summary>
        ///     Shuts down the app gracefully.<br />
        ///     Use this overload when you want to pass your own Exit Code.
        ///     Try not to make your exit codes overlap with <see cref="ExitCodes"/>.
        /// </summary>
        /// <param name="_exitCode"></param>
        public static void ShutdownGracefully(int _exitCode)
        {
            if (s_HostAppLifetime == null)
                Environment.Exit(_exitCode);

            Environment.ExitCode = _exitCode;
            s_HostAppLifetime.StopApplication();
        }


        private const string LINE = "==================================================";


        private static IHostApplicationLifetime s_HostAppLifetime;
    }

}

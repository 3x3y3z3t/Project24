/*  Project24
 *  
 *  Program.cs
 *  Version: v1.6 (2023.10.06)
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
using Project24.App.Identity;
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

    internal class DbSeedDataValidator
    {
        public DbSeedDataValidator(ApplicationDbContext _dbContext, IConfiguration _configuration, IServiceProvider _serviceProvider, ILogger _logger)
        {
            m_DbContext = _dbContext;

            m_Configuration = _configuration;
            m_ServiceProvider = _serviceProvider;
            m_logger = _logger;
        }


        internal async Task<bool> ValidateAsync()
        {
            bool flag = true;

            flag &= ValidateRoles();
            flag &= ValidatePowerUser();

            return flag;
        }

        private bool ValidateRoles()
        {
            RoleManager<P24IdentityRole> roleManager = m_ServiceProvider.GetService<RoleManager<P24IdentityRole>>();

            var roles = (from _role in roleManager.Roles
                         select _role.Name)
                        .ToList();

            foreach (string roleName in P24RoleName.AllRoleNames)
            {
                if (roles.Contains(roleName))
                    continue;

                P24IdentityRole role = new(roleName);
                if (!roleManager.CreateAsync(role).Result.Succeeded)
                    m_logger.LogWarning("Could not create role '{_roleName}'.", roleName);
            }

            return true;
        }

        private bool ValidatePowerUser()
        {
            UserManager<P24IdentityUser> userManager = m_ServiceProvider.GetRequiredService<UserManager<P24IdentityUser>>();

            string powerUsername = m_Configuration["Credentials:PowerUser:Username"];
            string powerPassword = m_Configuration["Credentials:PowerUser:Password"];

            P24IdentityUser powerUser = userManager.FindByNameAsync(powerUsername).Result;

            // add user if null;
            if (powerUser == null)
            {
                powerUser = new P24IdentityUser(Constants.AppReleaseDate)
                {
                    UserName = powerUsername,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                };

                if (!userManager.CreateAsync(powerUser, powerPassword).Result.Succeeded)
                {
                    m_logger.LogWarning("Power user was not found but could not be created (possible DB issue?).");
                    return false;
                }

                m_logger.LogInformation("Power user was not found and has been created.");
                return ValidatePowerUserRoles(powerUser, P24RoleName.AllRoleNames, true);
            }

            // correct user password if incorrect;
            if (!userManager.CheckPasswordAsync(powerUser, powerPassword).Result)
            {
                if (!userManager.RemovePasswordAsync(powerUser).Result.Succeeded)
                {
                    m_logger.LogWarning("Power user's password was invalid but could not be removed (this should not happen).");
                    return false;
                }

                if (!userManager.AddPasswordAsync(powerUser, powerPassword).Result.Succeeded)
                {
                    m_logger.LogCritical("Power user's password was invalid, got cleared but new password could not be created.\n" +
                        "App will exit due to security issue.");

                    Program.ShutdownGracefully(ExitCodes.PowerUserNoPassword);
                }

                m_logger.LogInformation("Power user's password was invalid and has been validated.");
            }

            // correct account status if incorrect;
            if (!powerUser.EmailConfirmed || !powerUser.PhoneNumberConfirmed)
            {
                powerUser.EmailConfirmed = true;
                powerUser.PhoneNumberConfirmed = true;

                if (!userManager.UpdateAsync(powerUser).Result.Succeeded)
                {
                    m_logger.LogWarning("Power user's confirmed status was incorrect but could not be correcred.\nPower user will not be able to log in.");
                }
                else
                {
                    m_logger.LogInformation("Power user's confirmed status was incorrect and has been corrected.");
                }
            }

            return ValidatePowerUserRoles(powerUser, P24RoleName.AllRoleNames);
        }

        private bool ValidatePowerUserRoles(P24IdentityUser _user, List<string> _roles, bool _bypassCheck = false)
        {
            UserManager<P24IdentityUser> userManager = m_ServiceProvider.GetRequiredService<UserManager<P24IdentityUser>>();

            List<string> addList;

            if (_bypassCheck)
                addList = _roles;
            else
            {
                var userRoles = userManager.GetRolesAsync(_user).Result;

                if (userRoles.Count == 0)
                    addList = _roles;
                else
                {
                    addList = new();
                    foreach (string role in _roles)
                    {
                        if (userRoles.Contains(role))
                            continue;

                        addList.Add(role);
                    }
                }
            }

            if (addList.Count == 0)
                return true;

            if (!userManager.AddToRolesAsync(_user, addList).Result.Succeeded)
            {
                m_logger.LogWarning("Could not add missing roles for power user.");
                return false;
            }

            m_logger.LogInformation("Power user's missing roles has been added.");
            return true;
        }








        private readonly ApplicationDbContext m_DbContext;

        private readonly IConfiguration m_Configuration;
        private readonly IServiceProvider m_ServiceProvider;
        private readonly ILogger m_logger;
    }

}

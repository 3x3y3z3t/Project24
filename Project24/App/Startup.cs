/*  Startup.cs
 *  Version: 1.10 (2022.10.29)
 *
 *  Contributor
 *      Arime-chan
 */

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Project24.Data;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Storage;
using Project24.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Hosting;
using tusdotnet;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Project24.App.Utils;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Project24.App.Services;
using Project24.App;

namespace Project24
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration _configuration)
        {
            Configuration = _configuration;

            App.DriveUtils.Init();
            App.DriveUtils.FixDirectoryStructure();
            App.DriveUtils.WriteStatFile();



        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection _services)
        {
            _services.AddLogging(LoggerConfig.ConfigureLogger);

            //services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseSqlServer(
            //        Configuration.GetConnectionString("DefaultConnection")));

            string mySqlConnectionStr = Configuration.GetConnectionString("DefaultConnection");
            _services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(mySqlConnectionStr, opts => opts.ServerVersion(ServerVersion.AutoDetect(mySqlConnectionStr)))
            );

            _services.AddIdentity<P24IdentityUser, IdentityRole>((_options) =>
            {
                _options.SignIn.RequireConfirmedAccount = true;
                _options.SignIn.RequireConfirmedEmail = false;

                _options.Password.RequiredLength = 8;
                _options.Password.RequireNonAlphanumeric = false;
                _options.Password.RequireUppercase = false;
                _options.Password.RequireLowercase = false;

                _options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_";
            })
                .AddDefaultUI()
                .AddDefaultTokenProviders()
                .AddErrorDescriber<P24IdentityErrorDescriber>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            _services.AddControllersWithViews();
            _services.AddRazorPages(_options =>
            {
                _options.Conventions.AddPageRoute("/Home/Index", "/");
            });

            _services.Configure<KestrelServerOptions>(_options =>
            {
                _options.Limits.MaxRequestBodySize = 64L * 1024L * 1024L;
            });

            _services.AddHostedService<NasDiskService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder _app,
            IWebHostEnvironment _env,
            IServiceProvider _serviceProvider,
            ApplicationDbContext _dbContext,
            ILogger<Startup> _logger)
        {
            m_Logger = _logger;

            _logger.LogInformation("Nas Stats:\r\n" + DriveUtils.GetNasDetails());

            Utils.UpdateCurrentVersion(_env).Wait();
            _logger.LogInformation("App version: " + Utils.CurrentVersion);

            MigrateDatabase(_serviceProvider, _dbContext, _logger).Wait();

            _logger.LogInformation("Configuring Middleware Pipelile..");

            _app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            if (_env.IsDevelopment())
            {
                _app.UseDeveloperExceptionPage();
                _app.UseDatabaseErrorPage();
            }
            else
            {
                _app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                _app.UseHsts();
            }

            _app.UseStaticFiles();

            _app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.GetFullPath(Utils.AppRoot + "/" + AppConfig.DataRoot)),
                RequestPath = "/data"
            });

            _app.UseHttpsRedirection();

            _app.UseRouting();

            _app.UseAuthentication();

            /*  ! The request timeout middleware is only needed when running directly on Kestrel. 
             *  In other circumstances the reverse proxy will handle the request timeout 
             *  and notify tusdotnet that the client has disconnected.
             */
            // tus upload;
            _app.UseTus(TusDotNetConfig.ConfigureTusDotNet);

            _app.UseAuthorization();

            _app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();

                //endpoints.MapControllerRoute(
                //    name: "static",
                //    pattern: "/Nas/Upload/{*_path}",
                //    defaults: new { page = "/Nas/Upload", action = "OnGetAsync" });

            });


            _logger.LogInformation(">> Configuring done. App ready. <<\n");
        }

        private async Task MigrateDatabase(IServiceProvider _serviceProvider, ApplicationDbContext _dbContext, ILogger<Startup> _logger)
        {
            _logger.LogInformation("Migrating Database..");

            _dbContext.Database.Migrate();

            RoleManager<IdentityRole> roleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await CreateRolesAsync(roleManager);

            UserManager<P24IdentityUser> userManager = _serviceProvider.GetRequiredService<UserManager<P24IdentityUser>>();
            await CreateDefaultUsers(userManager);

        }

        #region Initialize Roles
        private async Task CreateRolesAsync(RoleManager<IdentityRole> _roleManager)
        {
            m_Logger.LogInformation("Adding Roles..");

            foreach (string role in P24Roles.GetAllRoles())
            {
                IdentityRole p24Role = await _roleManager.FindByNameAsync(role);
                if (p24Role == null)
                {
                    p24Role = new IdentityRole() { Name = role };
                    var status = await _roleManager.CreateAsync(p24Role);
                    if (!status.Succeeded)
                    {
                        m_Logger.LogError("Could not add role " + role + ".");
                    }
                }
            }

            m_Logger.LogInformation("Done.");
        }
        #endregion

        private async Task CreateDefaultUsers(UserManager<P24IdentityUser> _userManager)
        {
            if (DefaultUsers.PowerUser != null)
                await CreatePower(DefaultUsers.PowerUser, _userManager);

            if (DefaultUsers.ArimeUser != null)
                await CreateArime(DefaultUsers.ArimeUser, _userManager);

            if (DefaultUsers.DefaultClinicManager != null)
                await CreateDefaultClinicManager(DefaultUsers.DefaultClinicManager, _userManager);

            await CreateDefaultNasTester(_userManager);


        }

        #region Initialize Default Users - Power
        private async Task CreatePower(DefaultUsers.UserCredential _user, UserManager<P24IdentityUser> _userManager)
        {
            m_Logger.LogInformation("Adding Power..");

            const string power = "POWER";

            P24IdentityUser user = await _userManager.FindByNameAsync(_user.Username);
            if (user == null)
            {
                user = new P24IdentityUser(new DateTime(2022, 8, 31, 2, 18, 37, 135))
                {
                    UserName = _user.Username,
                    Email = "hnt.exw@gmail.com",
                    EmailConfirmed = true,
                    LastName = power,
                };

                var status = await _userManager.CreateAsync(user, _user.Password);
                if (!status.Succeeded)
                {
                    m_Logger.LogWarning("Could not create Power.");
                    return;
                }
            }
            else
            {
                if (user.LastName != power)
                {
                    user.LastName = power;
                    var status = await _userManager.UpdateAsync(user);
                    if (!status.Succeeded)
                    {
                        m_Logger.LogWarning("Could not update corrected Power.");
                    }
                }
            }

            if (!await _userManager.CheckPasswordAsync(user, _user.Password))
            {
                var status = await _userManager.RemovePasswordAsync(user);
                if (!status.Succeeded)
                {
                    m_Logger.LogWarning("Could not remove Power password for correction.");
                }
                else
                {
                    status = await _userManager.AddPasswordAsync(user, _user.Password);
                    if (!status.Succeeded)
                    {
                        m_Logger.LogWarning("Could not add Power password.");
                    }
                }
            }

            foreach (string role in P24Roles.GetAllRoles())
            {
                if (!await _userManager.IsInRoleAsync(user, role))
                {
                    var status = await _userManager.AddToRoleAsync(user, role);
                    if (!status.Succeeded)
                    {
                        m_Logger.LogWarning("Could not add role " + role + " for Power.");
                    }
                }
            }

            m_Logger.LogInformation("Done.");
        }
        #endregion

        #region Initialize Default Users - Arime
        private async Task CreateArime(DefaultUsers.UserCredential _user, UserManager<P24IdentityUser> _userManager)
        {
            m_Logger.LogInformation("Adding Arime..");

            const string name = "アリメちゃん";

            P24IdentityUser user = await _userManager.FindByNameAsync(_user.Username);
            if (user == null)
            {
                user = new P24IdentityUser(new DateTime(2022, 8, 31, 2, 18, 37, 135))
                {
                    UserName = _user.Username,
                    Email = "recette.lemongrass95@gmail.com",
                    EmailConfirmed = true,
                    LastName = name,
                };

                var status = await _userManager.CreateAsync(user, _user.Password);
                if (!status.Succeeded)
                {
                    m_Logger.LogWarning("Could not create Arime.");
                    return;
                }
            }
            else
            {
                if (user.LastName != name)
                {
                    user.LastName = name;
                    var status = await _userManager.UpdateAsync(user);
                    if (!status.Succeeded)
                    {
                        m_Logger.LogWarning("Could not update corrected Arime.");
                    }
                }
            }

            if (!await _userManager.CheckPasswordAsync(user, _user.Password))
            {
                var status = await _userManager.RemovePasswordAsync(user);
                if (!status.Succeeded)
                {
                    m_Logger.LogWarning("Could not remove Arime password for correction.");
                }
                else
                {
                    status = await _userManager.AddPasswordAsync(user, _user.Password);
                    if (!status.Succeeded)
                    {
                        m_Logger.LogWarning("Could not add Arime password.");
                    }
                }
            }

            foreach (string role in P24Roles.GetAllRoles())
            {
                if (role == P24Roles.Power)
                    continue;

                if (!await _userManager.IsInRoleAsync(user, role))
                {
                    var status = await _userManager.AddToRoleAsync(user, role);
                    if (!status.Succeeded)
                    {
                        m_Logger.LogWarning("Could not add role " + role + " for Arime.");
                    }
                }
            }

            m_Logger.LogInformation("Done.");
        }
        #endregion

        #region Initialize Default Users - Default User 1
        private async Task CreateDefaultClinicManager(DefaultUsers.UserCredential _user, UserManager<P24IdentityUser> _userManager)
        {
            m_Logger.LogInformation("Adding Default User 1..");

            const string fname = "Nguyễn Trọng";
            const string lname = "Hưng";

            P24IdentityUser user = await _userManager.FindByNameAsync(_user.Username);
            if (user == null)
            {
                user = new P24IdentityUser(new DateTime(2022, 10, 4, 12, 22, 08, 135))
                {
                    UserName = _user.Username,
                    EmailConfirmed = true,
                    FirstName = fname,
                    LastName = lname,
                };

                var status = await _userManager.CreateAsync(user, _user.Password);
                if (!status.Succeeded)
                {
                    m_Logger.LogWarning("Could not create Default User 1.");
                    return;
                }
            }
            else
            {
                if (user.FirstName != fname || user.LastName != lname)
                {
                    user.FirstName = fname;
                    user.LastName = lname;

                    var status = await _userManager.UpdateAsync(user);
                    if (!status.Succeeded)
                    {
                        m_Logger.LogWarning("Could not update corrected Default User 1.");
                    }
                }
            }

            if (!await _userManager.CheckPasswordAsync(user, _user.Password))
            {
                var status = await _userManager.RemovePasswordAsync(user);
                if (!status.Succeeded)
                {
                    m_Logger.LogWarning("Could not remove Default User 1 password for correction.");
                }
                else
                {
                    status = await _userManager.AddPasswordAsync(user, _user.Password);
                    if (!status.Succeeded)
                    {
                        m_Logger.LogWarning("Could not add Default User 1 password.");
                    }
                }
            }

            if (!await _userManager.IsInRoleAsync(user, P24Roles.Manager))
            {
                var status = await _userManager.AddToRoleAsync(user, P24Roles.Manager);
                if (!status.Succeeded)
                {
                    m_Logger.LogWarning("Could not add role " + P24Roles.Manager + " for Default User 1.");
                }
            }

            m_Logger.LogInformation("Done.");
        }
        #endregion

        #region Initialize Default Users - NAS Tester
        private async Task CreateDefaultNasTester(UserManager<P24IdentityUser> _userManager)
        {
            m_Logger.LogInformation("Adding Default Nas Tester..");

            const string username = "nas-tester1";
            const string password = "nas-tester1";

            const string lname = "Nas Tester";

            P24IdentityUser user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                user = new P24IdentityUser(new DateTime(2022, 10, 18, 6, 54, 08, 135))
                {
                    UserName = username,
                    EmailConfirmed = true,
                    LastName = lname,
                };

                var status = await _userManager.CreateAsync(user, password);
                if (!status.Succeeded)
                {
                    m_Logger.LogWarning("Could not create Default Nas Tester.");
                    return;
                }
            }
            else
            {
                if (user.LastName != lname)
                {
                    user.LastName = lname;

                    var status = await _userManager.UpdateAsync(user);
                    if (!status.Succeeded)
                    {
                        m_Logger.LogWarning("Could not update corrected Default Nas Tester.");
                    }
                }
            }

            if (!await _userManager.CheckPasswordAsync(user, password))
            {
                var status = await _userManager.RemovePasswordAsync(user);
                if (!status.Succeeded)
                {
                    m_Logger.LogWarning("Could not remove Default Nas Tester password for correction.");
                }
                else
                {
                    status = await _userManager.AddPasswordAsync(user, password);
                    if (!status.Succeeded)
                    {
                        m_Logger.LogWarning("Could not add Default Nas Tester password.");
                    }
                }
            }

            if (!await _userManager.IsInRoleAsync(user, P24Roles.NasTester))
            {
                var status = await _userManager.AddToRoleAsync(user, P24Roles.NasTester);
                if (!status.Succeeded)
                {
                    m_Logger.LogWarning("Could not add role " + P24Roles.NasTester + " for Default Nas Tester.");
                }
            }

            m_Logger.LogInformation("Done.");
        }
        #endregion


        private ILogger<Startup> m_Logger;
    }

}

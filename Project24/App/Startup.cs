/*  Startup.cs
 *  Version: 1.2 (2022.10.10)
 *
 *  Contributor
 *      Arime-chan
 */
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Project24.Data;
using System;
using System.Collections.Generic;
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
using tusdotnet.Stores;
using Microsoft.Extensions.FileProviders;
using Project24.App;

namespace Project24
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseSqlServer(
            //        Configuration.GetConnectionString("DefaultConnection")));

            string mySqlConnectionStr = Configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(mySqlConnectionStr, opts => opts.ServerVersion(ServerVersion.AutoDetect(mySqlConnectionStr)))
            );

            services.AddIdentity<P24IdentityUser, P24IdentityRole>((_options) =>
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

            services.AddControllersWithViews();
            services.AddRazorPages(_options =>
            {
                _options.Conventions.AddPageRoute("/Home/Index", "/");
            });

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

            Constants.DataRoot = Configuration["RootDirs:DataRoot"];
            Constants.NasRoot = Configuration["RootDirs:NasRoot"];

            NasDriveUtils.Init();
            NasDriveUtils.WriteStatsFile();
            _logger.LogInformation("NasDriveInfo: \n" + NasDriveUtils.GetStatsString());

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

            _logger.LogInformation(Constants.WorkingDir);

            Directory.CreateDirectory(Constants.WorkingDir + "/" + Constants.DataRoot);

            _app.UseStaticFiles();
            _app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Constants.WorkingDir + "/" + Constants.DataRoot),
                RequestPath = "/data"
            });

            _app.UseHttpsRedirection();

            _app.UseRouting();

            _app.UseAuthentication();
            _app.UseAuthorization();

            /*  ! The request timeout middleware is only needed when running directly on Kestrel. 
             *  In other circumstances the reverse proxy will handle the request timeout 
             *  and notify tusdotnet that the client has disconnected.
             */

            // tus upload;
            _app.UseTus(_httpContext =>
            {
                return new tusdotnet.Models.DefaultTusConfiguration()
                {
                    // This method is called on each request so different configurations can be returned per user, domain, path etc.
                    // Return null to disable tusdotnet for the current request.

                    // c:\tusfiles is where to store files
                    Store = new TusDiskStore(Constants.WorkingDir + Constants.NasRoot),
                    // On what url should we listen for uploads?
                    UrlPath = "/Nas/Upload",
                    Events = new tusdotnet.Models.Configuration.Events
                    {
                        OnBeforeCreateAsync = (_eventContext) =>
                        {
                            if (!_eventContext.Metadata.ContainsKey("name"))
                            {
                                _eventContext.FailRequest("name metadata must be specified. ");
                            }

                            if (!_eventContext.Metadata.ContainsKey("contentType"))
                            {
                                _eventContext.FailRequest("contentType metadata must be specified. ");
                            }

                            return Task.CompletedTask;
                        },


                        OnFileCompleteAsync = async (_eventContext) =>
                        {
                            tusdotnet.Interfaces.ITusFile file = await _eventContext.GetFileAsync();
                            Dictionary<string, tusdotnet.Models.Metadata> metadata = await file.GetMetadataAsync(_eventContext.CancellationToken);
                            using Stream content = await file.GetContentAsync(_eventContext.CancellationToken);

                            // TODO: write file to disk;

                            await content.DisposeAsync();

                            var terminationStore = (tusdotnet.Interfaces.ITusTerminationStore)_eventContext.Store;
                            await terminationStore.DeleteFileAsync(_eventContext.FileId, _eventContext.CancellationToken);

                            // TODO: refresh page;
                            //await DoSomeProcessing(content, metadata);
                        }
                    },

                    MaxAllowedUploadSizeInBytesLong = 10L * 1024L * 1024L * 1024L, /* allow 10GiB */
                    Expiration = new tusdotnet.Models.Expiration.SlidingExpiration(new TimeSpan(3, 0, 0, 0)),
                };
            });

            _app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });


        }

        private async Task MigrateDatabase(IServiceProvider _serviceProvider, ApplicationDbContext _dbContext, ILogger<Startup> _logger)
        {
            _logger.LogInformation("Migrating Database..");

            _dbContext.Database.Migrate();

            RoleManager<P24IdentityRole> roleManager = _serviceProvider.GetRequiredService<RoleManager<P24IdentityRole>>();
            await CreateRolesAsync(roleManager);

            UserManager<P24IdentityUser> userManager = _serviceProvider.GetRequiredService<UserManager<P24IdentityUser>>();
            await CreateDefaultUsers(userManager);
           
        }

        private async Task CreateRolesAsync(RoleManager<P24IdentityRole> _roleManager)
        {
            m_Logger.LogInformation("Adding Roles..");

            for (int i = 0; i < Constants.s_Roles.Length; ++i)
            {
                P24IdentityRole role = await _roleManager.FindByNameAsync(Constants.s_Roles[i]);
                if (role == null)
                {
                    role = new P24IdentityRole() { Name = Constants.s_Roles[i], Level = i };
                    var result = await _roleManager.CreateAsync(role);
                    if (!result.Succeeded)
                    {
                        m_Logger.LogError($"Could not add role {Constants.s_Roles[i]}.");
                    }
                }
                else
                {
                    if (role.Level == i)
                        continue;

                    var result = await _roleManager.DeleteAsync(role);
                    if (!result.Succeeded)
                    {
                        m_Logger.LogError($"Could not delete role {role.Name} for correction.");
                        continue;
                    }

                    role.Level = i;
                    result = await _roleManager.CreateAsync(role);
                    if (!result.Succeeded)
                    {
                        m_Logger.LogError($"Could not add corrected role {role.Name}.");
                    }
                }
            }

            m_Logger.LogInformation("Done.");
        }

        private async Task CreateDefaultUsers(UserManager<P24IdentityUser> _userManager)
        {
            if (Constants.PowerUser != null)
                await CreatePower(_userManager);





        }

        private async Task CreatePower(UserManager<P24IdentityUser> _userManager)
        {
            m_Logger.LogInformation("Adding Power..");

            const string power = "POWER";

            P24IdentityUser user = await _userManager.FindByNameAsync(Constants.PowerUser.Username);
            if (user == null)
            {
                user = new P24IdentityUser()
                {
                    UserName = Constants.PowerUser.Username,
                    EmailConfirmed = true,
                    LastName = power,
                    JoinDateTime = new DateTime(2022, 8, 31, 2, 18, 37, 135),
                    LeaveDateTime = new DateTime(2022, 8, 31, 2, 18, 37, 136),
                    AttendanceProfileId = null,
                };

                var status = await _userManager.CreateAsync(user, Constants.PowerUser.Password);
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
                    var result = await _userManager.DeleteAsync(user);
                    if (!result.Succeeded)
                    {
                        m_Logger.LogWarning("Could not delete Power for correction.");
                    }
                    else
                    {
                        user.LastName = power;

                        result = await _userManager.CreateAsync(user);
                        if (!result.Succeeded)
                        {
                            m_Logger.LogWarning("Could not add corrected Power.");
                            return;
                        }
                    }
                }

            }

            if (!await _userManager.CheckPasswordAsync(user, Constants.PowerUser.Password))
            {
                var result = await _userManager.RemovePasswordAsync(user);
                if (!result.Succeeded)
                {
                    m_Logger.LogWarning("Could not remove Power password for correction.");
                }
                else
                {
                    result = await _userManager.AddPasswordAsync(user, Constants.PowerUser.Password);
                    if (!result.Succeeded)
                    {
                        m_Logger.LogWarning("Could not add Power password.");
                    }
                }
            }

            foreach (var role in Constants.s_Roles)
            {
                if (!(await _userManager.GetRolesAsync(user)).Contains(role))
                {
                    var result = await _userManager.AddToRoleAsync(user, role);
                    if (!result.Succeeded)
                    {
                        m_Logger.LogWarning($"Could not add role {role} for Power.");
                    }
                }
            }

            // adding one user user;
            m_Logger.LogInformation("Try add User User..");
            if (await _userManager.FindByNameAsync("hungnt") == null)
            {

                var userUser = new P24IdentityUser()
                {
                    UserName = "hungnt",
                    FamilyName = "Nguyễn",
                    MiddleName = "Trọng",
                    LastName = "Hưng",
                    EmailConfirmed = true,
                    JoinDateTime = DateTime.Now,
                };
                await _userManager.CreateAsync(userUser);
                await _userManager.AddPasswordAsync(userUser, Constants.DEFAULT_PASSWORD);

                await _userManager.AddToRoleAsync(userUser, P24Role_.Manager);
                m_Logger.LogInformation("Added.");
            }

            m_Logger.LogInformation("Done.");
        }

        private ILogger<Startup> m_Logger;
    }

}

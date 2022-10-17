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
using Project24.App.Utils;
using System.Text;

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
            _logger.LogInformation(NasDriveUtils.GetStatsString());

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
                string nasRootAbsPath = Path.GetFullPath(Constants.NasRoot, Constants.WorkingDir);

                return new tusdotnet.Models.DefaultTusConfiguration()
                {
                    // This method is called on each request so different configurations can be returned per user, domain, path etc.
                    // Return null to disable tusdotnet for the current request.

                    // c:\tusfiles is where to store files
                    Store = new TusDiskStore(nasRootAbsPath, true),
                    // On what url should we listen for uploads?
                    UrlPath = "/Nas/Upload0",
                    Events = new tusdotnet.Models.Configuration.Events
                    {
                        OnBeforeCreateAsync = (_eventContext) =>
                        {
                            if (!_eventContext.Metadata.ContainsKey("fileName"))
                            {
                                _eventContext.FailRequest("name metadata must be specified. ");
                            }

                            if (!_eventContext.Metadata.ContainsKey("contentType"))
                            {
                                _eventContext.FailRequest("contentType metadata must be specified. ");
                            }

                            string filePath = "";
                            if (_eventContext.Metadata.ContainsKey("filePath"))
                            {
                                filePath = _eventContext.Metadata["filePath"].GetString(Encoding.UTF8);
                            }

                            try
                            {
                                string uploadLocationAbsPath = Path.GetFullPath(filePath, nasRootAbsPath);
                                if (!uploadLocationAbsPath.Contains("wwwNas"))
                                {
                                    _eventContext.FailRequest("Invalid path '" + filePath + "'. ");
                                }
                            }
                            catch (Exception _e)
                            {
                                _eventContext.FailRequest("Invalid path '" + filePath + "'. ");
                            }

                            return Task.CompletedTask;
                        },

                        OnCreateCompleteAsync = (_eventContext) =>
                        {

                            //string filePath = "";
                            //if (_eventContext.Metadata.ContainsKey("filePath"))
                            //{
                            //    filePath = _eventContext.Metadata["filePath"].GetString(Encoding.UTF8);
                            //}

                            //string uploadLocationAbsPath = Path.GetFullPath(filePath, nasRootAbsPath);
                            //TusDotNetUtils.AddOrReplace(_eventContext.FileId, new TusDotNetUtils.FileMetadata()
                            //{
                            //    Path = uploadLocationAbsPath,
                            //    Filename = _eventContext.Metadata["fileName"].GetString(Encoding.UTF8)
                            //});

                            return Task.CompletedTask;
                        },

                        OnFileCompleteAsync = async (_eventContext) =>
                        {
                            tusdotnet.Interfaces.ITusFile file = await _eventContext.GetFileAsync();
                            Dictionary<string, tusdotnet.Models.Metadata> metadata = await file.GetMetadataAsync(_eventContext.CancellationToken);
                            Stream content = await file.GetContentAsync(_eventContext.CancellationToken);

                            // write file to disk;
                            string filePath = "";
                            if (metadata.ContainsKey("filePath"))
                            {
                                filePath = metadata["filePath"].GetString(Encoding.UTF8);
                            }

                            string uploadLocationAbsPath = Path.GetFullPath(filePath, nasRootAbsPath);
                            string fileName = metadata["fileName"].GetString(Encoding.UTF8);
                            Directory.CreateDirectory(uploadLocationAbsPath);

                            FileStream fileStream = new FileStream(uploadLocationAbsPath + fileName, FileMode.Create);
                            await content.CopyToAsync(fileStream);
                            await fileStream.DisposeAsync();

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

        #region Initialize Roles
        private async Task CreateRolesAsync(RoleManager<P24IdentityRole> _roleManager)
        {
            m_Logger.LogInformation("Adding Roles..");

            foreach (string role in P24Roles.GetAllRoles())
            {
                P24IdentityRole p24Role = await _roleManager.FindByNameAsync(role);
                if (p24Role == null)
                {
                    p24Role = new P24IdentityRole() { Name = role, Level = 0 };
                    var status = await _roleManager.CreateAsync(p24Role);
                    if (!status.Succeeded)
                    {
                        m_Logger.LogError("Could not add role " + role + ".");
                    }
                }
                else
                {
                    if (p24Role.Level == 0)
                        continue;

                    p24Role.Level = 0;
                    var status = await _roleManager.UpdateAsync(p24Role);
                    if (!status.Succeeded)
                    {
                        m_Logger.LogError("Could not update corrected role " + role + ".");
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




        }

        #region Initialize Default Users - Power
        private async Task CreatePower(DefaultUsers.UserCredential _user, UserManager<P24IdentityUser> _userManager)
        {
            m_Logger.LogInformation("Adding Power..");

            const string power = "POWER";

            P24IdentityUser user = await _userManager.FindByNameAsync(_user.Username);
            if (user == null)
            {
                user = new P24IdentityUser()
                {
                    UserName = _user.Username,
                    Email = "hnt.exw@gmail.com",
                    EmailConfirmed = true,
                    LastName = power,
                    JoinDateTime = new DateTime(2022, 8, 31, 2, 18, 37, 135),
                    LeaveDateTime = new DateTime(2022, 8, 31, 2, 18, 37, 136),
                    AttendanceProfileId = null,
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
                user = new P24IdentityUser()
                {
                    UserName = _user.Username,
                    Email = "recette.lemongrass95@gmail.com",
                    EmailConfirmed = true,
                    LastName = name,
                    JoinDateTime = new DateTime(2022, 8, 31, 2, 18, 37, 135),
                    LeaveDateTime = new DateTime(2022, 8, 31, 2, 18, 37, 136),
                    AttendanceProfileId = null,
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

            const string fname = "Nguyễn";
            const string mname = "Trọng";
            const string lname = "Hưng";

            P24IdentityUser user = await _userManager.FindByNameAsync(_user.Username);
            if (user == null)
            {
                user = new P24IdentityUser()
                {
                    UserName = _user.Username,
                    EmailConfirmed = true,
                    FamilyName = fname,
                    MiddleName = mname,
                    LastName = lname,
                    JoinDateTime = new DateTime(2022, 10, 4, 12, 22, 08, 135),
                    AttendanceProfileId = null,
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
                if (user.FamilyName != fname || user.MiddleName != mname || user.LastName != lname)
                {
                    user.FamilyName = fname;
                    user.MiddleName = mname;
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


        private ILogger<Startup> m_Logger;
    }

}

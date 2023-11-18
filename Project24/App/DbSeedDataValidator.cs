/*  App/DbSeedDataValidator.cs
 *  Version: v1.1 (2023.11.19)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Project24.App;
using Project24.App.Utils.Identity;
using Project24.Data;
using Project24.Model.Identity;

namespace Project24
{
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

            ValidateManually();

            P24RoleUtils.RolesDirtyUser = new();

            flag &= ValidateRoles();
            flag &= ValidatePowerUser();

            return flag;
        }

        private bool ValidateRoles()
        {
            RoleManager<P24IdentityRole> roleManager = m_ServiceProvider.GetRequiredService<RoleManager<P24IdentityRole>>();

            var roles = (from _role in roleManager.Roles
                         select _role.Name)
                        .ToList();

            foreach (string roleName in P24RoleUtils.AllRoleNames)
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
                return ValidatePowerUserRoles(powerUser, P24RoleUtils.AllRoleNames, true);
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

            return ValidatePowerUserRoles(powerUser, P24RoleUtils.AllRoleNames);
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

            _ = P24RoleUtils.RolesDirtyUser.Add(_user.UserName);

            m_logger.LogInformation("Power user's missing roles has been added.");
            return true;
        }

        private void ValidateManually()
        {

        }








        private readonly ApplicationDbContext m_DbContext;

        private readonly IConfiguration m_Configuration;
        private readonly IServiceProvider m_ServiceProvider;
        private readonly ILogger m_logger;
    }

}

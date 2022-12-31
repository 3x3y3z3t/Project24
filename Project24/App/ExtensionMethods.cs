/*  ExtensionMethods.cs
 *  Version: 1.0 (2022.12.31)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project24.Data;
using Project24.Models;
using Project24.Models.Identity;

namespace Project24.App.Extension
{
    public static class ExtensionMethods
    {
        public static async Task<bool> ValidateModelState(this PageModel _page, ApplicationDbContext _dbContext, P24IdentityUser _currentUser, string _operation)
        {
            if (_page.ModelState.IsValid)
                return true;

            await _dbContext.RecordChanges(
                _currentUser.UserName,
                _operation,
                ActionRecord.OperationStatus_.Failed,
                new Dictionary<string, string>()
                {
                    { CustomInfoKey.Error, ErrorMessage.InvalidModelState }
                }
            );

            return false;
        }

        public static async Task RecordChanges(this ApplicationDbContext _dbContext,
            string _username, 
            string _operation,
            string _status,
            Dictionary<string, string> _customInfo = null)
        {
            string json = null;
            if (_customInfo != null)
                json = JsonSerializer.Serialize(_customInfo);

            await _dbContext.RecordChanges(_username, _operation, _status, json);
        }

        public static async Task RecordChanges(this ApplicationDbContext _dbContext,
            string _username,
            string _operation,
            string _status,
            string _customInfo)
        {
            ActionRecord record = new ActionRecord()
            {
                Timestamp = DateTime.Now,
                Username = _username,
                Operation = _operation,
                OperationStatus = _status,
                CustomInfo = _customInfo
            };
            await _dbContext.AddAsync(record);
            await _dbContext.SaveChangesAsync();
        }


    }

}

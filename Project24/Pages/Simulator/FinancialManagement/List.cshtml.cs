/*  Home/Simulator/FinancialManagement/List.cshtml.cs
 *  Version: v1.3 (2023.10.30)
 *
 *  Author
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project24.App;
using Project24.App.Services;
using Project24.App.Utils;
using Project24.Data;
using Project24.Model.Identity;
using Project24.Model.Simulator.FinancialManagement;
using Project24.Serializer;

namespace Project24.Pages.Simulator.FinancialManagement
{
    public class ListModel : PageModel
    {
        public ListModel(UserManager<P24IdentityUser> _userManager, DBMaintenanceSvc _dbMaintenanceSvc, ApplicationDbContext _dbContext, ILogger<ListModel> _logger)
        {
            m_UserManager = _userManager;

            m_DbMaintenanceSvc = _dbMaintenanceSvc;
            m_DbContext = _dbContext;
            m_Logger = _logger;
        }


        public async Task<IActionResult> OnGetExportAsync()
        {
            if (!this.IsUserAuthorized(m_UserManager, new string[] { PageCollection.Simulator.FinancialManagement.List }))
                return Forbid();

            if (this.IsDbLockedForSync(m_DbMaintenanceSvc, m_Logger))
            {
                m_Logger.LogInformation("Sync in progress.");
                return Page();
            }

            if (this.IsDbLockedForImport(m_DbMaintenanceSvc, m_Logger))
            {
                m_Logger.LogInformation("Import in progress.");
                return Page();
            }

            var categories = await (from _category in m_DbContext.Sim_TransactionCategories select _category).ToListAsync();
            var transactions = await (from _transaction in m_DbContext.Sim_Transactions select _transaction).ToListAsync();
            var reports = await (from _report in m_DbContext.Sim_MonthlyReports select _report).ToListAsync();

            if (categories.Count <= 0 && transactions.Count <= 0 && reports.Count <= 0)
            {
                m_Logger.LogInformation("There is no data to export.");
                return Page();
            }

            ImportExportDataModel data = new()
            {
                Categories = categories,
                Transactions = transactions,
                Reports = reports,
            };

            string jsonData = JsonSerializer.Serialize(data);
            byte[] content = Encoding.UTF8.GetBytes(jsonData);
            string filename = string.Format("data-{0:yyMMdd-HHmmss}.json", DateTime.Now);

            return File(content, MediaTypeNames.Text.Plain, filename);
        }

        #region AJAX Handler
        // ==================================================
        // AJAX Handler

        // ajax handler;
        public IActionResult OnGetFetchPageData(short _year, short _month)
        {
            if (this.IsDbLockedForSync(m_DbMaintenanceSvc, m_Logger))
                return Content(MessageTag.Success + "SyncInProgress", MediaTypeNames.Text.Plain);

            if (this.IsDbLockedForImport(m_DbMaintenanceSvc, m_Logger))
                return Content(MessageTag.Success + "ImportInProgress", MediaTypeNames.Text.Plain);

            if (_year == 0)
                _year = (short)DateTime.Now.Year;

            if (_month == 0)
                _month = (short)DateTime.Now.Month;

            var report = (from _report in m_DbContext.Sim_MonthlyReports
                          where _report.Year == _year && _report.Month == _month
                          select new Sim_ReportViewModel()
                          {
                              Id = _report.Id,
                              Year = _year,
                              Month = _month,
                              BalanceIn = _report.BalanceIn,
                              BalanceOut = _report.BalanceOut,
                          })
                         .FirstOrDefault();

            if (report == null)
            {
                string msg = "No report for " + _year + "/" + _month + ".";
                return Content(MessageTag.Error + msg, MediaTypeNames.Text.Plain);
            }

            var transactions = (from _transaction in m_DbContext.Sim_Transactions.Include(_x => _x.Category)
                                where _transaction.ReportId == report.Id
                                orderby _transaction.AddedDate
                                select new Sim_TransactionViewModel()
                                {
                                    Id = _transaction.Id,
                                    AddedDate = _transaction.AddedDate,
                                    Category = _transaction.Category.Name,
                                    Amount = _transaction.Amount,
                                    Details = _transaction.Details,
                                })
                               .ToList();

            report.Transactions = transactions;

            string jsonData = JsonSerializer.Serialize(report);
            return Content(MessageTag.Success + jsonData, MediaTypeNames.Text.Plain);
        }

        // ajax handler;
        public IActionResult OnPostImportAsync(IFormFile _file)
        {
            if (!this.IsUserAuthorized(m_UserManager, new string[] { PageCollection.Simulator.FinancialManagement.List }))
                return Forbid();

            if (this.IsDbLockedForSync(m_DbMaintenanceSvc, m_Logger))
                return Content(MessageTag.Success + "SyncInProgress", MediaTypeNames.Text.Plain);

            if (this.IsDbLockedForImport(m_DbMaintenanceSvc, m_Logger))
                return Content(MessageTag.Success + "ImportInProgress", MediaTypeNames.Text.Plain);

            m_Logger.LogInformation("Accepted import data file: {_fileName} ({_strSize}).", _file.FileName, MiscUtils.FormatDataSize(_file.Length));

            ImportExportDataModel data;
            try
            {
                StreamReader reader = new(_file.OpenReadStream());
                string jsonData = reader.ReadToEnd();
                reader.Close();

                data = JsonSerializer.Deserialize(jsonData, P24JsonSerializerContext.Default.ImportExportDataModel);
            }
            catch (Exception _ex)
            {
                m_Logger.LogError("Exception during parsing import data file: {_ex}", _ex);
                return Content(MessageTag.Exception + _ex, MediaTypeNames.Text.Plain);
            }

            if (data.Categories == null && data.Reports == null && data.Transactions == null)
            {
                m_Logger.LogWarning("No data to import.");
                return Content(MessageTag.Warning + "No data to import.", MediaTypeNames.Text.Plain);
            }

            Task.Run(() => { ImportData(data); });

            return Content(MessageTag.Success + "Import", MediaTypeNames.Text.Plain);
        }

        // End: AJAX Handler
        // ==================================================
        #endregion

        private void ImportData(ImportExportDataModel _data) => m_DbMaintenanceSvc.ImportSimulatorData_FinMan(_data);


        private readonly UserManager<P24IdentityUser> m_UserManager;

        private readonly DBMaintenanceSvc m_DbMaintenanceSvc;
        private readonly ApplicationDbContext m_DbContext;
        private readonly ILogger<ListModel> m_Logger;
    }

}

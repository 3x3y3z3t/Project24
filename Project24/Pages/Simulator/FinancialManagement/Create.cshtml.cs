/*  Home/Simulator/FinancialManagement/Create.cshtml.cs
 *  Version: v1.1 (2023.10.02)
 *
 *  Author
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.App;
using Project24.App.Services;
using Project24.App.Utils;
using Project24.Data;
using Project24.Model.Simulator.FinancialManagement;

namespace Project24.Pages.Simulator.FinancialManagement
{
    public class CreateModel : PageModel
    {
        public class TransactionViewModel
        {
            public DateTime AddedDate { get; set; }
            public string Category { set; get; }
            public int Amount { set; get; }
            public string Details { set; get; }

            public TransactionViewModel()
            { }
        }


        public CreateModel(DBMaintenanceSvc _dbMaintenanceSvc, ApplicationDbContext _dbContext, ILogger<CreateModel> _logger)
        {
            m_DbMaintenanceSvc = _dbMaintenanceSvc;
            m_DbContext = _dbContext;
            m_Logger = _logger;
        }


        #region AJAX Handler
        // ==================================================
        // AJAX Handler

        // ajax handler;
        public IActionResult OnGetFetchPageData()
        {
            if (this.IsDbLockedForSync(m_DbMaintenanceSvc, m_Logger))
                return Content(MessageTag.Success + "SyncInProgress", MediaTypeNames.Text.Plain);

            if (this.IsDbLockedForImport(m_DbMaintenanceSvc, m_Logger))
                return Content(MessageTag.Success + "ImportInProgress", MediaTypeNames.Text.Plain);

            var categories = (from _category in m_DbContext.Sim_TransactionCategories
                              select _category.Name)
                             .ToList();

            string jsonData = JsonSerializer.Serialize(categories);
            return Content(MessageTag.Success + jsonData, MediaTypeNames.Text.Plain);
        }

        // ajax handler;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<IActionResult> OnPostAsync([FromBody] IList<TransactionViewModel> _data, CancellationToken _cancellationToken = default)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (this.IsDbLockedForSync(m_DbMaintenanceSvc, m_Logger))
                return Content(MessageTag.Success + "SyncInProgress", MediaTypeNames.Text.Plain);

            if (this.IsDbLockedForImport(m_DbMaintenanceSvc, m_Logger))
                return Content(MessageTag.Success + "ImportInProgress", MediaTypeNames.Text.Plain);

            if (!WaitForDbAccess())
                return Content(MessageTag.Success + "ImportInProgress", MediaTypeNames.Text.Plain);

            if (!ModelState.IsValid)
            {
                return Content(MessageTag.Error + "Invalid ModelState", MediaTypeNames.Text.Plain);
            }

            if (_data.Count <= 0)
                return OnGetFetchPageData();

            // ==========  ==========;
            m_DbMaintenanceSvc.LocbDbAccessForAdd_SimFinMan();

            List<Sim_Transaction> addedTransactions = new();
            List<Sim_TransactionCategory> addedCategories = new();
            List<Sim_MonthlyReport> addedReports;

            short firstAddedYear = short.MaxValue;
            short firstAddedMonth = 12;

            P24Stopwatch sw = P24Stopwatch.StartNew();

            var categories = (from _category in m_DbContext.Sim_TransactionCategories
                              select _category)
                             .ToList();

            var reports = (from _report in m_DbContext.Sim_MonthlyReports
                           select _report)
                          .ToList();

            double elapsedFetchReportsAndCategories = sw.Lap().TotalMilliseconds;

            addedReports = AddMissingReports(reports);
            double elapsedAddMissingReports = sw.Lap().TotalMilliseconds;

            foreach (var data in _data)
            {
                // ========== category ==========;
                Sim_TransactionCategory newCategory = TryGetCategoryFromList(data, categories);

                if (newCategory == null)
                    newCategory = TryGetCategoryFromList(data, addedCategories);

                if (newCategory == null)
                {
                    newCategory = new(data.Category);
                    addedCategories.Add(newCategory);
                }

                // ========== report ==========;
                Sim_MonthlyReport newReport = TryGetReportFromList(data, reports);

                if (newReport == null)
                    newReport = TryGetReportFromList(data, addedReports);

                if (newReport == null)
                {
                    newReport = new Sim_MonthlyReport((short)data.AddedDate.Year, (short)data.AddedDate.Month);
                    addedReports.Add(newReport);
                }

                // ========== transaction ==========;
                Sim_Transaction newTransaction = new(newCategory, newReport)
                {
                    AddedDate = data.AddedDate,
                    Amount = data.Amount,
                    Details = data.Details
                };
                addedTransactions.Add(newTransaction);

                if (firstAddedYear > data.AddedDate.Year)
                {
                    firstAddedYear = (short)data.AddedDate.Year;
                    firstAddedMonth = 12; // since Year has been pushed back, reset Month to max;
                }
                if (firstAddedMonth > data.AddedDate.Month)
                    firstAddedMonth = (short)data.AddedDate.Month;
            }

            double elapsedAddTransactions = sw.Lap().TotalMilliseconds;

            if (addedCategories.Count > 0)
                m_DbContext.AddRange(addedCategories);

            if (addedReports.Count > 0)
                m_DbContext.AddRange(addedReports);

            if (addedTransactions.Count > 0)
                m_DbContext.AddRange(addedTransactions);

            m_DbContext.SaveChanges();
            m_DbMaintenanceSvc.OpenDbAccess();

            double elapsedDbSave = sw.Lap().TotalMilliseconds;
            double totalTime = sw.Elapsed.TotalMilliseconds;
            sw.Stop();

            foreach (var report in addedReports)
            {
                if (report.Year != firstAddedYear || report.Month != firstAddedMonth)
                    continue;

                if (firstAddedMonth == 1)
                {
                    --firstAddedYear;
                    firstAddedMonth = 12;
                }
                else
                {
                    --firstAddedMonth;
                }

                if (firstAddedYear < 2023 || (firstAddedYear == 2023 && firstAddedMonth < 4))
                {
                    firstAddedYear = 2023;
                    firstAddedMonth = 4;
                }

                break;
            }

            string msg = string.Format("Added {0} transactions ({1:0.000} ms)\n"
                                 + "    > Fetch:            {2,9:0.000} ms\n"
                                 + "      MissingReports:   {3,9:0.000} ms\n"
                                 + "      Add:              {4,9:0.000} ms\n"
                                 + "    > DBSave:           {5,9:0.000} ms"
                                 ,
                                 _data.Count,
                                 totalTime,
                                 elapsedFetchReportsAndCategories,
                                 elapsedAddMissingReports,
                                 elapsedAddTransactions,
                                 elapsedDbSave);

            msg += "\n" + DBMaintenanceSvc.ResyncMonthlyReports(firstAddedYear, firstAddedMonth, m_DbContext);

            m_Logger.LogInformation("{_msg}", msg);

            return OnGetFetchPageData();
        }

        // End: AJAX Handler
        // ==================================================
        #endregion

        private static Sim_TransactionCategory TryGetCategoryFromList(TransactionViewModel _data, List<Sim_TransactionCategory> _categories)
        {
            foreach (var category in _categories)
            {
                if (_data.Category == category.Name)
                    return category;
            }

            return null;
        }

        private static Sim_MonthlyReport TryGetReportFromList(TransactionViewModel _data, List<Sim_MonthlyReport> _reports)
        {
            foreach (var report in _reports)
            {
                if (_data.AddedDate.Month == report.Month && _data.AddedDate.Year == report.Year)
                    return report;
            }

            return null;
        }

        private static List<Sim_MonthlyReport> AddMissingReports(List<Sim_MonthlyReport> _existingReports)
        {
            DateTime now = DateTime.Now;
            short currentYear = (short)now.Year;
            short currentMonth = (short)now.Month;

            List<Sim_MonthlyReport> addedList = new();

            for (short year = 2023; year <= currentYear; ++year)
            {
                for (short month = 1; month <= 12 && month <= currentMonth; ++month)
                {
                    if (year == 2023 && month < 4)
                        continue;

                    bool hasReport = false;
                    foreach (var report in _existingReports)
                    {
                        if (report.Year == year && report.Month == month)
                        {
                            hasReport = true;
                            break;
                        }
                    }

                    if (hasReport)
                        continue;

                    addedList.Add(new Sim_MonthlyReport(year, month));
                }
            }

            return addedList;
        }

        private bool WaitForDbAccess()
        {
            const int waitTime = 10 * 1000; // wait and try in 10s;
            const int tryInterval = 1000;

            for (int elapsed = 0; elapsed < waitTime; elapsed += tryInterval)
            {
                if (m_DbMaintenanceSvc.AccessState == DbAccessState.Open)
                    return true;

                Task.Delay(tryInterval).Wait();
            }

            if (m_DbMaintenanceSvc.AccessState == DbAccessState.Open)
                return true;

            if (m_DbMaintenanceSvc.AccessState == DbAccessState.LockForAdd_Sim_FinMan)
                m_Logger.LogInformation("Db access aborted (other add operation is in progress).");
            else
                m_Logger.LogInformation("Db access aborted ({_accessState}).", m_DbMaintenanceSvc.AccessState);

            return false;
        }


        private readonly DBMaintenanceSvc m_DbMaintenanceSvc;
        private readonly ApplicationDbContext m_DbContext;
        private readonly ILogger<CreateModel> m_Logger;
    }

}

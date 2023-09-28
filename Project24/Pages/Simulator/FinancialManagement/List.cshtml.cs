/*  Home/Simulator/FinancialManagement/List.cshtml.cs
 *  Version: v1.0 (2023.09.24)
 *
 *  Author
 *      Arime-chan
 */

using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project24.App;
using Project24.App.Services;
using Project24.Data;
using Project24.Model.Simulator.FinancialManagement;

namespace Project24.Pages.Simulator.FinancialManagement
{
    public class ListModel : PageModel
    {
        public ListModel(InternalTrackerSvc _trackerSvc, ApplicationDbContext _dbContext, ILogger<ListModel> _logger)
        {
            m_TrackerSvc = _trackerSvc;
            m_DbContext = _dbContext;
            m_Logger = _logger;
        }


        public void OnGet()
        { }

        #region AJAX Handler
        // ==================================================
        // AJAX Handler

        // ajax handler;
        public IActionResult OnGetFetchPageData(short _year, short _month)
        {
            if (_year == 0)
                _year = (short)DateTime.Now.Year;

            if (_month == 0)
                _month = (short)DateTime.Now.Month;

            if (bool.Parse(m_TrackerSvc[InternalTrackedKeys.SIM_FIN_MAN_IS_DIRTY]) || true)
            {
                TimeSpan ts = Task.Run(ResyncMonthlyReports).Result;
                m_Logger.LogInformation("Resync Sim_MonthlyReports took {_ts:0.00}ms.", ts.TotalMilliseconds);
            }

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

        // End: AJAX Handler
        // ==================================================
        #endregion

        private TimeSpan ResyncMonthlyReports()
        {
            Stopwatch sw = Stopwatch.StartNew();

            var reports = (from _report in m_DbContext.Sim_MonthlyReports
                           orderby _report.Year, _report.Month
                           select _report)
                          .ToList();

            var lastBalanceOut = 0;
            foreach (var report in reports)
            {
                var sum = (from _transaction in m_DbContext.Sim_Transactions
                           where _transaction.ReportId == report.Id
                           select _transaction.Amount)
                          .Sum();

                report.BalanceIn = lastBalanceOut;
                report.BalanceOut = report.BalanceIn + sum;
                lastBalanceOut = report.BalanceOut;
            }

            m_DbContext.UpdateRange(reports);
            m_TrackerSvc[InternalTrackedKeys.SIM_FIN_MAN_IS_DIRTY] = false.ToString();

            m_TrackerSvc.SaveChangesAsync(m_DbContext).Wait();

            sw.Stop();
            return sw.Elapsed;
        }


        private readonly InternalTrackerSvc m_TrackerSvc;
        private readonly ApplicationDbContext m_DbContext;
        private readonly ILogger<ListModel> m_Logger;
    }

}

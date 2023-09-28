/*  Home/Simulator/FinancialManagement/Create.cshtml.cs
 *  Version: v1.0 (2023.09.23)
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


        public CreateModel(InternalTrackerSvc _trackerSvc, ApplicationDbContext _dbContext, ILogger<CreateModel> _logger)
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
        public IActionResult OnGetFetchPageData()
        {
            var categories = (from _category in m_DbContext.Sim_TransactionCategories
                              select _category.Name)
                             .ToList();

            string jsonData = JsonSerializer.Serialize(categories);
            return Content(MessageTag.Success + jsonData, MediaTypeNames.Text.Plain);
        }

        // ajax handler;
        public async Task<IActionResult> OnPostAsync([FromBody] IList<TransactionViewModel> _data, CancellationToken _cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return Content(MessageTag.Error + "Invalid ModelState", MediaTypeNames.Text.Plain);
            }

            if (_data.Count <= 0)
            {
                return OnGetFetchPageData();
            }

            List<Sim_Transaction> addedTransactions = new();
            List<Sim_TransactionCategory> addedCategories = new();
            List<Sim_MonthlyReport> addedReports = new();

            // TODO: first sort by date time;

            foreach (var data in _data)
            {
                // category;
                var category = (from _category in m_DbContext.Sim_TransactionCategories
                                where _category.Name == data.Category
                                select _category)
                               .FirstOrDefault();

                if (category == null)
                {
                    foreach (var item in addedCategories)
                    {
                        if (data.Category == item.Name)
                        {
                            category = item;
                            break;
                        }
                    }

                    if (category == null)
                    {
                        category = new(data.Category);
                        addedCategories.Add(category);
                    }
                }
                // ==========;

                // report;
                var report = (from _report in m_DbContext.Sim_MonthlyReports
                              where _report.Year == data.AddedDate.Year && _report.Month == data.AddedDate.Month
                              select _report)
                             .FirstOrDefault();

                if (report == null)
                {
                    foreach (var item in addedReports)
                    {
                        if (data.AddedDate.Year == item.Year && data.AddedDate.Month == item.Month)
                        {
                            report = item;
                            break;
                        }
                    }

                    if (report == null)
                    {
                        report = new()
                        {
                            Year = (short)data.AddedDate.Year,
                            Month = (short)data.AddedDate.Month
                        };
                        addedReports.Add(report);
                    }
                }
                // ==========;

                Sim_Transaction transaction = new(category, report)
                {
                    AddedDate = data.AddedDate.ToLocalTime(),
                    Amount = data.Amount,
                    Details = data.Details
                };

                addedTransactions.Add(transaction);
            }

            if (addedCategories.Count > 0)
            {
                m_DbContext.AddRange(addedCategories);
            }
            if (addedReports.Count > 0)
            {
                m_DbContext.AddRange(addedReports);
            }
            if (addedTransactions.Count > 0)
            {
                m_DbContext.AddRange(addedTransactions);
            }

            m_TrackerSvc[InternalTrackedKeys.SIM_FIN_MAN_IS_DIRTY] = true.ToString();
            await m_TrackerSvc.SaveChangesAsync(m_DbContext, _cancellationToken);

            //_ = await m_DbContext.SaveChangesAsync(_cancellationToken);

            return OnGetFetchPageData();
        }

        // End: AJAX Handler
        // ==================================================
        #endregion


        private readonly InternalTrackerSvc m_TrackerSvc;
        private readonly ApplicationDbContext m_DbContext;
        private readonly ILogger<CreateModel> m_Logger;
    }

}

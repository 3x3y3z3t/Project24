/*  App/Services/DBMaintenanceSvc.cs
 *  Version: v1.0 (2023.10.02)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Project24.App.Utils;
using Project24.Data;
using Project24.Model.Simulator.FinancialManagement;

namespace Project24.App.Services
{
    public enum DbAccessState
    {
        Open = 0,
        LockForSync,
        LockForImport,
        LockForAdd_Sim_FinMan,
    }

    public class DBMaintenanceSvc : IProject24HostedService
    {
        public DbAccessState AccessState { get; private set; }


        public DBMaintenanceSvc(IServiceProvider _serviceProvider, ILogger<DBMaintenanceSvc> _logger)
        {
            m_ServiceProvider = _serviceProvider;
            m_Logger = _logger;

            m_Timer = new Timer(Update, null, Timeout.Infinite, Timeout.Infinite);
        }


        public async Task StartAsync(CancellationToken _cancellationToken = default)
        {
            AccessState = DbAccessState.LockForSync;

            using (var scope = m_ServiceProvider.CreateScope())
            {
                ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                _ = ResyncMonthlyReports(2023, 4, dbContext);
                AccessState = DbAccessState.Open;
            }


            //m_Timer.Change(0, 10 * 60 * 1000);

            m_Logger.LogInformation("Localization Service initialized.");
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="_data"></param>
        /// <returns></returns>
        internal bool ImportSimulatorData_FinMan(ImportExportDataModel _data)
        {
            m_Logger.LogInformation("Starting importing data..");
            AccessState = DbAccessState.LockForImport;

            double importTimeCreateDbContext = 0.0;
            double importTimeCategory = 0.0;
            double importTimeReport = 0.0;
            double importTimeTramsaction = 0.0;
            double importTimeSaveChanges = 0.0;
            double importTimeTotal = 0.0;

            P24Stopwatch sw = new();
            sw.Start();

            using (var scope = m_ServiceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                importTimeCreateDbContext = sw.Lap().TotalMilliseconds;

                // ========== categories ==========;
                List<Sim_TransactionCategory> categoriesAddList = new();
                List<Sim_TransactionCategory> categoriesUpdateList = new();

                if (_data.Categories != null && _data.Categories.Count > 0)
                {
                    var categories = (from _category in dbContext.Sim_TransactionCategories select new { _category.Id, _category.Version })
                                     .ToDictionary(_x => _x.Id, _x => _x.Version);

                    foreach (var newCategory in _data.Categories)
                    {
                        if (categories.ContainsKey(newCategory.Id))
                        {
                            if (newCategory.Version > categories[newCategory.Id])
                                categoriesUpdateList.Add(newCategory);
                        }
                        else
                        {
                            categoriesAddList.Add(newCategory);
                        }
                    }
                }
                importTimeCategory = sw.Lap().TotalMilliseconds;

                // ========== reports ==========;
                List<Sim_MonthlyReport> reportsAddList = new();
                List<Sim_MonthlyReport> reportsUpdateList = new();

                if (_data.Reports != null && _data.Reports.Count > 0)
                {
                    var reports = (from _report in dbContext.Sim_MonthlyReports select new { _report.Id, _report.Version })
                                  .ToDictionary(_x => _x.Id, _x => _x.Version);

                    foreach (var newReport in _data.Reports)
                    {
                        if (reports.ContainsKey(newReport.Id))
                        {
                            if (newReport.Version > reports[newReport.Id])
                                reportsUpdateList.Add(newReport);
                        }
                        else
                        {
                            reportsAddList.Add(newReport);
                        }
                    }
                }
                importTimeReport = sw.Lap().TotalMilliseconds;

                // ========== transactions ==========;
                List<Sim_Transaction> transactionsAddList = new();
                List<Sim_Transaction> transactionsUpdateList = new();

                if (_data.Transactions != null && _data.Transactions.Count > 0)
                {
                    var transactions = (from _transaction in dbContext.Sim_Transactions select new { _transaction.Id, _transaction.Version })
                                  .ToDictionary(_x => _x.Id, _x => _x.Version);

                    foreach (var newTransaction in _data.Transactions)
                    {
                        if (transactions.ContainsKey(newTransaction.Id))
                        {
                            if (newTransaction.Version > transactions[newTransaction.Id])
                                transactionsUpdateList.Add(newTransaction);
                        }
                        else
                        {
                            transactionsAddList.Add(newTransaction);
                        }
                    }
                }
                importTimeTramsaction = sw.Lap().TotalMilliseconds;

                // ========== AddRange ==========;
                if (categoriesAddList.Count > 0)
                    dbContext.AddRange(categoriesAddList);
                if (reportsAddList.Count > 0)
                    dbContext.AddRange(reportsAddList);
                if (transactionsAddList.Count > 0)
                    dbContext.AddRange(transactionsAddList);

                if (categoriesUpdateList.Count > 0)
                    dbContext.UpdateRange(categoriesUpdateList);
                if (reportsUpdateList.Count > 0)
                    dbContext.UpdateRange(reportsUpdateList);
                if (transactionsUpdateList.Count > 0)
                    dbContext.UpdateRange(transactionsUpdateList);

                dbContext.SaveChanges();
                importTimeSaveChanges = sw.Lap().TotalMilliseconds;
            }

            importTimeTotal = sw.Elapsed.TotalMilliseconds;
            sw.Stop();

            string msg = string.Format("Import done ({0:0.000} ms)\n"
                                 + "    > CreateDbCtx:  {1, 9:0.000} ms\n"
                                 + "      Categories:   {2, 9:0.000} ms\n"
                                 + "      Reports:      {3, 9:0.000} ms\n"
                                 + "      Transactions: {4, 9:0.000} ms\n"
                                 + "    > SaveChanges:  {5, 9:0.000} ms",
                                 importTimeTotal,
                                 importTimeCreateDbContext,
                                 importTimeCategory,
                                 importTimeReport,
                                 importTimeTramsaction,
                                 importTimeSaveChanges);

            AccessState = DbAccessState.Open;
            m_Logger.LogInformation("{_msg}", msg);
            return true;
        }

        internal static string ResyncMonthlyReports(short _firstAddedYear, short _firstAddedMonth, ApplicationDbContext _dbContext)
        {
            P24Stopwatch sw = P24Stopwatch.StartNew();

            var reports = (from _report in _dbContext.Sim_MonthlyReports
                           where _report.Year >= _firstAddedYear && _report.Month >= _firstAddedMonth
                           orderby _report.Year, _report.Month
                           select _report)
                          .ToList();

            double elapsedFetchReports = sw.Lap().TotalMilliseconds;

            if (reports.Count > 0)
            {
                var lastBalanceOut = reports[0].BalanceIn;
                foreach (var report in reports)
                {
                    var sum = (from _transaction in _dbContext.Sim_Transactions
                               where _transaction.ReportId == report.Id
                               select _transaction.Amount)
                              .Sum();

                    report.BalanceIn = lastBalanceOut;
                    report.BalanceOut = report.BalanceIn + sum;
                    lastBalanceOut = report.BalanceOut;
                }
            }

            double elapsedRecalculateSum = sw.Lap().TotalMilliseconds;

            _dbContext.UpdateRange(reports);
            _dbContext.SaveChanges();

            double elapsedDbSave = sw.Lap().TotalMilliseconds;
            double totalTime = sw.Elapsed.TotalMilliseconds;
            sw.Stop();

            string msg = string.Format("Synced {0} reports ({1:0.000} ms)\n"
                                 + "    > Fetch:    {2,9:0.000} ms\n"
                                 + "      Calc:     {3,9:0.000} ms\n"
                                 + "    > DBSave:   {4,9:0.000} ms"
                                 ,
                                 reports.Count,
                                 totalTime,
                                 elapsedFetchReports,
                                 elapsedRecalculateSum,
                                 elapsedDbSave);

            return msg;
        }

        internal void OpenDbAccess() => AccessState = DbAccessState.Open;
        internal void LocbDbAccessForAdd_SimFinMan() => AccessState = DbAccessState.LockForAdd_Sim_FinMan;




        private void Update(object? _state)
        {
            lock (m_Lock)
            {
                if (m_IsUpdating)
                    return;

                m_IsUpdating = true;


                m_IsUpdating = false;
            }
        }






        private bool m_IsUpdating = false;
        private long m_LastUpdateTick = 0;
        private readonly Timer m_Timer = null;
        private readonly object m_Lock = new();

        private readonly IServiceProvider m_ServiceProvider;
        private readonly ILogger<DBMaintenanceSvc> m_Logger;
    }

}

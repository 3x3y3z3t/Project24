/*  Home/Simulator/FinancialManagement/Remove.cshtml.cs
 *  Version: v1.0 (2023.10.02)
 *
 *  Author
 *      Arime-chan
 */

using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.App;
using Project24.App.Services;
using Project24.Data;

namespace Project24.Pages.Simulator.FinancialManagement
{
    public class RemoveModel : PageModel
    {
        public RemoveModel(DBMaintenanceSvc _dbMaintenanceSvc, ApplicationDbContext _dbContext, ILogger<RemoveModel> _logger)
        {
            m_DbMaintenanceSvc = _dbMaintenanceSvc;
            m_DbContext = _dbContext;
            m_Logger = _logger;
        }


        public IActionResult OnGet() => NotFound();


        #region AJAX Handler
        // ==================================================
        // AJAX Handler

        // ajax handler;
        public IActionResult OnPost([FromBody] int _id)
        {
            if (this.IsDbLockedForSync(m_DbMaintenanceSvc, m_Logger))
                return Content(MessageTag.Success + "SyncInProgress", MediaTypeNames.Text.Plain);

            if (this.IsDbLockedForImport(m_DbMaintenanceSvc, m_Logger))
                return Content(MessageTag.Success + "ImportInProgress", MediaTypeNames.Text.Plain);

            if (!ModelState.IsValid)
            {
                return Content(MessageTag.Error + "Invalid ModelState", MediaTypeNames.Text.Plain);
            }

            // ==========  ==========;
            var transaction = (from _transaction in m_DbContext.Sim_Transactions
                               where _transaction.Id == _id
                               select _transaction)
                              .FirstOrDefault();

            if (transaction == null)
            {
                return Content(MessageTag.Error + "Transaction <code>" + _id + "</code> not found.", MediaTypeNames.Text.Plain);
            }

            short year = (short)transaction.AddedDate.Year;
            short month = (short)transaction.AddedDate.Month;

            m_DbContext.Remove(transaction);
            m_DbContext.SaveChanges();

            _ = DBMaintenanceSvc.ResyncMonthlyReports(year, month, m_DbContext);

            return Content(MessageTag.Success + _id + ",Transaction <code>" + _id + "</code> removed.", MediaTypeNames.Text.Plain);
        }

        // End: AJAX Handler
        // ==================================================
        #endregion


        private readonly DBMaintenanceSvc m_DbMaintenanceSvc;
        private readonly ApplicationDbContext m_DbContext;
        private readonly ILogger<RemoveModel> m_Logger;
    }

}

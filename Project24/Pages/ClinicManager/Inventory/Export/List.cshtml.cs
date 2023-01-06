/*  P24/Inventory/Export/List.cshtml
 *  Version: 1.0 (2023.01.07)
 *
 *  Contributor
 *      Arime-chan
 */

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project24.App;
using Project24.Data;
using Project24.Models.ClinicManager.DataModel;

namespace Project24.Pages.ClinicManager.Inventory.Export
{
    [Authorize(Roles = P24RoleName.Manager)]
    public class ListModel : PageModel
    {
        public List<ImportExportBatchViewModel> ExportBatches { get; private set; }


        public ListModel(ApplicationDbContext _dbContext)
        {
            m_DbContext = _dbContext;
        }


        public async Task OnGetAsync()
        {
            var batches = await (from _batch in m_DbContext.DrugOutBatches.Include(_b => _b.AddedUser)
                                 select new ImportExportBatchViewModel()
                                 {
                                     Id = _batch.Id,
                                     AddedUserName = _batch.AddedUser.UserName,
                                     AddedDate = _batch.AddedDate,
                                     Type = P24ImportExportType.Export
                                 })
                          .ToListAsync();

            ExportBatches = batches;
        }


        private readonly ApplicationDbContext m_DbContext;
    }

}

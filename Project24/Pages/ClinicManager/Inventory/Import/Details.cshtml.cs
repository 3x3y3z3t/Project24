/*  P24/Inventory/Import/Details.cshtml
 *  Version: 1.0 (2023.01.03)
 *
 *  Contributor
 *      Arime-chan
 */

using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project24.App.Extension;
using Project24.Data;
using Project24.Models.ClinicManager.DataModel;

namespace Project24.Pages.ClinicManager.Inventory.Import
{
    [Authorize(Roles = P24RoleName.Manager)]
    public class DetailsModel : PageModel
    {
        public ImportExportBatchViewModel Batch { get; set; }


        public DetailsModel(ApplicationDbContext _dbContext)
        {
            m_DbContext = _dbContext;
        }


        public async Task OnGetAsync(int _id)
        {
            Batch = await this.DrugImport_GetBatchDetailsAsync(m_DbContext, _id);
            if (Batch == null)
            {
                Batch = new ImportExportBatchViewModel()
                {
                    Id = _id,
                    AddedUserName = CustomInfoTag.Error
                };
            }
        }

        public async Task<IActionResult> OnGetPartialOnlyAsync(int _id)
        {
            Batch = await this.DrugImport_GetBatchDetailsAsync(m_DbContext, _id);
            if (Batch == null)
            {
                string html = "<div class=\"font-weight-bold text-danger\" style=\"font-size:larger\">" + string.Format(P24Message.BatchNotFound, _id) + "</div>";
                return Content(html, MediaTypeNames.Text.Html);
            }

            return Partial("_BatchDetails", Batch);
        }


        private readonly ApplicationDbContext m_DbContext;
    }

}

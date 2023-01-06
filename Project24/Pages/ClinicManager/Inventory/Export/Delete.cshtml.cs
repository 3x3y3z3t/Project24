/*  P24/Inventory/Export/Delete.cshtml
 *  Version: 1.0 (2023.01.07)
 *
 *  Contributor
 *      Arime-chan
 */

using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project24.App.Extension;
using Project24.Data;
using Project24.Models;
using Project24.Models.Identity;

namespace Project24.Pages.ClinicManager.Inventory.Export
{
    [Authorize(Roles = P24RoleName.Admin)]
    public class DeleteModel : PageModel
    {
        public DeleteModel(ApplicationDbContext _dbContext, UserManager<P24IdentityUser> _userManager)
        {
            m_DbContext = _dbContext;
            m_UserManager = _userManager;
        }


        public async Task<IActionResult> OnPostDeleteSingleAsync([FromBody] string _exportId)
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            if (!await this.ValidateModelState(m_DbContext, currentUser, ActionRecord.Operation_.InventoryImportDelete))
                return Content("<div class=\"text-danger font-weight-bold\">" + ErrorMessage.InvalidModelState + "</div>", MediaTypeNames.Text.Html);

            if (!int.TryParse(_exportId, out int exportId))
                return Content("<div class=\"text-danger font-weight-bold\">" + string.Format(P24Message.RecordNotFound, _exportId) + "</div>", MediaTypeNames.Text.Html);

            var export = await (from _export in m_DbContext.DrugOutRecords
                                where _export.Id == exportId
                                select _export)
                         .FirstOrDefaultAsync();

            if (export == null)
                return Content("<div class=\"text-danger font-weight-bold\">" + string.Format(P24Message.RecordNotFound, _exportId) + "</div>", MediaTypeNames.Text.Html);

            int batchId = export.BatchId;

            m_DbContext.Remove(export);

            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.InventoryExportDelete,
                ActionRecord.OperationStatus_.Success,
                JsonSerializer.Serialize(new { Id = _exportId })
            );

            var exports = await (from _export in m_DbContext.DrugOutRecords
                                 where _export.BatchId == batchId
                                 select _export.Id)
                          .ToListAsync();

            if (exports.Count <= 0)
            {
                await DeleteBatchAsync(batchId);

                await m_DbContext.RecordChanges(
                    currentUser.UserName,
                    ActionRecord.Operation_.InventoryExportBatchDelete,
                    ActionRecord.OperationStatus_.Success,
                    JsonSerializer.Serialize(new { Id = batchId })
                );

                string html = "<div class=\"font-weight-bold\">";
                html += "<div>"+ string.Format(P24Message.RecordDeleted, _exportId) + "</div>";
                html += "<div>"+ string.Format(P24Message.BatchDeleted, batchId) + "</div>";
                html += "</div>";
                return Content(html, MediaTypeNames.Text.Html);
            }

            var batch = await this.DrugImport_GetBatchDetailsAsync(m_DbContext, batchId);
            return Partial("_BatchDetails", batch);
        }

        public async Task<IActionResult> OnPostDeleteBatchAsync([FromBody] string _batchId)
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            if (!await this.ValidateModelState(m_DbContext, currentUser, ActionRecord.Operation_.InventoryExportDelete))
                return Content(CustomInfoTag.Error + ErrorMessage.InvalidModelState, MediaTypeNames.Text.Plain);

            if (!int.TryParse(_batchId, out int batchId))
                return Content(CustomInfoTag.Error + "Invalid _batchId: " + _batchId, MediaTypeNames.Text.Plain);

            await DeleteBatchAsync(batchId);

            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.InventoryExportBatchDelete,
                ActionRecord.OperationStatus_.Success,
                JsonSerializer.Serialize(new { Id = batchId })
            );

            return Content("<div class=\"font-weight-bold\">" + string.Format(P24Message.BatchDeleted, _batchId) + "</div>", MediaTypeNames.Text.Html);
        }

        private async Task DeleteBatchAsync(int _batchId)
        {
            var batch = await (from _batch in m_DbContext.DrugOutBatches
                               where _batch.Id == _batchId
                               select _batch)
                        .FirstOrDefaultAsync();

            m_DbContext.Remove(batch);
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
    }

}

/*  P24/Inventory/Import/Delete.cshtml
 *  Version: 1.0 (2023.01.03)
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

namespace Project24.Pages.ClinicManager.Inventory.Import
{
    [Authorize(Roles = P24RoleName.Admin)]
    public class DeleteModel : PageModel
    {
        public DeleteModel(ApplicationDbContext _dbContext, UserManager<P24IdentityUser> _userManager)
        {
            m_DbContext = _dbContext;
            m_UserManager = _userManager;
        }


        public async Task<IActionResult> OnPostDeleteSingleAsync([FromBody] string _importId)
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            if (!await this.ValidateModelState(m_DbContext, currentUser, ActionRecord.Operation_.InventoryImportDelete))
                return Content("<div class=\"text-danger font-weight-bold\">" + ErrorMessage.InvalidModelState + "</div>", MediaTypeNames.Text.Html);

            if (!int.TryParse(_importId, out int importId))
                return Content("<div class=\"text-danger font-weight-bold\">" + string.Format(P24Message.ImportNotFound, _importId) + "</div>", MediaTypeNames.Text.Html);

            var import = await (from _import in m_DbContext.DrugImportations
                                where _import.Id == importId
                                select _import)
                         .FirstOrDefaultAsync();

            if (import == null)
                return Content("<div class=\"text-danger font-weight-bold\">" + string.Format(P24Message.ImportNotFound, _importId) + "</div>", MediaTypeNames.Text.Html);

            int batchId = import.ImportBatchId;

            m_DbContext.Remove(import);

            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.InventoryImportDelete,
                ActionRecord.OperationStatus_.Success,
                JsonSerializer.Serialize(new { Id = _importId })
            );

            var imports = await (from _import in m_DbContext.DrugImportations
                                 where _import.ImportBatchId == batchId
                                 select _import.Id)
                          .ToListAsync();

            if (imports.Count <= 0)
            {
                await DeleteBatchAsync(batchId);

                await m_DbContext.RecordChanges(
                    currentUser.UserName,
                    ActionRecord.Operation_.InventoryImportBatchDelete,
                    ActionRecord.OperationStatus_.Success,
                    JsonSerializer.Serialize(new { Id = batchId })
                );

                string html = "<div class=\"font-weight-bold\">";
                html += "<div>"+ string.Format(P24Message.ImportDeleted, _importId) + "</div>";
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
            if (!await this.ValidateModelState(m_DbContext, currentUser, ActionRecord.Operation_.InventoryImportDelete))
                return Content(CustomInfoTag.Error + ErrorMessage.InvalidModelState, MediaTypeNames.Text.Plain);

            if (!int.TryParse(_batchId, out int batchId))
                return Content(CustomInfoTag.Error + "Invalid _batchId: " + _batchId, MediaTypeNames.Text.Plain);

            await DeleteBatchAsync(batchId);

            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.InventoryImportBatchDelete,
                ActionRecord.OperationStatus_.Success,
                JsonSerializer.Serialize(new { Id = batchId })
            );

            return Content("<div class=\"font-weight-bold\">" + string.Format(P24Message.BatchDeleted, _batchId) + "</div>", MediaTypeNames.Text.Html);
        }

        private async Task DeleteBatchAsync(int _batchId)
        {
            var batch = await (from _batch in m_DbContext.DrugImportBatches
                               where _batch.Id == _batchId
                               select _batch)
                        .FirstOrDefaultAsync();

            m_DbContext.Remove(batch);
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
    }

}

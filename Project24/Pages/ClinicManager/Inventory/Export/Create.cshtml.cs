/*  P24/Inventory/Export/Create.cshtml.cs
 *  Version: 1.1 (2023.02.12)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project24.App;
using Project24.App.Extension;
using Project24.Data;
using Project24.Models;
using Project24.Models.Identity;
using Project24.Models.Inventory.ClinicManager;

namespace Project24.Pages.ClinicManager.Inventory.Export
{
    public class CreateModel : PageModel
    {
        public class TicketQuickView
        {
            public string TicketCode { get; set; }
            public string TicketInfo { get; set; }

            public string CustomerCode { get; set; }
            public string CustomerInfo { get; set; }
        }

        public class AddedDrug
        {
            public string Name { get; set; }
            public string Amount { get; set; }
        }

        public class FormData
        {
            [Required(AllowEmptyStrings = false)]
            public string TicketCode { get; set; }
            public AddedDrug[] Data { get; set; }
        }

        public TicketQuickView Ticket { get; set; }

        public string ReturnUrl { get; set; }

        public CreateModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager, ILogger<CreateModel> _logger)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
            //m_Logger = _logger;
        }


        public async Task<IActionResult> OnGetAsync(string _ticketCode, string _returnUrl)
        {
            if (string.IsNullOrEmpty(_ticketCode))
                return BadRequest();

            if (string.IsNullOrEmpty(_returnUrl))
                _returnUrl = Url.Content("~/ClinicManager/Inventory/List");

            ReturnUrl = _returnUrl;

            if (_ticketCode == P24ExportType_.Common || _ticketCode == P24ExportType_.Dump)
            {
                ViewData["ExportType"] = _ticketCode;
                return Page();
            }

            var ticket = await (from _ticket in m_DbContext.TicketProfiles.Include(_t => _t.Customer)
                                where _ticket.Code == _ticketCode && _ticket.DeletedDate == DateTime.MinValue
                                select _ticket)
                         .FirstOrDefaultAsync();

            if (ticket == null)
                return BadRequest();

            string customerGender = AppUtils.NormalizeGenderString(ticket.Customer.Gender);
            int customerAge = DateTime.Now.Year - ticket.Customer.DateOfBirth;
            Ticket = new TicketQuickView
            {
                TicketCode = _ticketCode,
                CustomerCode = ticket.Customer.Code,
                CustomerInfo = ticket.Customer.FullName + " (" + customerGender + ", " + customerAge + "t)",
                TicketInfo = ticket.Diagnose + ": " + ticket.ProposeTreatment
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync([FromBody] FormData _formData)
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            if (!await this.ValidateModelState(m_DbContext, currentUser, ActionRecord.Operation_.InventoryImportCreate))
                return Content(CustomInfoTag.Error + ErrorMessage.InvalidModelState, MediaTypeNames.Text.Plain);

            if (_formData.Data.Length <= 0)
            {
                if (User.IsInRole(P24RoleName.Admin))
                    return await CreateEmptyBatch(_formData, currentUser);

                return Content(CustomInfoTag.Error + "List Empty", MediaTypeNames.Text.Plain);
            }

            DrugOutBatch batch = await GetOrCreateDrugOutBatch(_formData.TicketCode, currentUser);
            if (batch == null)
                return Content(CustomInfoTag.Error + "Ticket " + _formData.TicketCode + " not found", MediaTypeNames.Text.Plain);

            var drugs = await (from _drug in m_DbContext.Drugs
                               select _drug)
                        .ToDictionaryAsync(_d => _d.Name);

            List<Drug> drugUpdateList = new List<Drug>();
            List<DrugOutRecord> exportAddList = new List<DrugOutRecord>();

            foreach (var data in _formData.Data)
            {
                if (!int.TryParse(data.Amount, out int amount))
                {
                    return Content(CustomInfoTag.Error + "Invalid data: { " + data.Name + ", " + data.Amount + " }", MediaTypeNames.Text.Plain);
                }

                if (!drugs.ContainsKey(data.Name))
                {
                    return Content(CustomInfoTag.Error + "Drug " + data.Name + " not found", MediaTypeNames.Text.Plain);
                }

                Drug drug = drugs[data.Name];
                drug.Amount -= amount;
                drugUpdateList.Add(drug);

                DrugOutRecord exportation = new DrugOutRecord(batch, drug, amount);
                exportAddList.Add(exportation);
            }

            m_DbContext.UpdateRange(drugUpdateList);
            await m_DbContext.AddRangeAsync(exportAddList);

            if ((batch.ExportType == P24ExportType_.Common || batch.ExportType == P24ExportType_.Dump) && batch.Id != 0)
                m_DbContext.Update(batch);
            else
                await m_DbContext.AddAsync(batch);

            var jsonEncoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            var json = JsonSerializer.Serialize(_formData.Data, new JsonSerializerOptions() { Encoder = jsonEncoder });

            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.InventoryExportCreate,
                ActionRecord.OperationStatus_.Success,
                new Dictionary<string, string>()
                {
                    { CustomInfoKey.TicketCode, _formData.TicketCode },
                    { "data", json }
                }
            );

            return Content(CustomInfoTag.Success, MediaTypeNames.Text.Plain);
        }

        private async Task<IActionResult> CreateEmptyBatch(FormData _formData, P24IdentityUser _currentUser)
        {
            DrugOutBatch batch = await GetOrCreateDrugOutBatch(_formData.TicketCode, _currentUser);
            if (batch == null)
                return Content(CustomInfoTag.Error + "Ticket " + _formData.TicketCode + " not found", MediaTypeNames.Text.Plain);

            if (batch.Id != 0)
                m_DbContext.Update(batch);
            else
                await m_DbContext.AddAsync(batch);

            await m_DbContext.RecordChanges(
                _currentUser.UserName,
                ActionRecord.Operation_.InventoryExportCreate,
                ActionRecord.OperationStatus_.Success,
                new Dictionary<string, string>()
                {
                    { CustomInfoKey.TicketCode, _formData.TicketCode },
                }
            );

            return Content(CustomInfoTag.Success, MediaTypeNames.Text.Plain);
        }

        private async Task<DrugOutBatch> GetOrCreateDrugOutBatch(string _exportType, P24IdentityUser _currentUser)
        {
            if (_exportType == P24ExportType_.Common || _exportType == P24ExportType_.Dump)
            {
                var batch = await (from _batch in m_DbContext.DrugOutBatches
                                   where _batch.ExportType == _exportType
                                    && _batch.AddedDate.Year == DateTime.Now.Year
                                    && _batch.AddedDate.Month == DateTime.Now.Month
                                   select _batch)
                            .FirstOrDefaultAsync();

                if (batch != null)
                    return batch;

                return new DrugOutBatch(_currentUser) { ExportType = _exportType };
            }

            var ticket = await (from _ticket in m_DbContext.TicketProfiles
                                where _ticket.Code == _exportType
                                select _ticket)
                         .FirstOrDefaultAsync();

            if (ticket == null)
                return null;

            return new DrugOutBatch(_currentUser, ticket) { ExportType = P24ExportType_.Ticket };
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
    }

}

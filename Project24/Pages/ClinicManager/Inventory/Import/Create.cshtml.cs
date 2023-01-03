/*  P24/Inventory/Import/Create.cshtml.cs
 *  Version: 1.1 (2023.01.03)
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project24.App.Extension;
using Project24.Data;
using Project24.Models;
using Project24.Models.Identity;
using Project24.Models.Internal.ClinicManager;

namespace Project24.Pages.ClinicManager.Inventory.Import
{
    [Authorize(Roles = P24RoleName.Manager)]
    public class CreateModel : PageModel
    {
        public class AddedDrug
        {
            public string Name { get; set; }
            public string Amount { get; set; }
            public string Unit { get; set; }

            public AddedDrug()
            { }
        }

        public string DummyString { get; set; }

        [Range(1.0, double.PositiveInfinity)]
        public int Amount { get; private set; }


        public CreateModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager, ILogger<CreateModel> _logger)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
            //m_Logger = _logger;
        }


        public async Task<IActionResult> OnPostAsync([FromBody] AddedDrug[] _data)
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            if (!await this.ValidateModelState(m_DbContext, currentUser, ActionRecord.Operation_.InventoryImportCreate))
                return Content(CustomInfoTag.Error + ErrorMessage.InvalidModelState, MediaTypeNames.Text.Plain);

            if (_data.Length <= 0)
                return Content(CustomInfoTag.Error + "List Empty", MediaTypeNames.Text.Plain);

            var drugs = await (from _drug in m_DbContext.Drugs
                               select _drug)
                        .ToDictionaryAsync(_d => _d.Name);

            DrugImportBatch batch = new DrugImportBatch(currentUser);

            List<Drug> drugUpdateList = new List<Drug>();
            List<Drug> drugAddList = new List<Drug>();
            List<DrugImportation> importAddList = new List<DrugImportation>();

            foreach (var data in _data)
            {
                if (!int.TryParse(data.Amount, out int amount))
                {
                    // TODO: log error here;
                    continue;
                }

                Drug drug = null;
                if (drugs.ContainsKey(data.Name))
                {
                    drug = drugs[data.Name];
                    drug.Amount += amount;

                    drugUpdateList.Add(drug);
                }
                else
                {
                    drug = new Drug()
                    {
                        Amount = amount,
                        Name = data.Name,
                        Unit = data.Unit
                    };

                    drugAddList.Add(drug);
                }

                DrugImportation importation = new DrugImportation(batch, drug, amount);
                importAddList.Add(importation);
            }

            if (drugAddList.Count > 0)
                await m_DbContext.AddRangeAsync(drugAddList);
            if (drugUpdateList.Count > 0)
                m_DbContext.UpdateRange(drugUpdateList);

            await m_DbContext.AddRangeAsync(importAddList);
            await m_DbContext.AddAsync(batch);

            var jsonEncoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            var json = JsonSerializer.Serialize(_data, new JsonSerializerOptions() { Encoder = jsonEncoder });

            await m_DbContext.RecordChanges(
                currentUser.UserName,
                ActionRecord.Operation_.InventoryImportCreate,
                ActionRecord.OperationStatus_.Success,
                json
            );

            return Content(CustomInfoTag.Success, MediaTypeNames.Text.Plain);
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
    }

}

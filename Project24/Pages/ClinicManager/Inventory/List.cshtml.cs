/*  P24/Inventory/List.cshtml.cs
 *  Version: 1.3 (2023.02.13)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
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
using Project24.App;
using Project24.App.Extension;
using Project24.Data;
using Project24.Models;
using Project24.Models.Identity;
using Project24.Models.Inventory.ClinicManager;

namespace Project24.Pages.ClinicManager.Inventory
{
    [Authorize(Roles = P24RoleName.Manager)]
    public class ListModel : PageModel
    {
        public class StorageDrugViewModel
        {
            public Drug Drug { get; set; }

            public bool NoData { get; set; } = false;

            public StorageDrugViewModel()
            { }
        }

        public string DrugNameFilter { get; private set; }

        public List<StorageDrugViewModel> DrugListings { get; private set; }

        public bool IsSearchMode { get; private set; } = false;


        public ListModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
        }


        public async Task OnGetAsync()
        {
            var drugs = await (from _drug in m_DbContext.Drugs
                               orderby _drug.Name
                               select new StorageDrugViewModel()
                               {
                                   Drug = _drug
                               })
                        .ToListAsync();

            DrugListings = drugs;
        }

        public async Task OnGetSearchAsync(string _name)
        {
            if (_name == null)
                _name = "";

            var drugs = await (from _drug in m_DbContext.Drugs
                               where _drug.Name.Contains(_name, StringComparison.OrdinalIgnoreCase)
                               orderby _drug.Name
                               select new StorageDrugViewModel()
                               {
                                   Drug = _drug
                               })
                        .ToListAsync();

            DrugListings = drugs;
            DrugNameFilter = _name;

            IsSearchMode = true;
        }

        // ajax call only;
        public async Task<IActionResult> OnGetFetchAvailDrugsInfoAsync()
        {
            var drugs = await (from _drug in m_DbContext.Drugs
                               orderby _drug.Name
                               select new
                               {
                                   _drug.Name,
                                   _drug.Unit
                               })
                        .ToListAsync();

            var jsonEncoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            string json = JsonSerializer.Serialize(drugs, new JsonSerializerOptions() { Encoder = jsonEncoder });

            return Content(CustomInfoTag.Success + json, MediaTypeNames.Text.Plain);
        }

        // ajax call only;
        public async Task<IActionResult> OnGetFetchAvailDrugsInfoWithAmountAsync()
        {
            var drugs = await (from _drug in m_DbContext.Drugs
                               orderby _drug.Name
                               select new
                               {
                                   _drug.Name,
                                   _drug.Amount,
                                   _drug.Unit
                               })
                        .ToListAsync();

            var jsonEncoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            string json = JsonSerializer.Serialize(drugs, new JsonSerializerOptions() { Encoder = jsonEncoder });

            return Content(CustomInfoTag.Success + json, MediaTypeNames.Text.Plain);
        }

        // ajax call only;
        public async Task<IActionResult> OnPostHideDrug(int _drugId, bool _hidden)
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            if (!await this.ValidateModelState(m_DbContext, currentUser, ActionRecord.Operation_.HideUnhideDrug))
                return Content(CustomInfoTag.Error + ErrorMessage.InvalidModelState, MediaTypeNames.Text.Plain);

            var drug = await (from _drug in m_DbContext.Drugs
                              where _drug.Id == _drugId
                              select _drug)
                       .FirstOrDefaultAsync();

            if (drug == null)
                return Content(CustomInfoTag.Error + string.Format(P24Message.RecordNotFound, _drugId), MediaTypeNames.Text.Plain);

            drug.Hidden = _hidden;
            m_DbContext.Update(drug);
            await m_DbContext.SaveChangesAsync();

            return Content(CustomInfoTag.Success, MediaTypeNames.Text.Plain);
        }

        // ajax call only;
        public async Task<IActionResult> OnPostValidateStorageAsync()
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            if (!await this.ValidateModelState(m_DbContext, currentUser, ActionRecord.Operation_.ValidateDrugStorage))
                return Content(CustomInfoTag.Error + ErrorMessage.InvalidModelState, MediaTypeNames.Text.Plain);

            if (BackingObject.IsP24StorageValidationInProgress)
                return Content(CustomInfoTag.Info + "Storage validation in progress, please try again in a few minutes..", MediaTypeNames.Text.Plain);

            BackingObject.IsP24StorageValidationInProgress = true;

            var importations = await (from _record in m_DbContext.DrugInRecords
                                      group _record by _record.DrugId into _group
                                      select new
                                      {
                                          Id = _group.Key,
                                          Amount = _group.Sum(_im => _im.Amount)
                                      })
                               .ToDictionaryAsync(_im => _im.Id, _im => _im.Amount);

            var exportations = await (from _record in m_DbContext.DrugOutRecords
                                      group _record by _record.DrugId into _group
                                      select new
                                      {
                                          Id = _group.Key,
                                          Amount = _group.Sum(_ex => _ex.Amount)
                                      })
                               .ToDictionaryAsync(_ex => _ex.Id, _ex => _ex.Amount);

            var drugs = await (from _drug in m_DbContext.Drugs
                               where _drug.DeletedDate == DateTime.MinValue
                               select _drug)
                        .ToListAsync();

            List<StorageDrugViewModel> list = new List<StorageDrugViewModel>();
            List<Drug> updateList = new List<Drug>();
            foreach (var drug in drugs)
            {
                StorageDrugViewModel drugView = new StorageDrugViewModel()
                {
                    Drug = drug
                };

                drug.Amount = 0;

                bool hasImport = importations.ContainsKey(drug.Id);
                bool hasExport = exportations.ContainsKey(drug.Id);
                if (!hasImport)
                {
                    if (!hasExport)
                    {
                        drugView.NoData = true;
                    }

                    // export > import???;
                    drugView.NoData = true;
                }

                if (hasImport)
                {
                    drug.Amount += importations[drug.Id];
                }

                if (hasExport)
                {
                    drug.Amount -= exportations[drug.Id];
                }

                updateList.Add(drug);
                list.Add(drugView);
            }

            m_DbContext.UpdateRange(updateList);
            await m_DbContext.SaveChangesAsync();

            //drugs.Sort((StorageDrugViewModel _m1, StorageDrugViewModel _m2) =>
            //{
            //    return _m1.Drug.Name.CompareTo(_m2.Drug.Name);
            //});

            BackingObject.IsP24StorageValidationInProgress = false;

            return Content(CustomInfoTag.Success, MediaTypeNames.Text.Plain);
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
    }

}

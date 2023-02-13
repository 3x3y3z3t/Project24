/*  P24/Inventory/Report/Monthly/List.cshtml.cs
 *  Version: 1.0 (2023.02.13)
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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project24.Data;
using Project24.Models.Identity;

namespace Project24.Pages.ClinicManager.Inventory.Report.Monthly
{
    public class ListModel : PageModel
    {
        public class ReportDrugListingModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Unit { get; set; }
            public string Note { get; set; }

            public int AmountIn { get; set; } = 0;
            public int AmountOut { get; set; } = 0;

            public ReportDrugListingModel()
            { }
        }

        public int[] SelectableYears { get; private set; }

        public string DrugNameFilter { get; private set; }

        public bool IsSearchMode { get; private set; } = false;


        public ListModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
        }


        public async Task OnGetAsync()
        {
            List<int> list = await GetSelectableYears();
            SelectableYears = list.OrderByDescending(_year => _year).ToArray();

        }

        // ajax call only;
        public async Task<IActionResult> OnGetFetchReportData(int _year, int _month)
        {
            var drugIns = await (from _record in m_DbContext.DrugInRecords.Include(_i => _i.Drug).Include(_i => _i.InBatch)
                                 where _record.InBatch.AddedDate.Year == _year && _record.InBatch.AddedDate.Month == _month
                                 group _record by new { _record.DrugId, _record.Drug.Name, _record.Drug.Unit, _record.Drug.Note } into _group
                                 select new
                                 {
                                     _group.Key.DrugId,
                                     _group.Key.Name,
                                     _group.Key.Unit,
                                     _group.Key.Note,
                                     Amount = _group.Sum(_r => _r.Amount)
                                 })
                          .ToDictionaryAsync(_r => _r.DrugId, _r => _r);

            var drugOuts = await (from _record in m_DbContext.DrugOutRecords.Include(_i => _i.Drug).Include(_i => _i.OutBatch)
                                  where _record.OutBatch.AddedDate.Year == _year && _record.OutBatch.AddedDate.Month == _month
                                  group _record by new { _record.DrugId, _record.Drug.Name, _record.Drug.Unit, _record.Drug.Note } into _group
                                  select new
                                  {
                                      _group.Key.DrugId,
                                      _group.Key.Name,
                                      _group.Key.Unit,
                                      _group.Key.Note,
                                      Amount = _group.Sum(_r => _r.Amount)
                                  })
                          .ToDictionaryAsync(_r => _r.DrugId, _r => _r);

            Dictionary<int, ReportDrugListingModel> dictionary = new Dictionary<int, ReportDrugListingModel>();

            foreach (var pair in drugIns)
            {
                if (dictionary.ContainsKey(pair.Key))
                {
                    dictionary[pair.Key].AmountIn += pair.Value.Amount;
                }
                else
                {
                    dictionary[pair.Key] = new ReportDrugListingModel()
                    {
                        Id = pair.Value.DrugId,
                        Name = pair.Value.Name,
                        Unit = pair.Value.Unit,
                        Note = pair.Value.Note,
                        AmountIn = pair.Value.Amount
                    };
                }
            }

            foreach (var pair in drugOuts)
            {
                if (dictionary.ContainsKey(pair.Key))
                {
                    dictionary[pair.Key].AmountOut += pair.Value.Amount;
                }
                else
                {
                    dictionary[pair.Key] = new ReportDrugListingModel()
                    {
                        Id = pair.Value.DrugId,
                        Name = pair.Value.Name,
                        Unit = pair.Value.Unit,
                        Note = pair.Value.Note,
                        AmountOut = pair.Value.Amount
                    };
                }
            }

            ReportDrugListingModel[] listing = dictionary.Values.OrderBy(_item => _item.Id).ToArray();

            var jsonEncoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            string json = JsonSerializer.Serialize(listing, new JsonSerializerOptions() { Encoder = jsonEncoder });

            return Content(CustomInfoTag.Success + json, MediaTypeNames.Text.Plain);
        }

        //public async Task<IActionResult> OnGetUpdateSelectableYears()
        //{
        //    List<int> years = await GetSelectableYears();

        //    var jsonEncoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        //    string json = JsonSerializer.Serialize(years.OrderByDescending(_year => _year).ToArray(), new JsonSerializerOptions() { Encoder = jsonEncoder });

        //    return Content(CustomInfoTag.Success + json, MediaTypeNames.Text.Plain);
        //}

        // ajax call only;
        public async Task<IActionResult> OnGetFetchSelectableMonths(int _year)
        {
            List<int> months = await GetSelectableMonths(_year);

            var jsonEncoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            string json = JsonSerializer.Serialize(months.OrderByDescending(_month => _month).ToArray(), new JsonSerializerOptions() { Encoder = jsonEncoder });

            return Content(CustomInfoTag.Success + json, MediaTypeNames.Text.Plain);
        }

        private async Task<List<int>> GetSelectableYears()
        {
            var queryDrugInCap = from _cap in m_DbContext.DrugInBatches
                                 orderby _cap.AddedDate
                                 select _cap.AddedDate;

            var queryDrugOutCap = from _cap in m_DbContext.DrugOutBatches
                                  orderby _cap.AddedDate
                                  select _cap.AddedDate;

            DateTime minInDate = await queryDrugInCap.FirstOrDefaultAsync();
            DateTime maxInDate = await queryDrugInCap.LastOrDefaultAsync();

            DateTime minOutDate = await queryDrugOutCap.FirstOrDefaultAsync();
            DateTime maxOutDate = await queryDrugOutCap.LastOrDefaultAsync();


            DateTime minDate = DateTime.Now;
            DateTime maxDate = AppConfig.AppBeginDate;

            if (minInDate > AppConfig.AppBeginDate && minOutDate > AppConfig.AppBeginDate)
            {
                if (minDate > minInDate)
                    minDate = minInDate;

                if (minDate > minOutDate)
                    minDate = minOutDate;
            }
            else
            {
                minDate = AppConfig.AppBeginDate;
            }

            if (maxDate < maxInDate)
                maxDate = maxInDate;

            if (maxDate < maxOutDate)
                maxDate = maxOutDate;

            List<int> years = new List<int>();
            for (int year = minDate.Year; year <= maxDate.Year; ++year)
                years.Add(year);

            return years;
        }

        private async Task<List<int>> GetSelectableMonths(int _year)
        {
            var queryDrugInCap = from _cap in m_DbContext.DrugInBatches
                                 where _cap.AddedDate.Year == _year
                                 orderby _cap.AddedDate
                                 select _cap.AddedDate;

            var queryDrugOutCap = from _cap in m_DbContext.DrugOutBatches
                                  where _cap.AddedDate.Year == _year
                                  orderby _cap.AddedDate
                                  select _cap.AddedDate;

            DateTime minInDate = await queryDrugInCap.FirstOrDefaultAsync();
            DateTime maxInDate = await queryDrugInCap.LastOrDefaultAsync();

            DateTime minOutDate = await queryDrugOutCap.FirstOrDefaultAsync();
            DateTime maxOutDate = await queryDrugOutCap.LastOrDefaultAsync();


            DateTime minDate = DateTime.Now;
            DateTime maxDate = AppConfig.AppBeginDate;

            if (minInDate > AppConfig.AppBeginDate && minOutDate > AppConfig.AppBeginDate)
            {
                if (minDate > minInDate)
                    minDate = minInDate;

                if (minDate > minOutDate)
                    minDate = minOutDate;
            }
            else
            {
                minDate = AppConfig.AppBeginDate;
            }

            if (maxDate < maxInDate)
                maxDate = maxInDate;

            if (maxDate < maxOutDate)
                maxDate = maxOutDate;

            List<int> months = new List<int>();
            for (int month = minDate.Month; month <= maxDate.Month; ++month)
                months.Add(month);

            return months;
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
    }

}

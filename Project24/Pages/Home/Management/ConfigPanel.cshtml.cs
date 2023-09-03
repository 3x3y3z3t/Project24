/*  Home/Management/ConfigPanel.cshtml.cs
 *  Version: v1.0 (2023.09.02)
 *  
 *  Author
 *      Arime-chan
 */


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using Project24.App.Services;
using Project24.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Project24.SerializerContext;
using Project24.App;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Project24.Pages.Home.Management
{
    public class ConfigPanelModel : PageModel
    {
        #region View Model
        public class ConfigViewModel
        {
            public string Key { get; set; }
            public string Value { get; set; }

            public string TabName { get; set; }
            public string SectionName { get; set; }


            public string ValueType { get; set; }
            public string ValueRangeType { get; set; }
            public string[] ValueRange { get; set; }

        }

        #endregion

        public Dictionary<string, List<ConfigViewModel>> ConfigItem { get; set; }

        public Dictionary<string, string> TrackedValues { get; private set; }


        public ConfigPanelModel(ApplicationDbContext _dbContext, LocalizationSvc _localizationSvc, InternalTrackerSvc _trackerSvc)
        {
            m_DbContext = _dbContext;
            m_LocalizationSvc = _localizationSvc;
            m_TrackerSvc = _trackerSvc;




        }


        public void OnGet()
        { }

        public IActionResult OnGetFetchPageData()
        {
            var configs = (from _trackable in m_DbContext.Trackables.Include(_x => _x.Metadata)
                           where _trackable.Key.StartsWith(InternalTrackedKeys.CONFIG_)
                           select new ConfigViewModel()
                           {
                               Key = _trackable.Key,
                               Value = _trackable.Value,
                               TabName = _trackable.Metadata.ValueDisplayTab,
                               SectionName = _trackable.Metadata.ValueDisplaySection,
                               ValueType = _trackable.Metadata.ValueType,
                               ValueRangeType = _trackable.Metadata.ValueRangeType,
                               ValueRange = new string[] { _trackable.Metadata.ValueRangeAsString }
                               //ValueRange = _trackable.Metadata.ValueRangeAsString,
                           })
                          .ToList();

            Dictionary<string, List<ConfigViewModel>> data = new();

            foreach (var item in configs)
            {
                if (item.ValueRange[0] != null)
                {
                    item.ValueRange = JsonSerializer.Deserialize(item.ValueRange[0], P24JsonSerializerContext.Default.StringArray);
                }

                if (!data.ContainsKey(item.TabName))
                {
                    data[item.TabName] = new();
                }

                data[item.TabName].Add(item);
            }

            string dataJson = JsonSerializer.Serialize(configs);
            return Content(MessageTag.Success + dataJson, MediaTypeNames.Text.Plain);
        }

        public async Task<IActionResult> OnPostSubmitChangesAsync([FromBody] List<KeyValuePair<string, string>> _data)
        {
            if (!ModelState.IsValid)
                return Content(MessageTag.Error + "Invalid ModelState (submit changes)", MediaTypeNames.Text.Plain);

            ValidateSubmittedData(_data);

            List<string> changesList = new();
            foreach (var pair in _data)
            {
                if (string.IsNullOrEmpty(pair.Value))
                    continue;
                if (m_TrackerSvc[pair.Key] == pair.Value)
                    continue;

                // TODO: peform data validation;






                m_TrackerSvc[pair.Key] = pair.Value;
                changesList.Add(pair.Key);
            }

            string dataJson = null;
            if (changesList.Count > 0)
            {
                await m_TrackerSvc.SaveChangesAsync(m_DbContext);
                dataJson = JsonSerializer.Serialize(changesList);
            }

            return Content(MessageTag.Success + dataJson, MediaTypeNames.Text.Plain);
        }

        private void ValidateSubmittedData(List<KeyValuePair<string, string>> _data)
        {

        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly LocalizationSvc m_LocalizationSvc;
        private readonly InternalTrackerSvc m_TrackerSvc;
    }

}

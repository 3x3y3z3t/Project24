/*  ExtensionMethods.cs
 *  Version: 1.3 (2023.02.14)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project24.Data;
using Project24.Models;
using Project24.Models.ClinicManager.DataModel;
using Project24.Models.Identity;

namespace Project24.App.Extension
{
    public static class ExtensionMethods
    {
        public static async Task<bool> ValidateModelState(this PageModel _page, ApplicationDbContext _dbContext, P24IdentityUser _currentUser, string _operation)
        {
            if (_page.ModelState.IsValid)
                return true;

            await _dbContext.RecordChanges(
                _currentUser.UserName,
                _operation,
                ActionRecord.OperationStatus_.Failed,
                new Dictionary<string, string>()
                {
                    { CustomInfoKey.Error, ErrorMessage.InvalidModelState }
                }
            );

            return false;
        }

        public static async Task RecordChanges(this ApplicationDbContext _dbContext,
            string _username,
            string _operation,
            string _status,
            Dictionary<string, string> _customInfo = null)
        {
            string json = null;
            if (_customInfo != null)
            {
                var jsonEncoder = JavaScriptEncoder.Create(UnicodeRanges.All);
                json = JsonSerializer.Serialize(_customInfo, new JsonSerializerOptions() { Encoder = jsonEncoder });
            }

            await _dbContext.RecordChanges(_username, _operation, _status, json);
        }

        public static async Task RecordChanges(this ApplicationDbContext _dbContext,
            string _username,
            string _operation,
            string _status,
            string _customInfo)
        {
            ActionRecord record = new ActionRecord()
            {
                Timestamp = DateTime.Now,
                Username = _username,
                Operation = _operation,
                OperationStatus = _status,
                CustomInfo = _customInfo
            };
            await _dbContext.AddAsync(record);
            await _dbContext.SaveChangesAsync();
        }



        public static async Task<ImportExportBatchViewModel> DrugImport_GetBatchDetailsAsync(this PageModel _page, ApplicationDbContext _dbContext, int _batchId)
        {
            var batch = await (from _batch in _dbContext.DrugInBatches.Include(_b => _b.AddedUser)
                               where _batch.Id == _batchId
                               select new ImportExportBatchViewModel()
                               {
                                   Id = _batch.Id,
                                   AddedUserName = _batch.AddedUser.UserName,
                                   AddedDate = _batch.AddedDate,
                                   Type = P24ImportExportType.Import
                               })
                          .FirstOrDefaultAsync();

            if (batch == null)
                return null;

            var importations = await (from _importation in _dbContext.DrugInRecords.Include(_im => _im.Drug)
                                      where _importation.BatchId == _batchId
                                      select new ImportExportQuickViewModel()
                                      {
                                          Id = _importation.Id,
                                          Name = _importation.Drug.Name,
                                          Amount = _importation.Amount,
                                          Unit = _importation.Drug.Unit
                                      })
                               .ToListAsync();

            batch.List = importations;

            return batch;
        }

        public static async Task<ImportExportBatchViewModel> DrugExport_GetBatchDetailsAsync(this PageModel _page, ApplicationDbContext _dbContext, int _batchId)
        {
            var batch = await (from _batch in _dbContext.DrugOutBatches.Include(_b => _b.Ticket).Include(_b => _b.AddedUser)
                               where _batch.Id == _batchId
                               select new ImportExportBatchViewModel()
                               {
                                   Id = _batch.Id,
                                   AddedUserName = _batch.AddedUser.UserName,
                                   AddedDate = _batch.AddedDate,
                                   Type = P24ImportExportType.Export,
                                   ExportType = _batch.ExportType,
                                   TicketCode = _batch.Ticket.Code
                               })
                          .FirstOrDefaultAsync();

            if (batch == null)
                return null;

            var exportations = await (from _exportation in _dbContext.DrugOutRecords.Include(_im => _im.Drug)
                                      where _exportation.BatchId == _batchId
                                      select new ImportExportQuickViewModel()
                                      {
                                          Id = _exportation.Id,
                                          Name = _exportation.Drug.Name,
                                          Amount = _exportation.Amount,
                                          Unit = _exportation.Drug.Unit
                                      })
                               .ToListAsync();

            batch.List = exportations;

            return batch;
        }

        public static async Task<List<P24ImageViewModel>> FetchCustomerImages(this Project24.Pages.ClinicManager.ImageManagerModel _page,
                                                                         ApplicationDbContext _dbContext, string _customerCode)
        {
            return await FetchCustomerImagesInternal(_dbContext, _customerCode);
        }

        public static async Task<List<P24ImageViewModel>> FetchTicketImages(this Project24.Pages.ClinicManager.ImageManagerModel _page,
                                                                         ApplicationDbContext _dbContext, string _ticketCode)
        {
            return await FetchTicketImagesInternal(_dbContext, _ticketCode);
        }



        private static async Task<List<P24ImageViewModel>> FetchCustomerImagesInternal(ApplicationDbContext _dbContext, string _customerCode)
        {
            var images = from _image in _dbContext.CustomerImages.Include(_i => _i.OwnerCustomer)
                         where _image.OwnerCustomer.Code == _customerCode && _image.DeletedDate == DateTime.MinValue
                         select new P24ImageViewModel()
                         {
                             Id = _image.Id,
                             Path = _image.Path,
                             Name = _image.Name
                         };
            return await images.ToListAsync();
        }

        private static async Task<List<P24ImageViewModel>> FetchTicketImagesInternal(ApplicationDbContext _dbContext, string _ticketCode)
        {
            var images = from _image in _dbContext.TicketImages.Include(_i => _i.OwnerTicket)
                         where _image.OwnerTicket.Code == _ticketCode && _image.DeletedDate == DateTime.MinValue
                         select new P24ImageViewModel()
                         {
                             Id = _image.Id,
                             Path = _image.Path,
                             Name = _image.Name
                         };
            return await images.ToListAsync();
        }


    }

}

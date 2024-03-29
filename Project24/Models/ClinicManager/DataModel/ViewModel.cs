/*  ViewModel.cs
 *  Version: 1.6 (2023.02.12)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Project24.App;

namespace Project24.Models.ClinicManager.DataModel
{
    public class P24CustomerDetailsViewModel
    {
        public string Code { get; set; }

        public string Fullname { get; set; }

        public string Gender { get; set; }

        public int DoB { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [DataType(DataType.MultilineText)]
        public string Address { get; set; }

        [DataType(DataType.MultilineText)]
        public string Note { get; set; }

        public DateTime DeletedDate { get; set; }

        public P24CustomerDetailsViewModel()
        { }
    }

    public class P24CustomerDetailsViewModelEx : P24CustomerDetailsViewModel
    {
        public DateTime AddedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        public string AddedUserName { get; set; }

        public string UpdatedUserName { get; set; }

        public List<P24TicketDetailsViewModel> Tickets { get; set; }

        public P24CustomerDetailsViewModelEx()
            : base()
        { }
    }

    public class P24TicketDetailsViewModel
    {
        public string Code { get; set; }

        public int? DrugExportBatchId { get; set; }

        public string Symptom { get; set; }

        public string Diagnose { get; set; }

        public string Treatment { get; set; }

        [DataType(DataType.MultilineText)]
        public string Note { get; set; }

        public DateTime AddedDate { get; set; }

        public P24TicketDetailsViewModel()
        { }
    }

    public class P24TicketDetailsViewModelEx : P24TicketDetailsViewModel
    {
        public DateTime UpdatedDate { get; set; }

        public DateTime DeletedDate { get; set; }

        public string AddedUserName { get; set; }

        public string UpdatedUserName { get; set; }

        public P24CustomerDetailsViewModel Customer { get; set; }

        public P24TicketDetailsViewModelEx()
            : base()
        { }
    }

    public class P24ImageViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public P24ImageViewModel()
        { }
    }

    public class P24ImageListingModel
    {
        public P24Module Module { get; set; }

        public string OwnerCode { get; set; }

        public bool IsReadonly { get; set; } = false;

        public List<P24ImageViewModel> Images { get; set; }

        public P24CreateImageFormDataModel _formData { get; }

        public P24ImageListingModel()
        { }
    }

    public class ImportExportQuickViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; }
        public string Unit { get; set; }
        public P24ImportExportType Type { get; set; }
    }

    public class ImportExportBatchViewModel
    {
        public int Id { get; set; }
        public string AddedUserName { get; set; }
        public DateTime AddedDate { get; set; }
        public P24ImportExportType Type { get; set; }

        /// <summary> Only if this is an Export Batch. </summary>
        public string ExportType { get; set; }
        public string TicketCode { get; set; }

        public List<ImportExportQuickViewModel> List { get; set; }
    }

}

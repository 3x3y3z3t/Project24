/*  ViewModel.cs
 *  Version: 1.3 (2023.01.02)
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

        List<P24TicketDetailsViewModel> Tickets { get; set; }

        public P24CustomerDetailsViewModelEx()
            : base()
        { }
    }

    public class P24TicketDetailsViewModel
    {
        public string Code { get; set; }

        public string Symptom { get; set; }

        public string Diagnose { get; set; }

        public string Treatment { get; set; }

        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

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

}

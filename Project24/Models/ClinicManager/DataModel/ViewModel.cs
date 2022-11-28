/*  ViewModel.cs
 *  Version: 1.0 (2022.11.28)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        public string Notes { get; set; }

        public DateTime AddedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        public DateTime DeletedDate { get; set; }

        public string AddedUserName { get; set; }

        public string UpdatedUserName { get; set; }

        // TODO: Ticket


        public P24CustomerDetailsViewModel()
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
        public bool IsReadonly { get; set; } = false;

        public string CustomerCode { get; set; }

        public List<P24ImageViewModel> Images { get; set; }

        public P24CreateImageFormDataModel _formData { get; }

        public P24ImageListingModel()
        { }
    }

}

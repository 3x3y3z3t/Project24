/*  P24/DataModel/FormDataModel.cs
 *  Version: 1.0 (2023.01.02)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace Project24.Models.ClinicManager.DataModel
{
    public class QuickSearchFormDataModel
    {
        public string Name { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }


        public QuickSearchFormDataModel()
        { }
    }
}

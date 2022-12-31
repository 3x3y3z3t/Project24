/*  Drug.cs
 *  Version: 1.0 (2022.12.29)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace Project24.Models.Internal.ClinicManager
{
    public class Drug
    {
        [Key]
        public int Id { get; protected set; }


        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Unit { get; set; }

        [DataType(DataType.MultilineText)]
        public string Note { get; set; }

        public int? Amount { get; set; }

        public DateTime DeletedDate { get; set; } = DateTime.MinValue;


        public Drug()
        { }
    }

}

/*  ServiceDev.cs
 *  Version: 1.0 (2022.09.08)
 *
 *  Contributor
 *      Arime-chan
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Project24.Models
{
    public class ServiceDev
    {
        public int Id { get; set; }

        public string ServiceCode { get; set; }

        public string Name { get; set; }

        [DataType(DataType.Currency)]
        public float Price { get; set; }

        [Required(ErrorMessage = Constants.ERROR_EMPTY_DATETIME)]
        [DataType(DataType.DateTime)]
        public DateTime AddedDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime UpdatedDate { get; set; }





        public ServiceDev()
        { }

    }

}

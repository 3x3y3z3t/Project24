/*  VisitingProfileDev.cs
 *  Version: 1.0 (2022.09.08)
 *
 *  Contributor
 *      Arime-chan
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Project24.Models
{
    public class VisitingProfileDev
    {

        public int Id { get; set; }

        [ForeignKey("Customer")]
        [Required(ErrorMessage = Constants.ERROR_EMPTY_CUSTOMER_ID)]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = Constants.ERROR_EMPTY_DATETIME)]
        [DataType(DataType.DateTime)]
        public DateTime CheckInDateTime { get; set; }

        [Required(ErrorMessage = Constants.ERROR_EMPTY_DATETIME)]
        [DataType(DataType.DateTime)]
        public DateTime CheckOutDateTime { get; set; }





        public virtual CustomerProfileDev Customer { get; set; }
        public virtual ICollection<ServiceUsageProfileDev> ServicesUsed { get; set; }

    }

}

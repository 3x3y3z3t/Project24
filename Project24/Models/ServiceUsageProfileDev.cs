/*  ServiceUsageProfileDev.cs
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
    public class ServiceUsageProfileDev
    {

        public int Id { get; set; }

        [ForeignKey("VisitingProfile")]
        public int VisitingProfileId { get; set; }

        [ForeignKey("Service")]
        public int ServiceId { get; set; }


        public virtual VisitingProfileDev VisitingProfile { get; set; }
        public virtual ServiceDev Service { get; set; }



        public ServiceUsageProfileDev()
        { }

    }

}

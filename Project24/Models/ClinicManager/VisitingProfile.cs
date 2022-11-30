/*  VisitingProfile.cs
 *  Version: 1.0 (2022.11.29)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project24.Identity;

namespace Project24.Models.ClinicManager
{
    public class VisitingProfile : P24ModelBase
    {
        [ForeignKey("Customer")]
        public int CustomerId { get; protected set; }

        public bool IsTicketOpen { get; set; }

        public string TicketStatus { get; set; }

        public string Diagnose { get; set; }

        public string ProposeTreatment { get; set; }

        [DataType(DataType.MultilineText)]
        public string Note { get; set; }

        public virtual CustomerProfile Customer { get; protected set; }
        public virtual ICollection<CustomerImage> Images { get; protected set; }


        public VisitingProfile()
            : base()
        { }

        public VisitingProfile(P24IdentityUser _addedUser, CustomerProfile _customer, int _dailyIndex)
            : base(_addedUser)
        {
            Code = string.Format(AppConfig.TicketCodeFormatString, AddedDate, _dailyIndex);
            Customer = _customer;
        }

    }

}

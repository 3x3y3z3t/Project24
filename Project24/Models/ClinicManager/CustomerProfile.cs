/*  CustomerProfile.cs
 *  Version: 1.5 (2022.12.28)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project24.Models.Identity;

namespace Project24.Models.ClinicManager
{
    public class CustomerProfile : P24ModelBase
    {
        [Required(AllowEmptyStrings = false)]
        public string Code { get; protected set; }

        public string FirstMidName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(10)]
        public string LastName { get; set; }

        [Required]
        [Column(TypeName = "CHAR(1)")]
        public char Gender { get; set; }

        [Required]
        [Range(1900, AppConfig.ThisYear)]
        public int DateOfBirth { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [DataType(DataType.MultilineText)]
        public string Address { get; set; }


        public virtual ICollection<CustomerImage> CustomerImages { get; protected set; }
        public virtual ICollection<TicketProfile> VisitingTickets { get; protected set; }


        public string FullName { get { if (string.IsNullOrEmpty(FirstMidName)) return LastName; return FirstMidName + " " + LastName; } }


        public CustomerProfile()
            : base()
        { }

        public CustomerProfile(P24IdentityUser _addedUser, int _dailyIndex)
            : base(_addedUser)
        {
            Code = string.Format(AppConfig.CustomerCodeFormatString, AddedDate, _dailyIndex);
        }
    }

}

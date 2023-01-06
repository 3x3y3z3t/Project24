/*  CustomerProfile.cs
 *  Version: 1.7 (2023.01.06)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Project24.Models.Identity;

namespace Project24.Models.ClinicManager
{
    public class CustomerProfile : P24MutableObject
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

        [Required]
        [StringLength(15)]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [DataType(DataType.MultilineText)]
        public string Address { get; set; }


        [JsonIgnore]
        public virtual ICollection<CustomerImage> CustomerImages { get; protected set; }
        [JsonIgnore]
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


        public override P24ObjectPreviousVersion ConstructCurrentVersionObject()
        {
            return ConstructCurrentVersionObject_Internal(nameof(CustomerProfile));
        }
    }

}

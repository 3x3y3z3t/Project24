/*  CustomerProfile.cs
 *  Version: 1.1 (2022.10.21)
 *
 *  Contributor
 *      Arime-chan
 */

using Project24.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project24.Models
{
    public class CustomerProfile
    {
        [Key]
        public int Id { get; protected set; }

        public string CustomerCode { get; protected set; }

        public string FirstMidName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = Constants.ERROR_EMPTY_NAME)]
        [StringLength(10, ErrorMessage = Constants.ERROR_LONG_NAME)]
        public string LastName { get; set; }

        // NOTE: Date of Birth didn't make it into this version;
        //[Range(1900, AppConfig.ThisYear, ErrorMessage = P24ErrorMessage.DoBMustBeInRange)]
        //public int DateOfBirth { get; set; }

        [DataType(DataType.MultilineText)]
        public string Address { get; set; }

        [DataType(DataType.PhoneNumber, ErrorMessage = Constants.ERROR_INVALID_PHONENUMBER)]
        public string PhoneNumber { get; set; }

        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [ForeignKey("AddedUser")]
        public string AddedUserId { get; protected set; }

        [ForeignKey("UpdatedUser")]
        public string UpdatedUserId { get; protected set; }

        [DataType(DataType.DateTime)]
        public DateTime AddedDate { get; protected set; }

        [DataType(DataType.DateTime)]
        public DateTime UpdatedDate { get; set; }
        public DateTime DeletedDate { get; set; }

        public virtual P24IdentityUser AddedUser { get; protected set; }
        public virtual P24IdentityUser UpdatedUser { get; set; }
        public virtual ICollection<CustomerImage> Images { get; protected set; }

        public string FullName
        {
            get
            {
                return FirstMidName + " " + LastName;
            }
        }


        public CustomerProfile()
        { }

        public CustomerProfile(string _customerCode, string _addedUserId)
        {
            CustomerCode = _customerCode;

            //DateOfBirth = AppConfig.ThisYear;

            AddedUserId = _addedUserId;
            UpdatedUserId = _addedUserId;

            AddedDate = DateTime.Now;
            UpdatedDate = DateTime.Now;
            DeletedDate = DateTime.MinValue;
        }
    }

}

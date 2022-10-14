/*  CustomerProfileDev.cs
 *  Version: 1.2 (2022.09.21)
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
    public class CustomerProfileDev
    {
        [Key]
        public int Id { get; protected set; }

        public string CustomerCode { get; protected set; }

        public string FirstMidName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = Constants.ERROR_EMPTY_NAME)]
        [StringLength(10, ErrorMessage = Constants.ERROR_LONG_NAME)]
        public string LastName { get; set; }

        [DataType(DataType.MultilineText)]
        public string Address { get; set; }

        [DataType(DataType.PhoneNumber, ErrorMessage = Constants.ERROR_INVALID_PHONENUMBER)]
        public string PhoneNumber { get; set; }


        [ForeignKey("AddedUser")]
        public string AddedUserId { get; protected set; }

        [ForeignKey("UpdatedUser")]
        public string UpdatedUserId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime AddedDate { get; protected set; }

        [DataType(DataType.DateTime)]
        public DateTime UpdatedDate { get; set; }


        public virtual P24IdentityUser AddedUser { get; protected set; }
        public virtual P24IdentityUser UpdatedUser { get; protected set; }
        public virtual ICollection<VisitingProfileDev> Visitings { get; protected set; }

        public string FullName
        {
            get
            {
                return FirstMidName + " " + LastName;
            }
        }


        public CustomerProfileDev()
        { }

        public CustomerProfileDev(string _customerCode, string _addedUserId)
        {
            CustomerCode = _customerCode;

            AddedUserId = _addedUserId;
            UpdatedUserId = _addedUserId;

            AddedDate = DateTime.Now;
            UpdatedDate = DateTime.Now;
        }

    }

}

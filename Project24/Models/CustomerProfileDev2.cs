/*  CustomerProfileDev.cs
 *  Version: 1.0 (2022.10.09)
 *
 *  Contributor
 *      Arime-chan
 */
using Project24.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Project24.Models
{
    public class CustomerProfileDev2
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
        public virtual ICollection<VisitingProfileDev> Visitings { get; protected set; }
        public virtual ICollection<CustomerImageDev> Images { get; protected set; }

        public string FullName
        {
            get
            {
                return FirstMidName + " " + LastName;
            }
        }


        public CustomerProfileDev2()
        { }

        public CustomerProfileDev2(string _customerCode, string _addedUserId)
        {
            CustomerCode = _customerCode;

            AddedUserId = _addedUserId;
            UpdatedUserId = _addedUserId;

            AddedDate = DateTime.Now;
            UpdatedDate = DateTime.Now;
            DeletedDate = DateTime.MinValue;
        }

    }

    public class CustomerImageDev
    {
        [Key]
        public int Id { get; protected set; }

        [Required(AllowEmptyStrings = false)]
        public string Filepath { get; set; }

        [Required]
        public DateTime AddedDate { get; protected set; }

        public DateTime DeletedDate { get; set; }

        [ForeignKey("OwnedCustomer")]
        public int OwnedCustomerId { get; protected set; }


        public virtual CustomerProfileDev2 OwnedCustomer { get; set; }

        public CustomerImageDev()
        {
            AddedDate = DateTime.Now;
        }
    }

}

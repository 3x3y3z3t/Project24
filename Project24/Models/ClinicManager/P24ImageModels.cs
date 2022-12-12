/*  P24ImageModels.cs
 *  Version: 1.2 (2022.12.04)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project24.Models.Identity;
using Project24.Utils.ClinicManager;

namespace Project24.Models.ClinicManager
{
    public abstract class P24ImageModelBase
    {
        [Key]
        public int Id { get; protected set; }

        [ForeignKey("AddedUser")]
        public string AddedUserId { get; protected set; }

        [ForeignKey("UpdatedUser")]
        public string UpdatedUserId { get; protected set; }

        [Required]
        public P24Module Module { get; protected set; } = P24Module.Unset;

        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }

        [Required]
        public string Path { get; set; }

        public DateTime AddedDate { get; protected set; } = DateTime.Now;

        public DateTime DeletedDate { get; set; } = DateTime.MinValue;

        public virtual P24IdentityUser AddedUser { get; protected set; }
        public virtual P24IdentityUser UpdatedUser { get; set; }

        public string FullName { get { return Path + "/" + Name; } }


        protected P24ImageModelBase()
        { }

        protected P24ImageModelBase(P24IdentityUser _addedUser)
        {
            AddedUser = _addedUser;
            UpdatedUser = _addedUser;
        }
    }

    public class CustomerImage : P24ImageModelBase
    {
        [ForeignKey("OwnerCustomer")]
        public int OwnerCustomerId { get; protected set; }

        public virtual CustomerProfile OwnerCustomer { get; protected set; }


        public CustomerImage()
            : base()
        {
            Module = P24Module.Customer;
        }

        public CustomerImage(P24IdentityUser _addedUser)
            : base(_addedUser)
        {
            Module = P24Module.Customer;
        }

        public CustomerImage(P24IdentityUser _addedUser, CustomerProfile _ownerCustomer)
            : base(_addedUser)
        {
            OwnerCustomer = _ownerCustomer;
            Module = P24Module.Customer;
        }
    }

    public class TicketImage : P24ImageModelBase
    {
        [ForeignKey("OwnerTicket")]
        public int OwnerTicketId { get; protected set; }

        public virtual TicketProfile OwnerTicket { get; protected set; }


        public TicketImage()
            : base()
        {
            Module = P24Module.Customer;
        }

        public TicketImage(P24IdentityUser _addedUser, TicketProfile _ownerTicket)
            : base(_addedUser)
        {
            OwnerTicket = _ownerTicket;
            Module = P24Module.Customer;
        }
    }

}

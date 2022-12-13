/*  P24EntityChangelog.cs
 *  Version: 1.0 (2022.12.13)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project24.App;
using Project24.Models.Identity;

namespace Project24.Models.ClinicManager
{
    public enum P24EntityOperation
    {
        Create,
        Delete,
        Update
    }

    public abstract class P24EntityChangelog
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("UpdatedUser")]
        public string UpdatedUserId { get; protected set; }

        public DateTime ChangedDateTime { get; protected set; } = DateTime.Now;

        public P24EntityOperation Operation { get; protected set; }

        public virtual P24IdentityUser UpdatedUser { get; protected set; }


        protected P24EntityChangelog()
        { }

        protected P24EntityChangelog(P24IdentityUser _updatedUser, P24EntityOperation _operation)
        {
            UpdatedUser = _updatedUser;
            Operation = _operation;
        }
    }

    public class CustomerProfileChangelog : P24EntityChangelog
    {
        [ForeignKey("Profile")]
        public int ProfileId { get; protected set; }

        public virtual CustomerProfile Profile { get; protected set; }


        public CustomerProfileChangelog()
            : base()
        { }

        public CustomerProfileChangelog(P24IdentityUser _updatedUser, CustomerProfile _profile, P24EntityOperation _operation)
            : base(_updatedUser, _operation)
        {
            Profile = _profile;
        }
    }

    public class TicketProfileChangelog : P24EntityChangelog
    {
        [ForeignKey("Profile")]
        public int ProfileId { get; protected set; }

        public virtual TicketProfile Profile { get; protected set; }


        public TicketProfileChangelog()
            : base()
        { }

        public TicketProfileChangelog(P24IdentityUser _updatedUser, TicketProfile _profile, P24EntityOperation _operation)
            : base(_updatedUser, _operation)
        {
            Profile = _profile;
        }
    }

}

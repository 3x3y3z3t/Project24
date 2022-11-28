/*  P24ModelBase.cs
 *  Version: 1.0 (2022.11.28)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project24.Identity;

namespace Project24.Models.ClinicManager
{
    public abstract class P24ModelBase
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("AddedUser")]
        public string AddedUserId { get; protected set; }

        [ForeignKey("UpdatedUser")]
        public string UpdatedUserId { get; protected set; }

        public DateTime AddedDate { get; protected set; } = DateTime.Now;

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        public DateTime DeletedDate { get; set; } = DateTime.MinValue;

        [Required]
        public string Code { get; protected set; }

        public virtual P24IdentityUser AddedUser { get; protected set; }
        public virtual P24IdentityUser UpdatedUser { get; set; }


        protected P24ModelBase()
        { }

        protected P24ModelBase(P24IdentityUser _addedUser)
        {
            AddedUser = _addedUser;
            UpdatedUser = _addedUser;
        }
    }

}

/*  P24MutableObject.cs
 *  Version: 1.0 (2022.12.29)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project24.Models.Identity;

namespace Project24.Models
{
    /// <summary> The base class for Project24 objects. P24MutableObject can be edited and deleted. </summary>
    public abstract class P24MutableObject
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(AddedUser))]
        public string AddedUserId { get; protected set; }

        [ForeignKey(nameof(EditedUser))]
        public string EditedUserId { get; protected set; }

        [ForeignKey(nameof(PreviousVersion))]
        public int? PreviousVersionId { get; protected set; }


        [DataType(DataType.MultilineText)]
        public string Note { get; set; }

        public DateTime AddedDate { get; protected set; } = DateTime.Now;

        public DateTime EditedDate { get; set; } = DateTime.Now;

        public DateTime DeletedDate { get; set; } = DateTime.MinValue;


        public virtual P24ObjectPreviousVersion PreviousVersion { get; set; }
        public virtual P24IdentityUser AddedUser { get; protected set; }
        public virtual P24IdentityUser EditedUser { get; set; }


        protected P24MutableObject()
        { }

        protected P24MutableObject(P24IdentityUser _addedUser)
        {
            AddedUser = _addedUser;
            EditedUser = _addedUser;
        }
    }

}

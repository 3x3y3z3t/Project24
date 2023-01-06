/*  P24MutableObject.cs
 *  Version: 1.0 (2023.01.06)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Project24.Models.Identity;

namespace Project24.Models.ClinicManager
{
    /// <summary> P24MutableObject can be edited. </summary>
    public abstract class P24MutableObject : P24ObjectBase
    {

        [ForeignKey(nameof(EditedUser))]
        public string EditedUserId { get; protected set; }

        [ForeignKey(nameof(PreviousVersion))]
        public int? PreviousVersionId { get; protected set; }


        public DateTime EditedDate { get; set; } = DateTime.Now;


        [JsonIgnore]
        public virtual P24ObjectPreviousVersion PreviousVersion { get; set; }
        [JsonIgnore]
        public virtual P24IdentityUser EditedUser { get; set; }


        protected P24MutableObject()
            : base()
        { }

        protected P24MutableObject(P24IdentityUser _addedUser)
            : base(_addedUser)
        {
            EditedUser = _addedUser;
        }


        public abstract P24ObjectPreviousVersion ConstructCurrentVersionObject();

        protected P24ObjectPreviousVersion ConstructCurrentVersionObject_Internal(string _objectTypeName)
            => new P24ObjectPreviousVersion(_objectTypeName, Id, SerializeToJson(), PreviousVersion);
    }

}

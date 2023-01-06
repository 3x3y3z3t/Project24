/*  P24ObjectBase.cs
 *  Version: 1.2 (2023.01.07)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Project24.Models.Identity;

namespace Project24.Models.ClinicManager
{
    /// <summary> Base class for Project24 objects. </summary>
    public abstract class P24ObjectBase
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(AddedUser))]
        public string AddedUserId { get; protected set; }


        [DataType(DataType.MultilineText)]
        public string Note { get; set; }

        public DateTime AddedDate { get; protected set; } = DateTime.Now;

        public DateTime DeletedDate { get; set; } = DateTime.MinValue;


        [JsonIgnore]
        public virtual P24IdentityUser AddedUser { get; protected set; }


        protected P24ObjectBase()
        { }

        protected P24ObjectBase(P24IdentityUser _addedUser)
        {
            AddedUser = _addedUser;
        }


        public string SerializeToJson() => JsonSerializer.Serialize<object>(this, new JsonSerializerOptions() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
    }

}

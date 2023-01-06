/*  P24ObjectPreviousVersion.cs
 *  Version: 1.0 (2022.12.29)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project24.Models
{
    public class P24ObjectPreviousVersion
    {
        [Key]
        public int Id { get; private set; }

        [ForeignKey(nameof(PreviousVersion))]
        public int? PreviousVersionId { get; private set; }


        /// <summary> Use nameof(classname) to get object type name. </summary>
        [Required(AllowEmptyStrings = false)]
        public string ObjectType { get; private set; }

        [Required(AllowEmptyStrings = false)]
        public int ObjectId { get; private set; }

        [Required(AllowEmptyStrings = false)]
        public string Data { get; private set; }

        public DateTime AddedDate { get; private set; } = DateTime.Now;


        public virtual P24ObjectPreviousVersion PreviousVersion { get; private set; }


        public P24ObjectPreviousVersion()
        { }

        public P24ObjectPreviousVersion(string _objTypeName, int _objId, string _data, P24ObjectPreviousVersion _previousVersion = null)
        {
            PreviousVersion = _previousVersion;
            ObjectType = _objTypeName;
            ObjectId = _objId;
            Data = _data;
        }
    }

}

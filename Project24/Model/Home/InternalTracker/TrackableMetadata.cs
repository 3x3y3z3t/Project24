/*  Model/Home/InternalTracker/InternalTrackedValueMetadata.cs
 *  Version: v1.0 (2023.09.02)
 *  
 *  Author
 *      Arime-chan
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project24.Model.Home
{
    public class TrackableMetadata
    {
        [Key]
        [ForeignKey(nameof(TrackedValue))]
        public string ValueKey { get; protected set; }

        [StringLength(255)]
        public string ValueDisplayTab { get; set; }
        [StringLength(255)]
        public string ValueDisplaySection { get; set; }

        [Required]
        [StringLength(127)]
        public string ValueType { get; set; }
        [StringLength(127)]
        public string ValueRangeType { get; set; }
        public string ValueRangeAsString { get; set; }


        public virtual Trackable TrackedValue { get; protected set; }


        public TrackableMetadata()
        { }

        public TrackableMetadata(string _valueKey)
        {
            ValueKey = _valueKey;
        }
    }


    public static class Trackable_ValueType
    {
        public const string TRACKABLE_VALUE_TYPE_UNSET = nameof(TRACKABLE_VALUE_TYPE_UNSET);

        public const string TRACKABLE_VALUE_TYPE_INT32 = nameof(TRACKABLE_VALUE_TYPE_INT32);
        public const string TRACKABLE_VALUE_TYPE_STRING = nameof(TRACKABLE_VALUE_TYPE_STRING);



        public const string TRACKABLE_VALUE_TYPE_SELECT = nameof(TRACKABLE_VALUE_TYPE_SELECT);
        public const string TRACKABLE_VALUE_TYPE_SELECT_CUSTOMIZE = nameof(TRACKABLE_VALUE_TYPE_SELECT_CUSTOMIZE);
    }

    public static class Trackable_ValueRangeType
    {
        public const string TRACKABLE_VALUE_RANGE_TYPE_RANGE = nameof(TRACKABLE_VALUE_RANGE_TYPE_RANGE);
        public const string TRACKABLE_VALUE_RANGE_TYPE_LIST = nameof(TRACKABLE_VALUE_RANGE_TYPE_LIST);
    }

}

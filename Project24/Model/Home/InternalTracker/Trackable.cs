/*  Model/Home/InternalTracker/Trackable.cs
 *  Version: v1.0 (2023.08.31)
 *  
 *  Author
 *      Arime-chan
 */

using System.ComponentModel.DataAnnotations;

namespace Project24.Model.Home
{
    public class Trackable
    {
        [Key]
        public string Key { get; protected set; }
        [Required]
        public string Value { get; protected set; }

        public virtual TrackableMetadata Metadata { get; protected set; }


        public Trackable()
        { }

        public Trackable(string _key, string _value)
        {
            Key = _key;
            Value = _value;
        }
    }

}

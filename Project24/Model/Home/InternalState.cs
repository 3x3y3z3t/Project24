/*  Model/Home/InternamStates.cs
 *  Version: v1.0 (2023.08.10)
 *  
 *  Contributor
 *      Arime-chan
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project24.Model.Home
{
    public sealed class InternalState
    {
        [Key]
        public long Id { get; set; }

        public string Key { get; set; }
        public string Value { get; set; }


        public InternalState()
        { }

        public InternalState(string _key, string _value)
        {
            Key = _key;
            Value = _value;
        }
    }

}

/*  Customer.cs
*   Version: 1.0 (2022.09.01)
*
*   Contributor
*       Arime-chan
*/

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project24.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string UniqueId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int CustomerProfileId { get; set; }
        public string CustomInfo { get; set; }

        public Customer()
        {

        }



    }

}

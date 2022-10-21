/*  CustomerImage.cs
 *  Version: 1.0 (2022.10.21)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project24.Models
{
    public class CustomerImage
    {
        [Key]
        public int Id { get; protected set; }

        [Required(AllowEmptyStrings = false)]
        public string Filepath { get; set; }

        [Required]
        public DateTime AddedDate { get; protected set; }

        public DateTime DeletedDate { get; set; }

        [ForeignKey("OwnedCustomer")]
        public int OwnedCustomerId { get; protected set; }


        public virtual CustomerProfile OwnedCustomer { get; set; }


        public CustomerImage()
        {
            AddedDate = DateTime.Now;
        }
    }

}

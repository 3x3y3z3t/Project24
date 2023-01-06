/*  DrugExportBatch.cs
 *  Version: 1.1 (2023.01.07)
 *
 *  Contributor
 *      Arime-chan
 */

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Project24.Models.ClinicManager;
using Project24.Models.Identity;

namespace Project24.Models.Inventory.ClinicManager
{
    public class DrugOutBatch : P24ObjectBase
    {
        [Required(AllowEmptyStrings = false)]
        public string ExportType { get; set; }


        public virtual TicketProfile Ticket { get; protected set; }
        public virtual ICollection<DrugOutRecord> OutRecords { get; protected set; }


        public P24IdentityUser ExportedUser { get { return AddedUser; } }


        public DrugOutBatch()
            : base()
        { }

        public DrugOutBatch(P24IdentityUser _addedUser, TicketProfile _ticket = null)
            : base(_addedUser)
        {
            Ticket = _ticket;
        }
    }

}

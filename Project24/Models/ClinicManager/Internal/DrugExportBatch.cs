/*  DrugExportBatch.cs
 *  Version: 1.0 (2022.12.29)
 *
 *  Contributor
 *      Arime-chan
 */

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project24.Models.ClinicManager;
using Project24.Models.Identity;

namespace Project24.Models.Internal.ClinicManager
{
    public class DrugExportBatch : P24MutableObject
    {
        [ForeignKey("Ticket")]
        public int TicketId { get; private set; }


        [Required(AllowEmptyStrings = false)]
        public string ExportType { get; set; }


        public virtual TicketProfile Ticket { get; protected set; }
        public virtual ICollection<DrugExportation> DrugExportation { get; protected set; }


        public P24IdentityUser ExportedUser { get { return AddedUser; } }


        public DrugExportBatch()
            : base()
        { }

        public DrugExportBatch(P24IdentityUser _addedUser, TicketProfile _ticket = null)
            : base(_addedUser)
        {
            Ticket = _ticket;
        }
    }

}

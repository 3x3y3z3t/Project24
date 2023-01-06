/*  VisitingProfile.cs
 *  Version: 1.4 (2023.01.06)
 *
 *  Contributor
 *      Arime-chan
 */

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Project24.Models.Identity;
using Project24.Models.Inventory.ClinicManager;

namespace Project24.Models.ClinicManager
{
    public class TicketProfile : P24MutableObject
    {
        [ForeignKey(nameof(Customer))]
        public int CustomerId { get; protected set; }

        [ForeignKey(nameof(DrugExportBatch))]
        public int? DrugExportBatchId { get; protected set; }


        [Required(AllowEmptyStrings = false)]
        public string Code { get; protected set; }

        public string Symptom { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Diagnose { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string ProposeTreatment { get; set; }


        [JsonIgnore]
        public virtual CustomerProfile Customer { get; protected set; }
        [JsonIgnore]
        public virtual DrugOutBatch DrugExportBatch { get; protected set; }
        [JsonIgnore]
        public virtual ICollection<TicketImage> TicketImages { get; protected set; }


        public TicketProfile()
            : base()
        { }

        public TicketProfile(P24IdentityUser _addedUser, CustomerProfile _customer, int _dailyIndex)
            : base(_addedUser)
        {
            Code = string.Format(AppConfig.TicketCodeFormatString, AddedDate, _dailyIndex);
            Customer = _customer;
        }


        public override P24ObjectPreviousVersion ConstructCurrentVersionObject()
        {
            return ConstructCurrentVersionObject_Internal(nameof(TicketProfile));
        }
    }

}

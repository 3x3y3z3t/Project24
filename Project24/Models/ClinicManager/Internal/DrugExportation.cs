/*  DrugExportation.cs
 *  Version: 1.0 (2022.12.28)
 *
 *  Contributor
 *      Arime-chan
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project24.Models.Internal.ClinicManager
{
    public class DrugExportation
    {
        [Key]
        public int Id { get; protected set; }

        [ForeignKey("Drug")]
        public int DrugId { get; protected set; }

        [ForeignKey("ExportBatch")]
        public int ExportBatchId { get; protected set; }


        public int Amount { get; set; }


        public virtual Drug Drug { get; protected set; }
        public virtual DrugExportBatch ExportBatch { get; protected set; }


        public DrugExportation()
        { }

        public DrugExportation(DrugExportBatch _exportBatch, Drug _drug, int _amount)
        {
            ExportBatch = _exportBatch;
            Drug = _drug;
            Amount = _amount;
        }
    }

}

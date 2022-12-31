/*  DrugImportation.cs
 *  Version: 1.0 (2022.12.31)
 *
 *  Contributor
 *      Arime-chan
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project24.Models.Internal.ClinicManager
{
    public class DrugImportation
    {
        [Key]
        public int Id { get; protected set; }

        [ForeignKey("Drug")]
        public int DrugId { get; protected set; }

        [ForeignKey("ImportBatch")]
        public int ImportBatchId { get; protected set; }


        public int Amount { get; set; }


        public virtual Drug Drug { get; protected set; }
        public virtual DrugImportBatch ImportBatch { get; protected set; }


        public DrugImportation()
        { }

        public DrugImportation(DrugImportBatch _importBatch, Drug _drug, int _amount)
        {
            ImportBatch = _importBatch;
            Drug = _drug;
            Amount = _amount;
        }
    }

}

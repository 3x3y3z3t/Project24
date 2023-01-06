/*  DrugExportation.cs
 *  Version: 1.1 (2023.01.07)
 *
 *  Contributor
 *      Arime-chan
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project24.App;

namespace Project24.Models.Inventory.ClinicManager
{
    public class DrugOutRecord
    {
        [Key]
        public int Id { get; protected set; }

        [ForeignKey(nameof(Drug))]
        public int DrugId { get; protected set; }

        [ForeignKey(nameof(OutBatch))]
        public int BatchId { get; protected set; }


        public int Amount { get; set; }


        public virtual Drug Drug { get; protected set; }
        public virtual DrugOutBatch OutBatch { get; protected set; }


        public DrugOutRecord()
        { }

        public DrugOutRecord(DrugOutBatch _outBatch, Drug _drug, int _amount)
        {
            OutBatch = _outBatch;
            Drug = _drug;
            Amount = _amount;
        }
    }

}

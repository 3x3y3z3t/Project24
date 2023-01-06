/*  DrugInRecord.cs
 *  Version: 1.1 (2023.01.07)
 *
 *  Contributor
 *      Arime-chan
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project24.Models.Inventory.ClinicManager
{
    public class DrugInRecord
    {
        [Key]
        public int Id { get; protected set; }

        [ForeignKey(nameof(Drug))]
        public int DrugId { get; protected set; }

        [ForeignKey(nameof(InBatch))]
        public int BatchId { get; protected set; }


        public int Amount { get; set; }


        public virtual Drug Drug { get; protected set; }
        public virtual DrugInBatch InBatch { get; protected set; }


        public DrugInRecord()
        { }

        public DrugInRecord(DrugInBatch _inBatch, Drug _drug, int _amount)
        {
            InBatch = _inBatch;
            Drug = _drug;
            Amount = _amount;
        }
    }

}

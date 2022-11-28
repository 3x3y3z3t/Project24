/*  DailyIndexes.cs
 *  Version: 1.0 (2022.11.20)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project24.Models
{
    public class DailyIndexes
    {
        [Key]
        public int Id { get; set; }

        [DataType(DataType.Date)]
        [Column(TypeName = "DATE")]
        public DateTime Date { get; private set; }

        public byte CustomerIndex { get; set; }

        public byte VisitingIndex { get; set; }


        public DailyIndexes()
            : this(DateTime.Today)
        { }

        public DailyIndexes(DateTime _today)
        {
            Date = _today;
        }
    }

}

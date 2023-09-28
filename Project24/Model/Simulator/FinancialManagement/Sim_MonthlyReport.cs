/*  Home/Simulator/FinancialManagement/Transaction.cs
 *  Version: v1.0 (2023.09.24)
 *  
 *  Author
 *      Arime-chan
 */

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Project24.Model.Simulator.FinancialManagement
{
    [Index(nameof(Year))]
    [Index(nameof(Month))]
    public class Sim_MonthlyReport
    {
        [Key]
        public short Id { get; private set; }

        [DataType(DataType.Currency)]
        public int BalanceIn { get; set; }
        [DataType(DataType.Currency)]
        public int BalanceOut { get; set; }

        
        [Range(2022, short.MaxValue)]
        public short Year { get; set; }
        [Range(1, 12)]
        public short Month { get; set; }


        public virtual IList<Sim_Transaction> Transactions { get; private set; }


        public Sim_MonthlyReport()
        { }
    }

}

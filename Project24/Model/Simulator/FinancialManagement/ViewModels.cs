/*  Home/Simulator/FinancialManagement/ViewModels.cs
 *  Version: v1.0 (2023.09.24)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Project24.Model.Simulator.FinancialManagement
{
    public class Sim_ReportViewModel
    {
        [JsonIgnore]
        public short Id { get; set; }

        public short Year { get; set; }
        public short Month { get; set; }
        public int BalanceIn { get; set; }
        public int BalanceOut { get; set; }

        public List<Sim_TransactionViewModel> Transactions { get; set; }


        public Sim_ReportViewModel()
        { }
    }

    public class Sim_TransactionViewModel
    {
        public int Id { get; set; }

        public DateTime AddedDate { get; set; }
        public string Category { set; get; }
        public int Amount { set; get; }
        public string Details { set; get; }


        public Sim_TransactionViewModel()
        { }
    }

}

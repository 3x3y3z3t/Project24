/*  Home/Simulator/FinancialManagement/Transaction.cs
 *  Version: v1.0 (2023.09.24)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project24.Model.Simulator.FinancialManagement
{
    public class Sim_Transaction
    {
        [Key]
        public int Id { get; private set; }

        [ForeignKey(nameof(Report))]
        public short ReportId { get; private set; }
        [ForeignKey(nameof(Category))]
        public short CategoryId { get; private set; }

        public DateTime AddedDate { get; set; }

        [DataType(DataType.Currency)]
        public int Amount { get; set; }

        public string Details { get; set; }


        public virtual Sim_TransactionCategory Category { get; private set; }
        public virtual Sim_MonthlyReport Report { get; private set; }


        public Sim_Transaction()
        { }

        public Sim_Transaction(Sim_TransactionCategory _category, Sim_MonthlyReport _report)
        {
            Category = _category;
            Report = _report;
        }
    }

    public class Sim_TransactionCategory
    {
        [Key]
        public short Id { get; private set; }

        public string Name { get; set; }


        public Sim_TransactionCategory()
        { }

        public Sim_TransactionCategory(string _name)
        {
            Name = _name;
        }
    }

}

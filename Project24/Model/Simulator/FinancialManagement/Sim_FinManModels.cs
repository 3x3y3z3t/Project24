/*  Home/Simulator/FinancialManagement/Sim_FinManModels.cs
 *  Version: v1.3 (2023.10.01)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Project24.Model.Simulator.FinancialManagement
{
    internal interface ISim_FinManModel { }

    internal class ImportExportDataModel
    {
        public List<Sim_TransactionCategory> Categories { get; set; }
        public List<Sim_Transaction> Transactions { get; set; }
        public List<Sim_MonthlyReport> Reports { get; set; }


        public ImportExportDataModel()
        { }
    }

    [Index(nameof(Year))]
    [Index(nameof(Month))]
    public class Sim_MonthlyReport : Syncable
    {
        [Key]
        public short Id { get; set; }

        [DataType(DataType.Currency)]
        public int BalanceIn { get; set; }
        [DataType(DataType.Currency)]
        public int BalanceOut { get; set; }

        [Range(2022, short.MaxValue)]
        public short Year { get; set; }
        [Range(1, 12)]
        public short Month { get; set; }


        [JsonIgnore]
        public virtual IList<Sim_Transaction> Transactions { get; private set; }


        public Sim_MonthlyReport()
        { }

        public Sim_MonthlyReport(short _year, short _month)
        {
            Year = _year;
            Month = _month;
        }


        protected override string GetFieldsValuesConcatenatedInternal() => ""
                + Id
                + BalanceIn
                + BalanceOut
                + Year
                + Month;
    }

    public class Sim_Transaction : Syncable
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Report))]
        public short ReportId { get; set; }
        [ForeignKey(nameof(Category))]
        public short CategoryId { get; set; }

        public DateTime AddedDate { get; set; }

        [DataType(DataType.Currency)]
        public int Amount { get; set; }

        public string Details { get; set; }


        [JsonIgnore]
        public virtual Sim_TransactionCategory Category { get; private set; }
        [JsonIgnore]
        public virtual Sim_MonthlyReport Report { get; private set; }


        public Sim_Transaction()
        { }

        public Sim_Transaction(Sim_TransactionCategory _category, Sim_MonthlyReport _report)
        {
            if (_category != null)
                Category = _category;
            if (_report != null)
                Report = _report;
        }


        protected override string GetFieldsValuesConcatenatedInternal() => ""
                + Id
                + ReportId
                + CategoryId
                + AddedDate
                + Amount
                + Details;
    }

    public class Sim_TransactionCategory : Syncable
    {
        [Key]
        public short Id { get; set; }

        public string Name { get; set; }


        public Sim_TransactionCategory()
        { }

        public Sim_TransactionCategory(string _name)
        {
            Name = _name;
        }


        protected override string GetFieldsValuesConcatenatedInternal() => ""
            + Id
            + Name;
    }

}

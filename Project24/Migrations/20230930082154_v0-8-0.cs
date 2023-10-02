using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project24.Migrations
{
    public partial class v080 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<ulong>(
                name: "Version",
                table: "Sim_Transactions",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);
            migrationBuilder.AddColumn<string>(
                name: "Hash",
                table: "Sim_Transactions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<ulong>(
                name: "Version",
                table: "Sim_TransactionCategories",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<string>(
                name: "Hash",
                table: "Sim_TransactionCategories",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<ulong>(
                name: "Version",
                table: "Sim_MonthlyReports",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<string>(
                name: "Hash",
                table: "Sim_MonthlyReports",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Sim_Transactions");
            migrationBuilder.DropColumn(
                name: "Hash",
                table: "Sim_Transactions");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Sim_TransactionCategories");

            migrationBuilder.DropColumn(
                name: "Hash",
                table: "Sim_TransactionCategories");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Sim_MonthlyReports");

            migrationBuilder.DropColumn(
                name: "Hash",
                table: "Sim_MonthlyReports");
        }
    }
}

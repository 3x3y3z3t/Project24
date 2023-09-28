using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project24.Migrations
{
    public partial class v070 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sim_MonthlyReports",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BalanceIn = table.Column<int>(type: "int", nullable: false),
                    BalanceOut = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<short>(type: "smallint", nullable: false),
                    Month = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sim_MonthlyReports", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Sim_TransactionCategories",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sim_TransactionCategories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Sim_Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ReportId = table.Column<short>(type: "smallint", nullable: false),
                    CategoryId = table.Column<short>(type: "smallint", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    Details = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sim_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sim_Transactions_Sim_MonthlyReports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Sim_MonthlyReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sim_Transactions_Sim_TransactionCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Sim_TransactionCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Sim_MonthlyReports_Month",
                table: "Sim_MonthlyReports",
                column: "Month");

            migrationBuilder.CreateIndex(
                name: "IX_Sim_MonthlyReports_Year",
                table: "Sim_MonthlyReports",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_Sim_Transactions_CategoryId",
                table: "Sim_Transactions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Sim_Transactions_ReportId",
                table: "Sim_Transactions",
                column: "ReportId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sim_Transactions");

            migrationBuilder.DropTable(
                name: "Sim_MonthlyReports");

            migrationBuilder.DropTable(
                name: "Sim_TransactionCategories");
        }
    }
}

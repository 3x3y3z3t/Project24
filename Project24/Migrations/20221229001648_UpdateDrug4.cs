using Microsoft.EntityFrameworkCore.Migrations;

namespace Project24.Migrations
{
    public partial class UpdateDrug4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Amount",
                table: "Drugs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Drugs");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace Project24.Migrations
{
    public partial class _01_UpdateCustomerProfile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "CustomerProfilesDev2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "CustomerProfilesDev2");
        }
    }
}

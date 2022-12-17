using Microsoft.EntityFrameworkCore.Migrations;

namespace Project24.Migrations
{
    public partial class asdsadad2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TusFileId",
                table: "NasCachedFiles");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TusFileId",
                table: "NasCachedFiles",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);
        }
    }
}

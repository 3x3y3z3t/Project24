using Microsoft.EntityFrameworkCore.Migrations;

namespace Project24.Migrations
{
    public partial class asdsadad : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TusFileId",
                table: "NasCachedFiles",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TusFileId",
                table: "NasCachedFiles");
        }
    }
}

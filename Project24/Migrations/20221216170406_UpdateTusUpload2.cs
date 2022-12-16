using Microsoft.EntityFrameworkCore.Migrations;

namespace Project24.Migrations
{
    public partial class UpdateTusUpload2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailCount",
                table: "NasCachedFiles",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailCount",
                table: "NasCachedFiles");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Project24.Migrations
{
    public partial class UpdateTusUpload : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "Module",
                table: "UserUploads",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModDate",
                table: "NasCachedFiles",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "Length",
                table: "NasCachedFiles",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastModDate",
                table: "NasCachedFiles");

            migrationBuilder.DropColumn(
                name: "Length",
                table: "NasCachedFiles");

            migrationBuilder.AlterColumn<int>(
                name: "Module",
                table: "UserUploads",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte));
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace Project24.Migrations
{
    public partial class UpdateModels1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DrugExportBatches_TicketProfiles_TicketId",
                table: "DrugExportBatches");

            migrationBuilder.DropIndex(
                name: "IX_DrugExportBatches_TicketId",
                table: "DrugExportBatches");

            migrationBuilder.DropColumn(
                name: "IsTicketOpen",
                table: "TicketProfiles");

            migrationBuilder.DropColumn(
                name: "TicketStatus",
                table: "TicketProfiles");

            migrationBuilder.DropColumn(
                name: "TicketId",
                table: "DrugExportBatches");

            migrationBuilder.AlterColumn<string>(
                name: "ProposeTreatment",
                table: "TicketProfiles",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Diagnose",
                table: "TicketProfiles",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DrugExportBatchId",
                table: "TicketProfiles",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "CustomerProfiles",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255) CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketProfiles_DrugExportBatchId",
                table: "TicketProfiles",
                column: "DrugExportBatchId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketProfiles_DrugExportBatches_DrugExportBatchId",
                table: "TicketProfiles",
                column: "DrugExportBatchId",
                principalTable: "DrugExportBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketProfiles_DrugExportBatches_DrugExportBatchId",
                table: "TicketProfiles");

            migrationBuilder.DropIndex(
                name: "IX_TicketProfiles_DrugExportBatchId",
                table: "TicketProfiles");

            migrationBuilder.DropColumn(
                name: "DrugExportBatchId",
                table: "TicketProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "ProposeTreatment",
                table: "TicketProfiles",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Diagnose",
                table: "TicketProfiles",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<bool>(
                name: "IsTicketOpen",
                table: "TicketProfiles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TicketStatus",
                table: "TicketProfiles",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TicketId",
                table: "DrugExportBatches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "CustomerProfiles",
                type: "varchar(255) CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 15);

            migrationBuilder.CreateIndex(
                name: "IX_DrugExportBatches_TicketId",
                table: "DrugExportBatches",
                column: "TicketId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DrugExportBatches_TicketProfiles_TicketId",
                table: "DrugExportBatches",
                column: "TicketId",
                principalTable: "TicketProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

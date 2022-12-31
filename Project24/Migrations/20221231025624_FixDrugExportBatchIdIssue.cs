using Microsoft.EntityFrameworkCore.Migrations;

namespace Project24.Migrations
{
    public partial class FixDrugExportBatchIdIssue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TicketProfiles_DrugExportBatchId",
                table: "TicketProfiles",
                column: "DrugExportBatchId");

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
        }
    }
}

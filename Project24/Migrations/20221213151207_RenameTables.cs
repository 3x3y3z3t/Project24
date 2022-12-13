using Microsoft.EntityFrameworkCore.Migrations;

namespace Project24.Migrations
{
    public partial class RenameTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketImages_VisitingProfiles_OwnerTicketId",
                table: "TicketImages");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketProfileChangelogs_VisitingProfiles_ProfileId",
                table: "TicketProfileChangelogs");

            migrationBuilder.DropForeignKey(
                name: "FK_VisitingProfiles_AspNetUsers_AddedUserId",
                table: "VisitingProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_VisitingProfiles_CustomerProfiles_CustomerId",
                table: "VisitingProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_VisitingProfiles_AspNetUsers_UpdatedUserId",
                table: "VisitingProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VisitingProfiles",
                table: "VisitingProfiles");

            migrationBuilder.RenameTable(
                name: "VisitingProfiles",
                newName: "TicketProfiles");

            migrationBuilder.RenameIndex(
                name: "IX_VisitingProfiles_UpdatedUserId",
                table: "TicketProfiles",
                newName: "IX_TicketProfiles_UpdatedUserId");

            migrationBuilder.RenameIndex(
                name: "IX_VisitingProfiles_CustomerId",
                table: "TicketProfiles",
                newName: "IX_TicketProfiles_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_VisitingProfiles_Code",
                table: "TicketProfiles",
                newName: "IX_TicketProfiles_Code");

            migrationBuilder.RenameIndex(
                name: "IX_VisitingProfiles_AddedUserId",
                table: "TicketProfiles",
                newName: "IX_TicketProfiles_AddedUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TicketProfiles",
                table: "TicketProfiles",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketImages_TicketProfiles_OwnerTicketId",
                table: "TicketImages",
                column: "OwnerTicketId",
                principalTable: "TicketProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketProfileChangelogs_TicketProfiles_ProfileId",
                table: "TicketProfileChangelogs",
                column: "ProfileId",
                principalTable: "TicketProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketProfiles_AspNetUsers_AddedUserId",
                table: "TicketProfiles",
                column: "AddedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketProfiles_CustomerProfiles_CustomerId",
                table: "TicketProfiles",
                column: "CustomerId",
                principalTable: "CustomerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketProfiles_AspNetUsers_UpdatedUserId",
                table: "TicketProfiles",
                column: "UpdatedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketImages_TicketProfiles_OwnerTicketId",
                table: "TicketImages");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketProfileChangelogs_TicketProfiles_ProfileId",
                table: "TicketProfileChangelogs");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketProfiles_AspNetUsers_AddedUserId",
                table: "TicketProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketProfiles_CustomerProfiles_CustomerId",
                table: "TicketProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketProfiles_AspNetUsers_UpdatedUserId",
                table: "TicketProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TicketProfiles",
                table: "TicketProfiles");

            migrationBuilder.RenameTable(
                name: "TicketProfiles",
                newName: "VisitingProfiles");

            migrationBuilder.RenameIndex(
                name: "IX_TicketProfiles_UpdatedUserId",
                table: "VisitingProfiles",
                newName: "IX_VisitingProfiles_UpdatedUserId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketProfiles_CustomerId",
                table: "VisitingProfiles",
                newName: "IX_VisitingProfiles_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketProfiles_Code",
                table: "VisitingProfiles",
                newName: "IX_VisitingProfiles_Code");

            migrationBuilder.RenameIndex(
                name: "IX_TicketProfiles_AddedUserId",
                table: "VisitingProfiles",
                newName: "IX_VisitingProfiles_AddedUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VisitingProfiles",
                table: "VisitingProfiles",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketImages_VisitingProfiles_OwnerTicketId",
                table: "TicketImages",
                column: "OwnerTicketId",
                principalTable: "VisitingProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketProfileChangelogs_VisitingProfiles_ProfileId",
                table: "TicketProfileChangelogs",
                column: "ProfileId",
                principalTable: "VisitingProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VisitingProfiles_AspNetUsers_AddedUserId",
                table: "VisitingProfiles",
                column: "AddedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VisitingProfiles_CustomerProfiles_CustomerId",
                table: "VisitingProfiles",
                column: "CustomerId",
                principalTable: "CustomerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VisitingProfiles_AspNetUsers_UpdatedUserId",
                table: "VisitingProfiles",
                column: "UpdatedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

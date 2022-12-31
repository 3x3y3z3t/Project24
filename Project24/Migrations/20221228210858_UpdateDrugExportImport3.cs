using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Project24.Migrations
{
    public partial class UpdateDrugExportImport3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerProfiles_AspNetUsers_UpdatedUserId",
                table: "CustomerProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_DrugExportBatches_AspNetUsers_ExporterUserId",
                table: "DrugExportBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_DrugImportBatches_AspNetUsers_ImporterUserId",
                table: "DrugImportBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketProfiles_AspNetUsers_UpdatedUserId",
                table: "TicketProfiles");

            migrationBuilder.DropTable(
                name: "CustomerProfileChangelogs");

            migrationBuilder.DropTable(
                name: "TicketProfileChangelogs");

            migrationBuilder.DropIndex(
                name: "IX_TicketProfiles_UpdatedUserId",
                table: "TicketProfiles");

            migrationBuilder.DropIndex(
                name: "IX_DrugImportBatches_ImporterUserId",
                table: "DrugImportBatches");

            migrationBuilder.DropIndex(
                name: "IX_DrugExportBatches_ExporterUserId",
                table: "DrugExportBatches");

            migrationBuilder.DropIndex(
                name: "IX_CustomerProfiles_UpdatedUserId",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "TicketProfiles");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "TicketProfiles");

            migrationBuilder.DropColumn(
                name: "UpdatedUserId",
                table: "TicketProfiles");

            migrationBuilder.DropColumn(
                name: "ImportedDate",
                table: "DrugImportBatches");

            migrationBuilder.DropColumn(
                name: "ImporterUserId",
                table: "DrugImportBatches");

            migrationBuilder.DropColumn(
                name: "ExportedDate",
                table: "DrugExportBatches");

            migrationBuilder.DropColumn(
                name: "ExporterUserId",
                table: "DrugExportBatches");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "UpdatedUserId",
                table: "CustomerProfiles");

            migrationBuilder.AddColumn<int>(
                name: "DrugExportBatchId",
                table: "TicketProfiles",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EditedDate",
                table: "TicketProfiles",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "EditedUserId",
                table: "TicketProfiles",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "TicketProfiles",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousVersionId",
                table: "TicketProfiles",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Symptom",
                table: "TicketProfiles",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Drugs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "Drugs",
                nullable: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedDate",
                table: "DrugImportBatches",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AddedUserId",
                table: "DrugImportBatches",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "DrugImportBatches",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EditedDate",
                table: "DrugImportBatches",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "EditedUserId",
                table: "DrugImportBatches",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousVersionId",
                table: "DrugImportBatches",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedDate",
                table: "DrugExportBatches",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AddedUserId",
                table: "DrugExportBatches",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "DrugExportBatches",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EditedDate",
                table: "DrugExportBatches",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "EditedUserId",
                table: "DrugExportBatches",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousVersionId",
                table: "DrugExportBatches",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TicketId",
                table: "DrugExportBatches",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "CustomerProfiles",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "EditedDate",
                table: "CustomerProfiles",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "EditedUserId",
                table: "CustomerProfiles",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "CustomerProfiles",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousVersionId",
                table: "CustomerProfiles",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Changelogs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PreviousVersionId = table.Column<int>(nullable: true),
                    ObjectType = table.Column<string>(nullable: false),
                    ObjectId = table.Column<string>(nullable: false),
                    Data = table.Column<string>(nullable: false),
                    AddedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Changelogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Changelogs_Changelogs_PreviousVersionId",
                        column: x => x.PreviousVersionId,
                        principalTable: "Changelogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketProfiles_EditedUserId",
                table: "TicketProfiles",
                column: "EditedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketProfiles_PreviousVersionId",
                table: "TicketProfiles",
                column: "PreviousVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugImportBatches_AddedUserId",
                table: "DrugImportBatches",
                column: "AddedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugImportBatches_EditedUserId",
                table: "DrugImportBatches",
                column: "EditedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugImportBatches_PreviousVersionId",
                table: "DrugImportBatches",
                column: "PreviousVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugExportBatches_AddedUserId",
                table: "DrugExportBatches",
                column: "AddedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugExportBatches_EditedUserId",
                table: "DrugExportBatches",
                column: "EditedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugExportBatches_PreviousVersionId",
                table: "DrugExportBatches",
                column: "PreviousVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugExportBatches_TicketId",
                table: "DrugExportBatches",
                column: "TicketId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerProfiles_Code",
                table: "CustomerProfiles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerProfiles_EditedUserId",
                table: "CustomerProfiles",
                column: "EditedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerProfiles_PreviousVersionId",
                table: "CustomerProfiles",
                column: "PreviousVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Changelogs_ObjectType",
                table: "Changelogs",
                column: "ObjectType");

            migrationBuilder.CreateIndex(
                name: "IX_Changelogs_PreviousVersionId",
                table: "Changelogs",
                column: "PreviousVersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerProfiles_AspNetUsers_EditedUserId",
                table: "CustomerProfiles",
                column: "EditedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerProfiles_Changelogs_PreviousVersionId",
                table: "CustomerProfiles",
                column: "PreviousVersionId",
                principalTable: "Changelogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DrugExportBatches_AspNetUsers_AddedUserId",
                table: "DrugExportBatches",
                column: "AddedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DrugExportBatches_AspNetUsers_EditedUserId",
                table: "DrugExportBatches",
                column: "EditedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DrugExportBatches_Changelogs_PreviousVersionId",
                table: "DrugExportBatches",
                column: "PreviousVersionId",
                principalTable: "Changelogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DrugExportBatches_TicketProfiles_TicketId",
                table: "DrugExportBatches",
                column: "TicketId",
                principalTable: "TicketProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DrugImportBatches_AspNetUsers_AddedUserId",
                table: "DrugImportBatches",
                column: "AddedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DrugImportBatches_AspNetUsers_EditedUserId",
                table: "DrugImportBatches",
                column: "EditedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DrugImportBatches_Changelogs_PreviousVersionId",
                table: "DrugImportBatches",
                column: "PreviousVersionId",
                principalTable: "Changelogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketProfiles_AspNetUsers_EditedUserId",
                table: "TicketProfiles",
                column: "EditedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketProfiles_Changelogs_PreviousVersionId",
                table: "TicketProfiles",
                column: "PreviousVersionId",
                principalTable: "Changelogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerProfiles_AspNetUsers_EditedUserId",
                table: "CustomerProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerProfiles_Changelogs_PreviousVersionId",
                table: "CustomerProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_DrugExportBatches_AspNetUsers_AddedUserId",
                table: "DrugExportBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_DrugExportBatches_AspNetUsers_EditedUserId",
                table: "DrugExportBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_DrugExportBatches_Changelogs_PreviousVersionId",
                table: "DrugExportBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_DrugExportBatches_TicketProfiles_TicketId",
                table: "DrugExportBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_DrugImportBatches_AspNetUsers_AddedUserId",
                table: "DrugImportBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_DrugImportBatches_AspNetUsers_EditedUserId",
                table: "DrugImportBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_DrugImportBatches_Changelogs_PreviousVersionId",
                table: "DrugImportBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketProfiles_AspNetUsers_EditedUserId",
                table: "TicketProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketProfiles_Changelogs_PreviousVersionId",
                table: "TicketProfiles");

            migrationBuilder.DropTable(
                name: "Changelogs");

            migrationBuilder.DropIndex(
                name: "IX_TicketProfiles_EditedUserId",
                table: "TicketProfiles");

            migrationBuilder.DropIndex(
                name: "IX_TicketProfiles_PreviousVersionId",
                table: "TicketProfiles");

            migrationBuilder.DropIndex(
                name: "IX_DrugImportBatches_AddedUserId",
                table: "DrugImportBatches");

            migrationBuilder.DropIndex(
                name: "IX_DrugImportBatches_EditedUserId",
                table: "DrugImportBatches");

            migrationBuilder.DropIndex(
                name: "IX_DrugImportBatches_PreviousVersionId",
                table: "DrugImportBatches");

            migrationBuilder.DropIndex(
                name: "IX_DrugExportBatches_AddedUserId",
                table: "DrugExportBatches");

            migrationBuilder.DropIndex(
                name: "IX_DrugExportBatches_EditedUserId",
                table: "DrugExportBatches");

            migrationBuilder.DropIndex(
                name: "IX_DrugExportBatches_PreviousVersionId",
                table: "DrugExportBatches");

            migrationBuilder.DropIndex(
                name: "IX_DrugExportBatches_TicketId",
                table: "DrugExportBatches");

            migrationBuilder.DropIndex(
                name: "IX_CustomerProfiles_Code",
                table: "CustomerProfiles");

            migrationBuilder.DropIndex(
                name: "IX_CustomerProfiles_EditedUserId",
                table: "CustomerProfiles");

            migrationBuilder.DropIndex(
                name: "IX_CustomerProfiles_PreviousVersionId",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "DrugExportBatchId",
                table: "TicketProfiles");

            migrationBuilder.DropColumn(
                name: "EditedDate",
                table: "TicketProfiles");

            migrationBuilder.DropColumn(
                name: "EditedUserId",
                table: "TicketProfiles");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "TicketProfiles");

            migrationBuilder.DropColumn(
                name: "PreviousVersionId",
                table: "TicketProfiles");

            migrationBuilder.DropColumn(
                name: "Symptom",
                table: "TicketProfiles");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "Drugs");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Drugs");

            migrationBuilder.DropColumn(
                name: "AddedDate",
                table: "DrugImportBatches");

            migrationBuilder.DropColumn(
                name: "AddedUserId",
                table: "DrugImportBatches");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "DrugImportBatches");

            migrationBuilder.DropColumn(
                name: "EditedDate",
                table: "DrugImportBatches");

            migrationBuilder.DropColumn(
                name: "EditedUserId",
                table: "DrugImportBatches");

            migrationBuilder.DropColumn(
                name: "PreviousVersionId",
                table: "DrugImportBatches");

            migrationBuilder.DropColumn(
                name: "AddedDate",
                table: "DrugExportBatches");

            migrationBuilder.DropColumn(
                name: "AddedUserId",
                table: "DrugExportBatches");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "DrugExportBatches");

            migrationBuilder.DropColumn(
                name: "EditedDate",
                table: "DrugExportBatches");

            migrationBuilder.DropColumn(
                name: "EditedUserId",
                table: "DrugExportBatches");

            migrationBuilder.DropColumn(
                name: "PreviousVersionId",
                table: "DrugExportBatches");

            migrationBuilder.DropColumn(
                name: "TicketId",
                table: "DrugExportBatches");

            migrationBuilder.DropColumn(
                name: "EditedDate",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "EditedUserId",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "PreviousVersionId",
                table: "CustomerProfiles");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "TicketProfiles",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "TicketProfiles",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedUserId",
                table: "TicketProfiles",
                type: "varchar(255) CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ImportedDate",
                table: "DrugImportBatches",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ImporterUserId",
                table: "DrugImportBatches",
                type: "varchar(255) CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExportedDate",
                table: "DrugExportBatches",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ExporterUserId",
                table: "DrugExportBatches",
                type: "varchar(255) CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "CustomerProfiles",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "CustomerProfiles",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "CustomerProfiles",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedUserId",
                table: "CustomerProfiles",
                type: "varchar(255) CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomerProfileChangelogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChangedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Operation = table.Column<int>(type: "int", nullable: false),
                    ProfileId = table.Column<int>(type: "int", nullable: false),
                    UpdatedUserId = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerProfileChangelogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerProfileChangelogs_CustomerProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "CustomerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerProfileChangelogs_AspNetUsers_UpdatedUserId",
                        column: x => x.UpdatedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TicketProfileChangelogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChangedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Operation = table.Column<int>(type: "int", nullable: false),
                    ProfileId = table.Column<int>(type: "int", nullable: false),
                    UpdatedUserId = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketProfileChangelogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketProfileChangelogs_TicketProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "TicketProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketProfileChangelogs_AspNetUsers_UpdatedUserId",
                        column: x => x.UpdatedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketProfiles_UpdatedUserId",
                table: "TicketProfiles",
                column: "UpdatedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugImportBatches_ImporterUserId",
                table: "DrugImportBatches",
                column: "ImporterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugExportBatches_ExporterUserId",
                table: "DrugExportBatches",
                column: "ExporterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerProfiles_UpdatedUserId",
                table: "CustomerProfiles",
                column: "UpdatedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerProfileChangelogs_ProfileId",
                table: "CustomerProfileChangelogs",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerProfileChangelogs_UpdatedUserId",
                table: "CustomerProfileChangelogs",
                column: "UpdatedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketProfileChangelogs_ProfileId",
                table: "TicketProfileChangelogs",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketProfileChangelogs_UpdatedUserId",
                table: "TicketProfileChangelogs",
                column: "UpdatedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerProfiles_AspNetUsers_UpdatedUserId",
                table: "CustomerProfiles",
                column: "UpdatedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DrugExportBatches_AspNetUsers_ExporterUserId",
                table: "DrugExportBatches",
                column: "ExporterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DrugImportBatches_AspNetUsers_ImporterUserId",
                table: "DrugImportBatches",
                column: "ImporterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketProfiles_AspNetUsers_UpdatedUserId",
                table: "TicketProfiles",
                column: "UpdatedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

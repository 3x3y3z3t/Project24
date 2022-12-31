using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Project24.Migrations
{
    public partial class AddDrugExportImport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DrugExportBatches",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ExporterUserId = table.Column<string>(nullable: true),
                    ExportType = table.Column<string>(nullable: false),
                    Note = table.Column<string>(nullable: true),
                    ExportedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugExportBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrugExportBatches_AspNetUsers_ExporterUserId",
                        column: x => x.ExporterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DrugImportBatches",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ImporterUserId = table.Column<string>(nullable: true),
                    Note = table.Column<string>(nullable: true),
                    ImportedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugImportBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrugImportBatches_AspNetUsers_ImporterUserId",
                        column: x => x.ImporterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Drugs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: false),
                    DeletedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drugs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DrugExportations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DrugId = table.Column<int>(nullable: false),
                    ExportBatchId = table.Column<int>(nullable: false),
                    Amount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugExportations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrugExportations_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrugExportations_DrugExportBatches_ExportBatchId",
                        column: x => x.ExportBatchId,
                        principalTable: "DrugExportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DrugImportations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DrugId = table.Column<int>(nullable: false),
                    ImportBatchId = table.Column<int>(nullable: false),
                    Amount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugImportations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrugImportations_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrugImportations_DrugImportBatches_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "DrugImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DrugExportations_DrugId",
                table: "DrugExportations",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugExportations_ExportBatchId",
                table: "DrugExportations",
                column: "ExportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugExportBatches_ExporterUserId",
                table: "DrugExportBatches",
                column: "ExporterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugImportations_DrugId",
                table: "DrugImportations",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugImportations_ImportBatchId",
                table: "DrugImportations",
                column: "ImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugImportBatches_ImporterUserId",
                table: "DrugImportBatches",
                column: "ImporterUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DrugExportations");

            migrationBuilder.DropTable(
                name: "DrugImportations");

            migrationBuilder.DropTable(
                name: "DrugExportBatches");

            migrationBuilder.DropTable(
                name: "Drugs");

            migrationBuilder.DropTable(
                name: "DrugImportBatches");
        }
    }
}

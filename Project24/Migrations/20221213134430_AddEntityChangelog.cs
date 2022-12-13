using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Project24.Migrations
{
    public partial class AddEntityChangelog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerProfileChangelogs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedUserId = table.Column<string>(nullable: true),
                    ChangedDateTime = table.Column<DateTime>(nullable: false),
                    Operation = table.Column<int>(nullable: false),
                    ProfileId = table.Column<int>(nullable: false)
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
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedUserId = table.Column<string>(nullable: true),
                    ChangedDateTime = table.Column<DateTime>(nullable: false),
                    Operation = table.Column<int>(nullable: false),
                    ProfileId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketProfileChangelogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketProfileChangelogs_VisitingProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "VisitingProfiles",
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerProfileChangelogs");

            migrationBuilder.DropTable(
                name: "TicketProfileChangelogs");
        }
    }
}

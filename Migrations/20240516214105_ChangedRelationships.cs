using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollegeAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangedRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "College_Careers");

            migrationBuilder.AddColumn<int>(
                name: "CollegeId",
                table: "Careers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Careers_CollegeId",
                table: "Careers",
                column: "CollegeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Careers_Colleges_CollegeId",
                table: "Careers",
                column: "CollegeId",
                principalTable: "Colleges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Careers_Colleges_CollegeId",
                table: "Careers");

            migrationBuilder.DropIndex(
                name: "IX_Careers_CollegeId",
                table: "Careers");

            migrationBuilder.DropColumn(
                name: "CollegeId",
                table: "Careers");

            migrationBuilder.CreateTable(
                name: "College_Careers",
                columns: table => new
                {
                    CareerId = table.Column<int>(type: "int", nullable: false),
                    CollegeId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_College_Careers", x => new { x.CareerId, x.CollegeId });
                    table.ForeignKey(
                        name: "FK_College_Careers_Careers_CareerId",
                        column: x => x.CareerId,
                        principalTable: "Careers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_College_Careers_Colleges_CollegeId",
                        column: x => x.CollegeId,
                        principalTable: "Colleges",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_College_Careers_CollegeId",
                table: "College_Careers",
                column: "CollegeId");
        }
    }
}

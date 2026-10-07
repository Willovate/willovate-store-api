using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Willovate.Store.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddThemeEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pages_Websites_WebsiteId",
                table: "Pages");

            migrationBuilder.RenameColumn(
                name: "WebsiteId",
                table: "Pages",
                newName: "ThemeId");

            migrationBuilder.RenameIndex(
                name: "IX_Pages_WebsiteId_Slug",
                table: "Pages",
                newName: "IX_Pages_ThemeId_Slug");

            migrationBuilder.CreateTable(
                name: "Themes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WebsiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsLive = table.Column<bool>(type: "boolean", nullable: false),
                    LastEdited = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Themes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Themes_Websites_WebsiteId",
                        column: x => x.WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Themes_WebsiteId",
                table: "Themes",
                column: "WebsiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pages_Themes_ThemeId",
                table: "Pages",
                column: "ThemeId",
                principalTable: "Themes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pages_Themes_ThemeId",
                table: "Pages");

            migrationBuilder.DropTable(
                name: "Themes");

            migrationBuilder.RenameColumn(
                name: "ThemeId",
                table: "Pages",
                newName: "WebsiteId");

            migrationBuilder.RenameIndex(
                name: "IX_Pages_ThemeId_Slug",
                table: "Pages",
                newName: "IX_Pages_WebsiteId_Slug");

            migrationBuilder.AddForeignKey(
                name: "FK_Pages_Websites_WebsiteId",
                table: "Pages",
                column: "WebsiteId",
                principalTable: "Websites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

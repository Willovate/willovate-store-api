using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Willovate.Store.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplatesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TemplateId",
                table: "Templates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Templates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Subcategory",
                table: "Templates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Templates",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Templates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Badge",
                table: "Templates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviewImage",
                table: "Templates",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThemeConfiguration",
                table: "Templates",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SectionConfiguration",
                table: "Templates",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageConfiguration",
                table: "Templates",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Templates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Templates",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Templates",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: DateTimeOffset.UtcNow);

            migrationBuilder.CreateIndex(
                name: "IX_Templates_TemplateId",
                table: "Templates",
                column: "TemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Templates_TemplateId",
                table: "Templates");

            migrationBuilder.DropColumn(name: "TemplateId", table: "Templates");
            migrationBuilder.DropColumn(name: "Category", table: "Templates");
            migrationBuilder.DropColumn(name: "Subcategory", table: "Templates");
            migrationBuilder.DropColumn(name: "Description", table: "Templates");
            migrationBuilder.DropColumn(name: "Status", table: "Templates");
            migrationBuilder.DropColumn(name: "Badge", table: "Templates");
            migrationBuilder.DropColumn(name: "PreviewImage", table: "Templates");
            migrationBuilder.DropColumn(name: "ThemeConfiguration", table: "Templates");
            migrationBuilder.DropColumn(name: "SectionConfiguration", table: "Templates");
            migrationBuilder.DropColumn(name: "ImageConfiguration", table: "Templates");
            migrationBuilder.DropColumn(name: "DisplayOrder", table: "Templates");
            migrationBuilder.DropColumn(name: "IsAvailable", table: "Templates");
            migrationBuilder.DropColumn(name: "UpdatedAt", table: "Templates");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Willovate.Store.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddThemeMetadataFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Themes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Themes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StyleTags",
                table: "Themes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subcategory",
                table: "Themes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TemplateSlug",
                table: "Themes",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Themes");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Themes");

            migrationBuilder.DropColumn(
                name: "StyleTags",
                table: "Themes");

            migrationBuilder.DropColumn(
                name: "Subcategory",
                table: "Themes");

            migrationBuilder.DropColumn(
                name: "TemplateSlug",
                table: "Themes");
        }
    }
}

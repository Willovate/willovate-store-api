using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Willovate.Store.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddThemePriceAndThumbnail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Themes",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "Themes",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Themes");

            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "Themes");
        }
    }
}

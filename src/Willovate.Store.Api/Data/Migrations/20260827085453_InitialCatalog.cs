using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Willovate.Store.Api.Data.Migrations;

/// <inheritdoc />
public partial class InitialCatalog : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                Description = table.Column<string>(type: "character varying(1200)", maxLength: 1200, nullable: false),
                SearchText = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: false),
                Category = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                CategoryKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                CompareAtPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                StockQuantity = table.Column<int>(type: "integer", nullable: false),
                VisualTheme = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Products", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Products_CategoryKey",
            table: "Products",
            column: "CategoryKey");

        migrationBuilder.CreateIndex(
            name: "IX_Products_Slug",
            table: "Products",
            column: "Slug",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Products");
    }
}

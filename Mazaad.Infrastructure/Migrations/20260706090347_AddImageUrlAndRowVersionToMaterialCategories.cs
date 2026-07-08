using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mazaad.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlAndRowVersionToMaterialCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "image_url",
                table: "MaterialCategories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "MaterialCategories",
                type: "rowversion",
                rowVersion: true,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image_url",
                table: "MaterialCategories");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "MaterialCategories");
        }
    }
}
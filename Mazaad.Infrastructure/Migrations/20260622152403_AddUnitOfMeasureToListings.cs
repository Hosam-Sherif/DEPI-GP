using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mazaad.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitOfMeasureToListings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UnitOfMeasure",
                table: "Listings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitOfMeasure",
                table: "Listings");
        }
    }
}

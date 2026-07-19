using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mazaad.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Listing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Listings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByUserId",
                table: "Listings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Listings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Listings");
        }
    }
}

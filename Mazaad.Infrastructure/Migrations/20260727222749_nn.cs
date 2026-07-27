using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mazaad.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class nn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "Listings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CompaniesId",
                table: "Listings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SellerUserId",
                table: "Listings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Listings_CompaniesId",
                table: "Listings",
                column: "CompaniesId");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_SellerUserId",
                table: "Listings",
                column: "SellerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Listings_AspNetUsers_SellerUserId",
                table: "Listings",
                column: "SellerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Listings_Companies_CompaniesId",
                table: "Listings",
                column: "CompaniesId",
                principalTable: "Companies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Listings_AspNetUsers_SellerUserId",
                table: "Listings");

            migrationBuilder.DropForeignKey(
                name: "FK_Listings_Companies_CompaniesId",
                table: "Listings");

            migrationBuilder.DropIndex(
                name: "IX_Listings_CompaniesId",
                table: "Listings");

            migrationBuilder.DropIndex(
                name: "IX_Listings_SellerUserId",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "CompaniesId",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "SellerUserId",
                table: "Listings");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "Listings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}

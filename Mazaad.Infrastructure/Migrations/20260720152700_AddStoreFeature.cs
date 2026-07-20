using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mazaad.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReverseAuctions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuyerCompanyId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TechnicalSpecs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaxBudgetPerUnit = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    BaseCurrency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DeliveryLocation = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DeadlineDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AwardedOfferId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReverseAuctions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReverseAuctions_Companies_BuyerCompanyId",
                        column: x => x.BuyerCompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReverseAuctions_MaterialCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "MaterialCategories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stores_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReverseAuctionOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReverseAuctionId = table.Column<int>(type: "int", nullable: false),
                    SupplierCompanyId = table.Column<int>(type: "int", nullable: false),
                    PricePerUnit = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OfferedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DeliveryTerms = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DeliveryDays = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAwarded = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReverseAuctionOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReverseAuctionOffers_Companies_SupplierCompanyId",
                        column: x => x.SupplierCompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReverseAuctionOffers_ReverseAuctions_ReverseAuctionId",
                        column: x => x.ReverseAuctionId,
                        principalTable: "ReverseAuctions",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "CommissionPolicies",
                columns: new[] { "Id", "Active", "CommissionRate", "EffectiveFrom", "EffectiveTo", "MaxAmount", "MinAmount", "PolicyName" },
                values: new object[,]
                {
                    { 1, true, 0.02m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2030, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), 9999999m, 0m, "Standard 2%" },
                    { 2, true, 0.015m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2030, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), 99999999m, 500000m, "Premium 1.5% (High-Value)" }
                });

            migrationBuilder.InsertData(
                table: "IndustryTypes",
                columns: new[] { "Id", "CreatedAt", "IndustryName", "IsDeleted", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Steel & Metals", false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Plastics & Polymers", false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Construction", false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Chemicals", false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Agriculture", false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Electronics", false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Textiles", false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Food & Beverages", false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Energy & Fuel", false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Logistics", false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "MaterialCategories",
                columns: new[] { "Id", "CategoryName", "CreatedAt", "Description", "UnitOfMeasure", "image_url" },
                values: new object[,]
                {
                    { 1, "Carbon Steel", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Structural and engineering carbon steel", "Ton", "" },
                    { 2, "Stainless Steel", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Corrosion-resistant stainless steel grades", "Ton", "" },
                    { 3, "Copper & Alloys", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Pure copper and copper-based alloys", "Ton", "" },
                    { 4, "Aluminum", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Aluminum sheets, coils and extrusions", "Ton", "" },
                    { 5, "PVC Resin", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Polyvinyl chloride for pipes and profiles", "Ton", "" },
                    { 6, "HDPE / LDPE", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Polyethylene pellets for packaging & pipes", "Ton", "" },
                    { 7, "Cement & Clinker", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ordinary Portland cement and clinker", "Ton", "" },
                    { 8, "Chemicals — Solvents", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Industrial organic and inorganic solvents", "L", "" },
                    { 9, "Grains & Pulses", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Wheat, corn, lentils and agricultural grains", "Ton", "" },
                    { 10, "Crude Oil Derivatives", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Fuel oil, naphtha and petroleum distillates", "Barrel", "" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReverseAuctionOffers_ReverseAuctionId_SupplierCompanyId",
                table: "ReverseAuctionOffers",
                columns: new[] { "ReverseAuctionId", "SupplierCompanyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReverseAuctionOffers_SupplierCompanyId",
                table: "ReverseAuctionOffers",
                column: "SupplierCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ReverseAuctions_BuyerCompanyId",
                table: "ReverseAuctions",
                column: "BuyerCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ReverseAuctions_CategoryId",
                table: "ReverseAuctions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ReverseAuctions_DeadlineDate",
                table: "ReverseAuctions",
                column: "DeadlineDate");

            migrationBuilder.CreateIndex(
                name: "IX_ReverseAuctions_Status",
                table: "ReverseAuctions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_CompanyId",
                table: "Stores",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_Slug",
                table: "Stores",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReverseAuctionOffers");

            migrationBuilder.DropTable(
                name: "Stores");

            migrationBuilder.DropTable(
                name: "ReverseAuctions");

            migrationBuilder.DeleteData(
                table: "CommissionPolicies",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "CommissionPolicies",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "IndustryTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "IndustryTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "IndustryTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "IndustryTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "IndustryTypes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "IndustryTypes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "IndustryTypes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "IndustryTypes",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "IndustryTypes",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "IndustryTypes",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "MaterialCategories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MaterialCategories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MaterialCategories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MaterialCategories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "MaterialCategories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "MaterialCategories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "MaterialCategories",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "MaterialCategories",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "MaterialCategories",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "MaterialCategories",
                keyColumn: "Id",
                keyValue: 10);
        }
    }
}
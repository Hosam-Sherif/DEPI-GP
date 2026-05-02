using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mazaad.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingFieldsAndSeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Orders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Listings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "BaseCurrency",
                table: "Listings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "BidCount",
                table: "Listings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Condition",
                table: "Listings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DueDiligenceUrls",
                table: "Listings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Listings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Listings",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Listings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TechnicalSpecs",
                table: "Listings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Bids",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "CommissionPolicies",
                columns: new[] { "Id", "Active", "CommissionRate", "EffectiveFrom", "EffectiveTo", "MaxAmount", "MinAmount", "PolicyName" },
                values: new object[] { 1, true, 2.5m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2027, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), 10000000m, 10000m, "Standard Platform Fee" });

            migrationBuilder.InsertData(
                table: "IndustryTypes",
                columns: new[] { "Id", "CreatedAt", "IndustryName", "IsDeleted", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Medical Devices", false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Heavy Machinery", false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Data Centers", false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Real Estate", false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Robotics", false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "MaterialCategories",
                columns: new[] { "Id", "CategoryName", "CreatedAt", "Description", "UnitOfMeasure" },
                values: new object[,]
                {
                    { 1, "Medical Equipment", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Diagnostic and therapeutic medical devices", "Unit" },
                    { 2, "Construction Machinery", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Heavy construction and earthmoving equipment", "Unit" },
                    { 3, "IT Infrastructure", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Servers, networking equipment and cooling systems", "Unit" },
                    { 4, "Fleet Vehicles", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Commercial and fleet vehicle bundles", "Fleet" },
                    { 5, "Real Estate", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Commercial and industrial real estate", "sqm" },
                    { 6, "Industrial Robots", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Robotic arms and automated production lines", "Unit" }
                });

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "AddressDetails", "City", "CommercialRegNum", "CompanyName", "CreatedAt", "IndustryId", "IsVerified", "TaxRegistrationNum", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Industrial Park West, Gate 12, Munich 80331, DE", "Munich", "CR-001-2025", "Siemens Healthineers MENA", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, "TRN-001-2025", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "Al Quoz Industrial Area 3, Dubai, UAE", "Dubai", "CR-002-2025", "Gulf Heavy Equipment LLC", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, true, "TRN-002-2025", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "2400 Technology Drive, Austin TX 78745, US", "Austin", "CR-003-2025", "Hydro Cooling Solutions", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, true, "TRN-003-2025", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "AppUsers",
                columns: new[] { "Id", "CompanyId", "CreatedAt", "Email", "FullName", "IsActive", "JobTitle", "LastLoginDate", "PasswordHash" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ahmed@siemens-mena.com", "Ahmed Al-Rashid", true, "Asset Manager", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "hashed_password_placeholder" },
                    { 2, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "sara@gulhheavy.com", "Sara Johnson", true, "Procurement Officer", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "hashed_password_placeholder" }
                });

            migrationBuilder.InsertData(
                table: "Listings",
                columns: new[] { "Id", "AvailableQuantity", "BaseCurrency", "BidCount", "CategoryId", "CompanyId", "Condition", "CreatedAt", "CurrentHighestBid", "Description", "DueDiligenceUrls", "EndDate", "ImageUrl", "IsDeleted", "Location", "MinOrderQuantity", "PurityPercentage", "StartDate", "Status", "TechnicalSpecs", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1m, "USD", 12, 1, 1, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3420000m, "Deutsche Klinik Regional Distribution Center · Condition: Certified/Refurbished. Full-body 3 Tesla MRI system with 70cm bore.", "inspection_report.pdf,maintenance_log.pdf,terms_of_sale.pdf", new DateTime(2026, 12, 31, 23, 59, 59, 0, DateTimeKind.Utc), "https://images.unsplash.com/photo-1519494026892-80bbd2d6fd0d?w=800", false, "Industrial Park West, Gate 12, Munich 80331, DE", 1m, 100m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "{\"FieldStrength\":\"3 Tesla\",\"BoreSize\":\"70 cm\",\"ManufactureDate\":\"October 2021\",\"ImportStatus\":\"Documentation/Cited\",\"SoftwareVersion\":\"Syngo MR E11\"}", "Siemens Healthineers Lumina 3T MRI Suite", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 3m, "USD", 8, 2, 2, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 285000m, "Late 2024 model, 49-tonne hydraulic excavator with advanced automation package.", "excavator_inspection.pdf", new DateTime(2026, 12, 31, 23, 59, 59, 0, DateTimeKind.Utc), "https://images.unsplash.com/photo-1581092160562-40aa08e78837?w=800", false, "Al Quoz Industrial Area 3, Dubai, UAE", 1m, 100m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "{\"OperatingWeight\":\"49 tonnes\",\"NetPower\":\"396 HP\",\"MaxDigDepth\":\"7.42m\",\"Condition\":\"New\"}", "Caterpillar 349 Next Gen Hydraulic Excavator", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 2m, "USD", 5, 3, 3, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1240000m, "128x NVIDIA H100 80GB SXM5 GPUs in liquid-cooled rack cluster configuration.", "cluster_specs.pdf,warranty.pdf", new DateTime(2026, 12, 31, 23, 59, 59, 0, DateTimeKind.Utc), "https://images.unsplash.com/photo-1518770660439-4636190af475?w=800", false, "2400 Technology Drive, Austin TX 78745, US", 1m, 100m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "{\"GPUs\":\"128x NVIDIA H100 80GB\",\"CoolingType\":\"Liquid\",\"TDP\":\"700W per GPU\",\"Interconnect\":\"NVLink 4.0\"}", "Bulk Liquid-Cooled GPU Cluster (128x H100)", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, 10m, "USD", 3, 4, 2, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 640000m, "10x 2026 electric delivery vans, 400km range, pre-configured with fleet management software.", "fleet_inspection.pdf", new DateTime(2026, 12, 31, 23, 59, 59, 0, DateTimeKind.Utc), "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=800", false, "Dubai Fleet Depot, Al Quoz, UAE", 10m, 100m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "{\"Units\":10,\"Range\":\"400 km\",\"Payload\":\"900 kg\",\"Charging\":\"DC Fast 150kW\"}", "2026 EV Delivery Fleet Bundle (10 Units)", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, 1m, "USD", 2, 5, 1, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 18500000m, "Grade-A commercial office building, 12 floors, 15,000 sqm GLA, fully tenanted.", "property_report.pdf,tenancy_schedule.pdf", new DateTime(2026, 12, 31, 23, 59, 59, 0, DateTimeKind.Utc), "https://images.unsplash.com/photo-1486325212027-8081e485255e?w=800", false, "Downtown Business District, Munich, DE", 1m, 100m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "{\"GLA\":\"15000 sqm\",\"Floors\":12,\"Occupancy\":\"97%\",\"Certification\":\"LEED Gold\"}", "Downtown Financial District Office Complex", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, 5m, "USD", 7, 6, 3, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 192500m, "5x FANUC R-2000iC/165F robotic arms, fully integrated production line, 2023 vintage.", "robot_certifications.pdf,maintenance_history.pdf", new DateTime(2026, 12, 31, 23, 59, 59, 0, DateTimeKind.Utc), "https://images.unsplash.com/photo-1535378917042-10a22c95931a?w=800", false, "2400 Technology Drive, Austin TX 78745, US", 5m, 100m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "{\"Units\":5,\"Payload\":\"165 kg\",\"Reach\":\"2655 mm\",\"Repeatability\":\"±0.05 mm\",\"Year\":2023}", "FANUC R-2000iC Production Line (5 Units)", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "CommissionPolicies",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "IndustryTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "IndustryTypes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Listings",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Listings",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Listings",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Listings",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Listings",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Listings",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 3);

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

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BidCount",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "Condition",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "DueDiligenceUrls",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "TechnicalSpecs",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Bids");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Listings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "BaseCurrency",
                table: "Listings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);
        }
    }
}

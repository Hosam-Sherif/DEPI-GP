using Mazaad.Domain.Enums;
using Mazaad.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Mazaad.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // ─── DbSets ───────────────────────────────────────────────────────────────

        public DbSet<Companies> Companies { get; set; }
        public DbSet<IndustryType> IndustryTypes { get; set; }
        public DbSet<App_Users> AppUsers { get; set; }
        public DbSet<Listings> Listings { get; set; }
        public DbSet<Bids> Bids { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<Payments> Payments { get; set; }
        public DbSet<Commission_Policies> CommissionPolicies { get; set; }
        public DbSet<Chat_Channels> ChatChannels { get; set; }
        public DbSet<Messages> Messages { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
        public DbSet<Material_Categories> MaterialCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ─── Relationship Configuration ──────────────────────────────────────

            // Chat_Channels ↔ SellerCompany
            modelBuilder.Entity<Chat_Channels>()
                .HasOne(cc => cc.SellerCompany)
                .WithMany(c => c.SellerChatChannels)
                .HasForeignKey(cc => cc.SellerCompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            // Chat_Channels ↔ BuyerCompany
            modelBuilder.Entity<Chat_Channels>()
                .HasOne(cc => cc.BuyerCompany)
                .WithMany(c => c.BuyerChatChannels)
                .HasForeignKey(cc => cc.BuyerCompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            // Orders ↔ SellerCompany
            modelBuilder.Entity<Orders>()
                .HasOne(o => o.SellerCompany)
                .WithMany(c => c.SalesOrders)
                .HasForeignKey(o => o.SellerCompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            // Orders ↔ BuyerCompany
            modelBuilder.Entity<Orders>()
                .HasOne(o => o.BuyerCompany)
                .WithMany(c => c.PurchaseOrders)
                .HasForeignKey(o => o.BuyerCompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            // Bids ↔ BuyerCompany
            modelBuilder.Entity<Bids>()
                .HasOne(b => b.BuyerCompany)
                .WithMany(c => c.Bids)
                .HasForeignKey(b => b.BuyerCompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            // Apply NoAction to all remaining FK relationships to prevent cascades
            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                         .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }

            // ─── Seed Data ───────────────────────────────────────────────────────

            SeedIndustryTypes(modelBuilder);
            SeedCompanies(modelBuilder);
            SeedAppUsers(modelBuilder);
            SeedMaterialCategories(modelBuilder);
            SeedCommissionPolicies(modelBuilder);
            SeedListings(modelBuilder);
        }

        // ─── Seed Methods ─────────────────────────────────────────────────────────

        private static void SeedIndustryTypes(ModelBuilder modelBuilder)
        {
            var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            modelBuilder.Entity<IndustryType>().HasData(
                new IndustryType { Id = 1, IndustryName = "Medical Devices", CreatedAt = now, UpdatedAt = now, IsDeleted = false },
                new IndustryType { Id = 2, IndustryName = "Heavy Machinery", CreatedAt = now, UpdatedAt = now, IsDeleted = false },
                new IndustryType { Id = 3, IndustryName = "Data Centers", CreatedAt = now, UpdatedAt = now, IsDeleted = false },
                new IndustryType { Id = 4, IndustryName = "Real Estate", CreatedAt = now, UpdatedAt = now, IsDeleted = false },
                new IndustryType { Id = 5, IndustryName = "Robotics", CreatedAt = now, UpdatedAt = now, IsDeleted = false }
            );
        }

        private static void SeedCompanies(ModelBuilder modelBuilder)
        {
            var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            modelBuilder.Entity<Companies>().HasData(
                new Companies
                {
                    Id = 1,
                    IndustryId = 1,
                    CompanyName = "Siemens Healthineers MENA",
                    CommercialRegNum = "CR-001-2025",
                    TaxRegistrationNum = "TRN-001-2025",
                    City = "Munich",
                    AddressDetails = "Industrial Park West, Gate 12, Munich 80331, DE",
                    IsVerified = true,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Companies
                {
                    Id = 2,
                    IndustryId = 2,
                    CompanyName = "Gulf Heavy Equipment LLC",
                    CommercialRegNum = "CR-002-2025",
                    TaxRegistrationNum = "TRN-002-2025",
                    City = "Dubai",
                    AddressDetails = "Al Quoz Industrial Area 3, Dubai, UAE",
                    IsVerified = true,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Companies
                {
                    Id = 3,
                    IndustryId = 3,
                    CompanyName = "Hydro Cooling Solutions",
                    CommercialRegNum = "CR-003-2025",
                    TaxRegistrationNum = "TRN-003-2025",
                    City = "Austin",
                    AddressDetails = "2400 Technology Drive, Austin TX 78745, US",
                    IsVerified = true,
                    CreatedAt = now,
                    UpdatedAt = now
                }
            );
        }

        private static void SeedAppUsers(ModelBuilder modelBuilder)
        {
            var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            modelBuilder.Entity<App_Users>().HasData(
                new App_Users
                {
                    Id = 1,
                    CompanyId = 1,
                    FullName = "Ahmed Al-Rashid",
                    Email = "ahmed@siemens-mena.com",
                    PasswordHash = "hashed_password_placeholder",
                    JobTitle = "Asset Manager",
                    IsActive = true,
                    LastLoginDate = now,
                    CreatedAt = now
                },
                new App_Users
                {
                    Id = 2,
                    CompanyId = 2,
                    FullName = "Sara Johnson",
                    Email = "sara@gulhheavy.com",
                    PasswordHash = "hashed_password_placeholder",
                    JobTitle = "Procurement Officer",
                    IsActive = true,
                    LastLoginDate = now,
                    CreatedAt = now
                }
            );
        }

        private static void SeedMaterialCategories(ModelBuilder modelBuilder)
        {
            var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            modelBuilder.Entity<Material_Categories>().HasData(
                new Material_Categories { Id = 1, CategoryName = "Medical Equipment", Description = "Diagnostic and therapeutic medical devices", UnitOfMeasure = "Unit", CreatedAt = now },
                new Material_Categories { Id = 2, CategoryName = "Construction Machinery", Description = "Heavy construction and earthmoving equipment", UnitOfMeasure = "Unit", CreatedAt = now },
                new Material_Categories { Id = 3, CategoryName = "IT Infrastructure", Description = "Servers, networking equipment and cooling systems", UnitOfMeasure = "Unit", CreatedAt = now },
                new Material_Categories { Id = 4, CategoryName = "Fleet Vehicles", Description = "Commercial and fleet vehicle bundles", UnitOfMeasure = "Fleet", CreatedAt = now },
                new Material_Categories { Id = 5, CategoryName = "Real Estate", Description = "Commercial and industrial real estate", UnitOfMeasure = "sqm", CreatedAt = now },
                new Material_Categories { Id = 6, CategoryName = "Industrial Robots", Description = "Robotic arms and automated production lines", UnitOfMeasure = "Unit", CreatedAt = now }
            );
        }

        private static void SeedCommissionPolicies(ModelBuilder modelBuilder)
        {
            var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            modelBuilder.Entity<Commission_Policies>().HasData(
                new Commission_Policies
                {
                    Id = 1,
                    PolicyName = "Standard Platform Fee",
                    CommissionRate = 2.5m,
                    MinAmount = 10000m,
                    MaxAmount = 10000000m,
                    EffectiveFrom = now,
                    EffectiveTo = new DateTime(2027, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    Active = true
                }
            );
        }

        private static void SeedListings(ModelBuilder modelBuilder)
        {
            var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var futureEnd = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc);

            modelBuilder.Entity<Listings>().HasData(
                new Listings
                {
                    Id = 1,
                    CompanyId = 1,
                    CategoryId = 1,
                    Title = "Siemens Healthineers Lumina 3T MRI Suite",
                    Description = "Deutsche Klinik Regional Distribution Center · Condition: Certified/Refurbished. Full-body 3 Tesla MRI system with 70cm bore.",
                    TechnicalSpecs = "{\"FieldStrength\":\"3 Tesla\",\"BoreSize\":\"70 cm\",\"ManufactureDate\":\"October 2021\",\"ImportStatus\":\"Documentation/Cited\",\"SoftwareVersion\":\"Syngo MR E11\"}",
                    MinOrderQuantity = 1,
                    AvailableQuantity = 1,
                    PurityPercentage = 100,
                    BaseCurrency = "USD",
                    CurrentHighestBid = 3420000m,
                    BidCount = 12,
                    Status = ListingStatus.Active,
                    Condition = ListingCondition.Certified,
                    ImageUrl = "https://images.unsplash.com/photo-1519494026892-80bbd2d6fd0d?w=800",
                    Location = "Industrial Park West, Gate 12, Munich 80331, DE",
                    DueDiligenceUrls = "inspection_report.pdf,maintenance_log.pdf,terms_of_sale.pdf",
                    StartDate = now,
                    EndDate = futureEnd,
                    IsDeleted = false,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Listings
                {
                    Id = 2,
                    CompanyId = 2,
                    CategoryId = 2,
                    Title = "Caterpillar 349 Next Gen Hydraulic Excavator",
                    Description = "Late 2024 model, 49-tonne hydraulic excavator with advanced automation package.",
                    TechnicalSpecs = "{\"OperatingWeight\":\"49 tonnes\",\"NetPower\":\"396 HP\",\"MaxDigDepth\":\"7.42m\",\"Condition\":\"New\"}",
                    MinOrderQuantity = 1,
                    AvailableQuantity = 3,
                    PurityPercentage = 100,
                    BaseCurrency = "USD",
                    CurrentHighestBid = 285000m,
                    BidCount = 8,
                    Status = ListingStatus.Active,
                    Condition = ListingCondition.New,
                    ImageUrl = "https://images.unsplash.com/photo-1581092160562-40aa08e78837?w=800",
                    Location = "Al Quoz Industrial Area 3, Dubai, UAE",
                    DueDiligenceUrls = "excavator_inspection.pdf",
                    StartDate = now,
                    EndDate = futureEnd,
                    IsDeleted = false,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Listings
                {
                    Id = 3,
                    CompanyId = 3,
                    CategoryId = 3,
                    Title = "Bulk Liquid-Cooled GPU Cluster (128x H100)",
                    Description = "128x NVIDIA H100 80GB SXM5 GPUs in liquid-cooled rack cluster configuration.",
                    TechnicalSpecs = "{\"GPUs\":\"128x NVIDIA H100 80GB\",\"CoolingType\":\"Liquid\",\"TDP\":\"700W per GPU\",\"Interconnect\":\"NVLink 4.0\"}",
                    MinOrderQuantity = 1,
                    AvailableQuantity = 2,
                    PurityPercentage = 100,
                    BaseCurrency = "USD",
                    CurrentHighestBid = 1240000m,
                    BidCount = 5,
                    Status = ListingStatus.Active,
                    Condition = ListingCondition.New,
                    ImageUrl = "https://images.unsplash.com/photo-1518770660439-4636190af475?w=800",
                    Location = "2400 Technology Drive, Austin TX 78745, US",
                    DueDiligenceUrls = "cluster_specs.pdf,warranty.pdf",
                    StartDate = now,
                    EndDate = futureEnd,
                    IsDeleted = false,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Listings
                {
                    Id = 4,
                    CompanyId = 2,
                    CategoryId = 4,
                    Title = "2026 EV Delivery Fleet Bundle (10 Units)",
                    Description = "10x 2026 electric delivery vans, 400km range, pre-configured with fleet management software.",
                    TechnicalSpecs = "{\"Units\":10,\"Range\":\"400 km\",\"Payload\":\"900 kg\",\"Charging\":\"DC Fast 150kW\"}",
                    MinOrderQuantity = 10,
                    AvailableQuantity = 10,
                    PurityPercentage = 100,
                    BaseCurrency = "USD",
                    CurrentHighestBid = 640000m,
                    BidCount = 3,
                    Status = ListingStatus.Active,
                    Condition = ListingCondition.New,
                    ImageUrl = "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=800",
                    Location = "Dubai Fleet Depot, Al Quoz, UAE",
                    DueDiligenceUrls = "fleet_inspection.pdf",
                    StartDate = now,
                    EndDate = futureEnd,
                    IsDeleted = false,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Listings
                {
                    Id = 5,
                    CompanyId = 1,
                    CategoryId = 5,
                    Title = "Downtown Financial District Office Complex",
                    Description = "Grade-A commercial office building, 12 floors, 15,000 sqm GLA, fully tenanted.",
                    TechnicalSpecs = "{\"GLA\":\"15000 sqm\",\"Floors\":12,\"Occupancy\":\"97%\",\"Certification\":\"LEED Gold\"}",
                    MinOrderQuantity = 1,
                    AvailableQuantity = 1,
                    PurityPercentage = 100,
                    BaseCurrency = "USD",
                    CurrentHighestBid = 18500000m,
                    BidCount = 2,
                    Status = ListingStatus.Active,
                    Condition = ListingCondition.Certified,
                    ImageUrl = "https://images.unsplash.com/photo-1486325212027-8081e485255e?w=800",
                    Location = "Downtown Business District, Munich, DE",
                    DueDiligenceUrls = "property_report.pdf,tenancy_schedule.pdf",
                    StartDate = now,
                    EndDate = futureEnd,
                    IsDeleted = false,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Listings
                {
                    Id = 6,
                    CompanyId = 3,
                    CategoryId = 6,
                    Title = "FANUC R-2000iC Production Line (5 Units)",
                    Description = "5x FANUC R-2000iC/165F robotic arms, fully integrated production line, 2023 vintage.",
                    TechnicalSpecs = "{\"Units\":5,\"Payload\":\"165 kg\",\"Reach\":\"2655 mm\",\"Repeatability\":\"±0.05 mm\",\"Year\":2023}",
                    MinOrderQuantity = 5,
                    AvailableQuantity = 5,
                    PurityPercentage = 100,
                    BaseCurrency = "USD",
                    CurrentHighestBid = 192500m,
                    BidCount = 7,
                    Status = ListingStatus.Active,
                    Condition = ListingCondition.Certified,
                    ImageUrl = "https://images.unsplash.com/photo-1535378917042-10a22c95931a?w=800",
                    Location = "2400 Technology Drive, Austin TX 78745, US",
                    DueDiligenceUrls = "robot_certifications.pdf,maintenance_history.pdf",
                    StartDate = now,
                    EndDate = futureEnd,
                    IsDeleted = false,
                    CreatedAt = now,
                    UpdatedAt = now
                }
            );
        }
    }
}

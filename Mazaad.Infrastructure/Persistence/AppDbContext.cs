// Mazaad.Infrastructure/Persistence/AppDbContext.cs

using Mazaad.Domain.Enums;
using Mazaad.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Mazaad.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        int,
        IdentityUserClaim<int>,
        IdentityUserRole<int>,
        IdentityUserLogin<int>,
        IdentityRoleClaim<int>,
        IdentityUserToken<int>>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ── Auth Module ───────────────────────────────────────────────────────
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<SecurityLog> SecurityLogs { get; set; }
        public DbSet<CompanyDocument> CompanyDocuments { get; set; }

        // ── Existing Tables ───────────────────────────────────────────────────
        public DbSet<Companies> Companies { get; set; }
        public DbSet<IndustryType> IndustryTypes { get; set; }
        public DbSet<Listings> Listings { get; set; }
        public DbSet<Bids> Bids { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<Payments> Payments { get; set; }
        public DbSet<Commission_Policies> CommissionPolicies { get; set; }
        public DbSet<Chat_Channels> ChatChannels { get; set; }
        public DbSet<Messages> Messages { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
        public DbSet<Material_Categories> MaterialCategories { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }

        // ── Reverse Auction ───────────────────────────────────────────────────
        public DbSet<ReverseAuction> ReverseAuctions { get; set; }
        public DbSet<ReverseAuctionOffer> ReverseAuctionOffers { get; set; }
        // ── Store ──────────────────────────────────────────────────────────────────
        public DbSet<Store> Stores { get; set; }

        // ── Escrow & Payout Module ────────────────────────────────────────────
        public DbSet<EscrowRecord> EscrowRecords { get; set; }
        public DbSet<PayoutRecord> PayoutRecords { get; set; }
        public DbSet<SellerBankAccount> SellerBankAccounts { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── ApplicationUser ───────────────────────────────────────────────
            modelBuilder.Entity<ApplicationUser>(b =>
            {
                b.HasOne(u => u.Company)
                 .WithMany(c => c.Users)
                 .HasForeignKey(u => u.CompanyId)
                 .OnDelete(DeleteBehavior.NoAction)
                 .IsRequired(false);

                b.HasMany(u => u.Messages)
                 .WithOne(m => m.SenderUser)
                 .HasForeignKey(m => m.SenderUserId)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasMany(u => u.Bids)
                 .WithOne(bid => bid.User)
                 .HasForeignKey(bid => bid.PlacedByUserId)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasMany(u => u.Notifications)
                 .WithOne(n => n.User)
                 .HasForeignKey(n => n.UserId)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            // ── Bids ──────────────────────────────────────────────────────────
            modelBuilder.Entity<Bids>(b =>
            {
                b.HasOne(bid => bid.BuyerCompany)
                 .WithMany(c => c.Bids)
                 .HasForeignKey(bid => bid.BuyerCompanyId)
                 .OnDelete(DeleteBehavior.NoAction)
                 .IsRequired(false);

                b.Property(bid => bid.BidAmountPerUnit).HasPrecision(18, 4);
                b.Property(bid => bid.Quantity).HasPrecision(18, 4);
                b.Property(bid => bid.TotalBidAmount).HasPrecision(18, 4);
            });

            // ── Listings ─────────────────────────────────────────────────────
            modelBuilder.Entity<Listings>(l =>
            {
                l.Property(x => x.CurrentHighestBid).HasPrecision(18, 4);
                l.Property(x => x.MinOrderQuantity).HasPrecision(18, 4);
                l.Property(x => x.AvailableQuantity).HasPrecision(18, 4);
                l.Property(x => x.PurityPercentage).HasPrecision(5, 2);

                // A listing belongs to EITHER a company OR an individual seller.
                l.HasOne(x => x.Company)
                 .WithMany()
                 .HasForeignKey(x => x.CompanyId)
                 .OnDelete(DeleteBehavior.NoAction)
                 .IsRequired(false);

                l.HasOne(x => x.Seller)
                 .WithMany()
                 .HasForeignKey(x => x.SellerUserId)
                 .OnDelete(DeleteBehavior.NoAction)
                 .IsRequired(false);
            });

            // ── RefreshToken ──────────────────────────────────────────────────
            modelBuilder.Entity<RefreshToken>(b =>
            {
                b.HasKey(r => r.Id);

                b.HasOne(r => r.User)
                 .WithMany(u => u.RefreshTokens)
                 .HasForeignKey(r => r.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(r => r.Token).IsUnique();
            });

            // ── SecurityLog ───────────────────────────────────────────────────
            modelBuilder.Entity<SecurityLog>(b =>
            {
                b.HasKey(s => s.Id);

                b.HasOne(s => s.User)
                 .WithMany(u => u.SecurityLogs)
                 .HasForeignKey(s => s.UserId)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasIndex(s => s.CreatedAt);
                b.HasIndex(s => s.EventType);
                b.HasIndex(s => s.UserId);
            });

            // ── Payments ─────────────────────────────────────────────────────
            modelBuilder.Entity<Payments>().Property(x => x.Amount).HasPrecision(18, 4);

            // ── Commission_Policies ────────────────────────────────────────────
            modelBuilder.Entity<Commission_Policies>(cp =>
            {
                cp.Property(x => x.CommissionRate).HasPrecision(8, 4);
                cp.Property(x => x.MinAmount).HasPrecision(18, 4);
                cp.Property(x => x.MaxAmount).HasPrecision(18, 4);
            });

            // ── InventoryItem ──────────────────────────────────────────────────
            modelBuilder.Entity<InventoryItem>(inv =>
            {
                inv.Property(x => x.quantity).HasPrecision(18, 4);
                inv.Property(x => x.minimum_auction_price).HasPrecision(18, 4);
                inv.Property(x => x.current_market_price).HasPrecision(18, 4);
            });
            modelBuilder.Entity<RefreshToken>(b =>
            {
                b.HasKey(r => r.Id);

                b.HasOne(r => r.User)
                 .WithMany(u => u.RefreshTokens)
                 .HasForeignKey(r => r.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(r => r.Token).IsUnique();
            });

            // ── CompanyDocument ───────────────────────────────────────────────
            modelBuilder.Entity<CompanyDocument>(b =>
            {
                b.HasKey(d => d.Id);

                b.HasOne(d => d.Company)
                 .WithMany()
                 .HasForeignKey(d => d.CompanyId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(d => d.UploadedByUser)
                 .WithMany()
                 .HasForeignKey(d => d.UploadedByUserId)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            // ── Companies ─────────────────────────────────────────────────────
            modelBuilder.Entity<Companies>(b =>
            {
                b.HasOne(c => c.Industry)
                 .WithMany(i => i.Companies)
                 .HasForeignKey(c => c.IndustryId)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasOne<ApplicationUser>()
                 .WithMany()
                 .HasForeignKey(c => c.VerifiedByUserId)
                 .OnDelete(DeleteBehavior.NoAction)
                 .IsRequired(false);
            });

            // ── Chat_Channels ─────────────────────────────────────────────────
            modelBuilder.Entity<Chat_Channels>()
                .HasOne(cc => cc.SellerCompany)
                .WithMany(c => c.SellerChatChannels)
                .HasForeignKey(cc => cc.SellerCompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Chat_Channels>()
                .HasOne(cc => cc.BuyerCompany)
                .WithMany(c => c.BuyerChatChannels)
                .HasForeignKey(cc => cc.BuyerCompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── Orders ────────────────────────────────────────────────────────
            modelBuilder.Entity<Orders>()
                .HasOne(o => o.SellerCompany)
                .WithMany(c => c.SalesOrders)
                .HasForeignKey(o => o.SellerCompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Orders>()
                .HasOne(o => o.BuyerCompany)
                .WithMany(c => c.PurchaseOrders)
                .HasForeignKey(o => o.BuyerCompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Orders>(o =>
            {
                o.Property(x => x.AgreedQuantity).HasPrecision(18, 4);
                o.Property(x => x.AgreedUnitPrice).HasPrecision(18, 4);
                o.Property(x => x.TotalAmount).HasPrecision(18, 4);
                o.Property(x => x.PlatformFee).HasPrecision(18, 4);
            });


            // ── ReverseAuction ────────────────────────────────────────────────
            modelBuilder.Entity<ReverseAuction>(ra =>
            {
                ra.Property(x => x.RequiredQuantity).HasPrecision(18, 4);
                ra.Property(x => x.MaxBudgetPerUnit).HasPrecision(18, 4);

                ra.HasOne(x => x.BuyerCompany)
                  .WithMany()
                  .HasForeignKey(x => x.BuyerCompanyId)
                  .OnDelete(DeleteBehavior.NoAction);

                ra.HasOne(x => x.Category)
                  .WithMany()
                  .HasForeignKey(x => x.CategoryId)
                  .OnDelete(DeleteBehavior.NoAction);

                ra.HasMany(x => x.Offers)
                  .WithOne(o => o.ReverseAuction)
                  .HasForeignKey(o => o.ReverseAuctionId)
                  .OnDelete(DeleteBehavior.Cascade);

                ra.HasIndex(x => x.Status);
                ra.HasIndex(x => x.BuyerCompanyId);
                ra.HasIndex(x => x.DeadlineDate);
            });

            modelBuilder.Entity<ReverseAuctionOffer>(o =>
            {
                o.Property(x => x.PricePerUnit).HasPrecision(18, 4);
                o.Property(x => x.TotalPrice).HasPrecision(18, 4);
                o.Property(x => x.OfferedQuantity).HasPrecision(18, 4);

                o.HasOne(x => x.SupplierCompany)
                 .WithMany()
                 .HasForeignKey(x => x.SupplierCompanyId)
                 .OnDelete(DeleteBehavior.NoAction);

                // قاعدة عمل: عرض واحد فقط لكل مورّد لكل طلب
                o.HasIndex(x => new { x.ReverseAuctionId, x.SupplierCompanyId }).IsUnique();
                o.HasIndex(x => x.SupplierCompanyId);
            });

            // ── Store ─────────────────────────────────────────────────────────────────
            modelBuilder.Entity<Store>(b =>
            {
                b.HasKey(s => s.Id);

                b.HasOne(s => s.Company)
                 .WithOne(c => c.Store)
                 .HasForeignKey<Store>(s => s.CompanyId)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasIndex(s => s.Slug).IsUnique();
                b.HasIndex(s => s.CompanyId).IsUnique(); // شركة واحدة = متجر واحد
            });

            // ── RefreshToken ──────────────────────────────────────────────────
            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                         .SelectMany(e => e.GetForeignKeys()))
            {
                if (relationship.DeleteBehavior == DeleteBehavior.Cascade
                    && relationship.DeclaringEntityType.ClrType != typeof(RefreshToken))
                {
                    relationship.DeleteBehavior = DeleteBehavior.NoAction;
                }
            }


            // ── Bids ──────────────────────────────────────────────────────────
            modelBuilder.Entity<Bids>(b =>
            {
                b.HasOne(bid => bid.BuyerCompany)
                 .WithMany(c => c.Bids)
                 .HasForeignKey(bid => bid.BuyerCompanyId)
                 .OnDelete(DeleteBehavior.NoAction)
                 .IsRequired(false);

                b.Property(bid => bid.BidAmountPerUnit).HasPrecision(18, 4);
                b.Property(bid => bid.Quantity).HasPrecision(18, 4);
                b.Property(bid => bid.TotalBidAmount).HasPrecision(18, 4);
            });

            // ── Escrow & Payout Module Configurations ─────────────────────────
            modelBuilder.Entity<EscrowRecord>(b =>
            {
                b.HasOne(e => e.Order)
                 .WithOne(o => o.Escrow)
                 .HasForeignKey<EscrowRecord>(e => e.OrderId)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(e => e.SourcePayment)
                 .WithMany()
                 .HasForeignKey(e => e.SourcePaymentId)
                 .OnDelete(DeleteBehavior.NoAction);

                b.Property(e => e.AmountHeld).HasPrecision(18, 4);
                b.Property(e => e.PlatformFee).HasPrecision(18, 4);
                b.Property(e => e.SellerDueAmount).HasPrecision(18, 4);
            });

            modelBuilder.Entity<PayoutRecord>(b =>
            {
                b.HasOne(p => p.EscrowRecord)
                 .WithMany(e => e.Payouts)
                 .HasForeignKey(p => p.EscrowRecordId)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(p => p.SellerCompany)
                 .WithMany()
                 .HasForeignKey(p => p.SellerCompanyId)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(p => p.DestinationAccount)
                 .WithMany(a => a.Payouts)
                 .HasForeignKey(p => p.SellerBankAccountId)
                 .OnDelete(DeleteBehavior.NoAction);

                b.Property(p => p.Amount).HasPrecision(18, 4);
            });

            modelBuilder.Entity<SellerBankAccount>(b =>
            {
                b.HasOne(a => a.Company)
                 .WithMany()
                 .HasForeignKey(a => a.CompanyId)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(a => a.VerifiedBy)
                 .WithMany()
                 .HasForeignKey(a => a.VerifiedByUserId)
                 .OnDelete(DeleteBehavior.NoAction)
                 .IsRequired(false);

                // Enforce at most one default account per company (excluding deleted ones)
                b.HasIndex(a => a.CompanyId)
                 .HasFilter("[IsDefault] = 1 AND [IsDeleted] = 0")
                 .IsUnique();
            });

            modelBuilder.Entity<Payments>(b =>
            {
                b.HasOne(p => p.Escrow)
                 .WithMany()
                 .HasForeignKey(p => p.EscrowRecordId)
                 .OnDelete(DeleteBehavior.NoAction)
                 .IsRequired(false);
            });


            // NoAction على كل الـ relationships الباقية
            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                         .SelectMany(e => e.GetForeignKeys()))
            {
                if (relationship.DeleteBehavior == DeleteBehavior.Cascade
                    && relationship.DeclaringEntityType.ClrType != typeof(RefreshToken))
                {
                    relationship.DeleteBehavior = DeleteBehavior.NoAction;
                }
            }



            // ── Seed Roles ────────────────────────────────────────────────────
            SeedRoles(modelBuilder);

            // ── Existing Seed Data ────────────────────────────────────────────
            SeedIndustryTypes(modelBuilder);
            SeedMaterialCategories(modelBuilder);
            SeedCommissionPolicies(modelBuilder);
        }

        private static void SeedRoles(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<ApplicationRole>().HasData(
                new ApplicationRole
                {
                    Id = 1,
                    Name = "SuperAdmin",
                    NormalizedName = "SUPERADMIN",
                    Description = "Platform-level administrator",
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },

                new ApplicationRole
                {
                    Id = 2,
                    Name = "CompanyAdmin",
                    NormalizedName = "COMPANYADMIN",
                    Description = "Manages users within their own company",
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ApplicationRole
                {
                    Id = 3,
                    Name = "CompanyUser",
                    NormalizedName = "COMPANYUSER",
                    Description = "Standard bidder / operator",
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }

            );
        }

        // باقي الـ seed methods من الكود الأصلي
        private static void SeedIndustryTypes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IndustryType>().HasData(
                new IndustryType { Id = 1, IndustryName = "Steel & Metals", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
                new IndustryType { Id = 2, IndustryName = "Plastics & Polymers", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
                new IndustryType { Id = 3, IndustryName = "Construction", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
                new IndustryType { Id = 4, IndustryName = "Chemicals", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
                new IndustryType { Id = 5, IndustryName = "Agriculture", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
                new IndustryType { Id = 6, IndustryName = "Electronics", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
                new IndustryType { Id = 7, IndustryName = "Textiles", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
                new IndustryType { Id = 8, IndustryName = "Food & Beverages", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
                new IndustryType { Id = 9, IndustryName = "Energy & Fuel", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
                new IndustryType { Id = 10, IndustryName = "Logistics", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false }
            );
        }

        private static void SeedMaterialCategories(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Material_Categories>().HasData(
                new Material_Categories { Id = 1, CategoryName = "Carbon Steel", Description = "Structural and engineering carbon steel", UnitOfMeasure = "Ton", image_url = "", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Material_Categories { Id = 2, CategoryName = "Stainless Steel", Description = "Corrosion-resistant stainless steel grades", UnitOfMeasure = "Ton", image_url = "", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Material_Categories { Id = 3, CategoryName = "Copper & Alloys", Description = "Pure copper and copper-based alloys", UnitOfMeasure = "Ton", image_url = "", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Material_Categories { Id = 4, CategoryName = "Aluminum", Description = "Aluminum sheets, coils and extrusions", UnitOfMeasure = "Ton", image_url = "", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Material_Categories { Id = 5, CategoryName = "PVC Resin", Description = "Polyvinyl chloride for pipes and profiles", UnitOfMeasure = "Ton", image_url = "", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Material_Categories { Id = 6, CategoryName = "HDPE / LDPE", Description = "Polyethylene pellets for packaging & pipes", UnitOfMeasure = "Ton", image_url = "", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Material_Categories { Id = 7, CategoryName = "Cement & Clinker", Description = "Ordinary Portland cement and clinker", UnitOfMeasure = "Ton", image_url = "", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Material_Categories { Id = 8, CategoryName = "Chemicals — Solvents", Description = "Industrial organic and inorganic solvents", UnitOfMeasure = "L", image_url = "", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Material_Categories { Id = 9, CategoryName = "Grains & Pulses", Description = "Wheat, corn, lentils and agricultural grains", UnitOfMeasure = "Ton", image_url = "", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Material_Categories { Id = 10, CategoryName = "Crude Oil Derivatives", Description = "Fuel oil, naphtha and petroleum distillates", UnitOfMeasure = "Barrel", image_url = "", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        }

        private static void SeedCommissionPolicies(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Commission_Policies>().HasData(
                new Commission_Policies
                {
                    Id = 1,
                    PolicyName = "Standard 2%",
                    CommissionRate = 0.02m,   // 2% stored as decimal fraction
                    MinAmount = 0m,
                    MaxAmount = 9_999_999m,
                    EffectiveFrom = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    EffectiveTo = new DateTime(2030, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    Active = true
                },
                new Commission_Policies
                {
                    Id = 2,
                    PolicyName = "Premium 1.5% (High-Value)",
                    CommissionRate = 0.015m,  // 1.5% for large deals > 500K
                    MinAmount = 500_000m,
                    MaxAmount = 99_999_999m,
                    EffectiveFrom = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    EffectiveTo = new DateTime(2030, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    Active = true
                }
            );
        }
    }
}
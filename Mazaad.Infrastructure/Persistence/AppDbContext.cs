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

            // ── Bids ──────────────────────────────────────────────────────────
            modelBuilder.Entity<Bids>()
                .HasOne(b => b.BuyerCompany)
                .WithMany(c => c.Bids)
                .HasForeignKey(b => b.BuyerCompanyId)
                .OnDelete(DeleteBehavior.NoAction);

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
        private static void SeedIndustryTypes(ModelBuilder modelBuilder) { /* كما هي */ }
        private static void SeedMaterialCategories(ModelBuilder modelBuilder) { /* كما هي */ }
        private static void SeedCommissionPolicies(ModelBuilder modelBuilder) { /* كما هي */ }
    }
}
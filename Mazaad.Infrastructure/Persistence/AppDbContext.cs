using Mazaad.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Mazaad.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

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

            // Chat_Channels - SellerCompany
            modelBuilder.Entity<Chat_Channels>()
                .HasOne(cc => cc.SellerCompany)
                .WithMany(c => c.SellerChatChannels)
                .HasForeignKey(cc => cc.SellerCompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            // Chat_Channels - BuyerCompany
            modelBuilder.Entity<Chat_Channels>()
                .HasOne(cc => cc.BuyerCompany)
                .WithMany(c => c.BuyerChatChannels)
                .HasForeignKey(cc => cc.BuyerCompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            // Orders - SellerCompany
            modelBuilder.Entity<Orders>()
                .HasOne(o => o.SellerCompany)
                .WithMany(c => c.SalesOrders)
                .HasForeignKey(o => o.SellerCompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            // Orders - BuyerCompany
            modelBuilder.Entity<Orders>()
                .HasOne(o => o.BuyerCompany)
                .WithMany(c => c.PurchaseOrders)
                .HasForeignKey(o => o.BuyerCompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            // Bids - BuyerCompany
            modelBuilder.Entity<Bids>()
                .HasOne(b => b.BuyerCompany)
                .WithMany(c => c.Bids)
                .HasForeignKey(b => b.BuyerCompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            // Apply NoAction globally for any remaining FK relationships
            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                         .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }
        }
    }
}

using Mazaad.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mazaad.Infrastructure.Persistence
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions option):base(option)
        {
            
        }

        public DbSet<Companies> Companies { get; set; }
        public DbSet<IndustryType> IndustryTypes { get; set; }
        public DbSet<App_Users> App_Users { get; set; }
        public DbSet<Listings> Listings { get; set; }
        public DbSet<Bids> Bids { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<Payments> Payments { get; set; }
        public DbSet<Commission_Policies> Commission_Policies { get; set; }
        public DbSet<Chat_Channels> Chat_Channels { get; set; }
        public DbSet<Messages> Messages { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
        public DbSet<Material_Categories> Material_Categories { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Chat_Channels>()
           .HasOne(cc => cc.SellerCompany)
           .WithMany(c => c.SellerChatChannels)
           .HasForeignKey(cc => cc.seller_company_id)
           .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Chat_Channels>()
                .HasOne(cc => cc.BuyerCompany)
                .WithMany(c => c.BuyerChatChannels)
                .HasForeignKey(cc => cc.buyer_company_id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Orders>()
                .HasOne(o => o.SellerCompany)
                .WithMany(c => c.SalesOrders)
                .HasForeignKey(o => o.seller_company_id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Orders>()
                .HasOne(o => o.BuyerCompany)
                .WithMany(c => c.PurchaseOrders)
                .HasForeignKey(o => o.buyer_company_id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Bids>()
                .HasOne(b => b.BuyerCompany)
                .WithMany(c => c.Bids)
                .HasForeignKey(b => b.buyer_company_id)
                .OnDelete(DeleteBehavior.NoAction);
                foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }

        }
    }
}


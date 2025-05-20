using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjecteSOS_Grup03API.Models;

namespace ProjecteSOS_Grup03API.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ProductOrder> ProductsOrders { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Herència TPH (Table-Per-Hierarchy)
            builder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<User>("User")
                .HasValue<Employee>("Employee")
                .HasValue<Client>("Client");

            // Employee - Manager (autoreferència)
            builder.Entity<Employee>()
                .HasMany(m => m.Employees)
                .WithOne(e => e.Manager)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Sales - Order (1:N)
            builder.Entity<Order>()
                .HasOne(o => o.SalesRep)
                .WithMany(e => e.Orders)
                .HasForeignKey(o => o.SalesRepId)
                .OnDelete(DeleteBehavior.SetNull);

            // Client - Order (1:N)
            builder.Entity<Order>()
                .HasOne(o => o.Client)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // ProductOrder (N:M entre Order i Product)
            builder.Entity<ProductOrder>()
                .HasKey(po => new { po.OrderId, po.ProductId });

            builder.Entity<ProductOrder>()
                .HasOne(po => po.Order)
                .WithMany(o => o.OrdersProducts)
                .HasForeignKey(po => po.OrderId);

            builder.Entity<ProductOrder>()
                .HasOne(po => po.Product)
                .WithMany(o => o.ProductsOrders)
                .HasForeignKey(po => po.ProductId);
        }
    }
}

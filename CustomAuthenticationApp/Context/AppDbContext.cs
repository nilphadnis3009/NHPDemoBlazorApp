using Entities.Models;
using Entities.Shopping;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace CustomAuthenticationApp.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Stock> Stocks => Set<Stock>();
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
        public DbSet<BackgroundJob> BackgroundJobs => Set<BackgroundJob>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Composite keys for Join Tables
            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
            modelBuilder.Entity<CartItem>().HasKey(c => c.cid);

            // Map User side
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Automatically clean up if User is deleted

            // Map Role side
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });
            // Map Role side
            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Map Permission side
            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);


            // If a Product has one Stock record
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Stock)
                .WithOne(s => s.product)
                .HasForeignKey<Stock>("product_uid"); // e.g., "ProductId" or "Product_Id"

            // OR if a Product has many Stock records
            // modelBuilder.Entity<Stock>()
            //     .HasOne(s => s.Product)
            //     .WithMany(p => p.Stocks)
            //     .HasForeignKey(s => s.ProductId); 

            modelBuilder.Entity<CartItem>()
           .HasIndex(c => c.user_id);

            // Map decimal precision to avoid warnings
            modelBuilder.Entity<CartItem>()
                .Property(c => c.Price)
                .HasPrecision(18, 2);

            // 1. Map Nested Object (Owned Type) -> Flattens fields into the Projects table
            modelBuilder.Entity<Project>().OwnsOne(p => p.Budget, budget =>
            {
                budget.Property(b => b.AllocatedAmount).HasColumnType("decimal(18,2)");
                budget.Property(b => b.Currency).HasMaxLength(10);
            });

            // 2. Map Nested List (One-to-Many Relationship)
            modelBuilder.Entity<Project>()
                .HasMany(p => p.TeamMembers)
                .WithOne() // Unidirectional or configure back-reference if needed
                .OnDelete(DeleteBehavior.Cascade); // Deleting a project wipes its team automatically

            modelBuilder.Entity<BackgroundJob>(entity =>
            {
                entity.HasIndex(e => new { e.Status, e.CreatedAt });
            });
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore;

namespace DevShop.Models;

public partial class ShopDbContext : DbContext
{
    public ShopDbContext()
    {
    }

    public ShopDbContext(DbContextOptions<ShopDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }
    public virtual DbSet<CartItem> CartItems { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderItem> OrderItems { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=devshop.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        bool isSql = Database.ProviderName?.Contains("SqlServer") == true;

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.IdCarts);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.IdCarts).HasColumnName("ID_carts");

            if (isSql)
            {
                entity.Property(e => e.CartCreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime")
                    .HasColumnName("cart_created_at");
            }
            else
            {
                entity.Property(e => e.CartCreatedAt).HasColumnName("cart_created_at");
            }

            entity.Property(e => e.UserId).HasColumnName("user_ID");

            entity.HasOne(d => d.User).WithOne(p => p.Cart)
                .HasForeignKey<Cart>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.IdCartItems);
            entity.Property(e => e.IdCartItems).HasColumnName("ID_cart_items");
            entity.Property(e => e.CartId).HasColumnName("cart_ID");
            entity.Property(e => e.CartItemQuantity).HasColumnName("cart_item_quantity");
            entity.Property(e => e.ProductId).HasColumnName("product_ID");
            entity.Property(e => e.UserId).HasColumnName("user_id").HasMaxLength(450);

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            entity.HasOne(d => d.Product).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.IdCategories);
            entity.HasIndex(e => e.CategoryName).IsUnique();
            entity.Property(e => e.IdCategories).HasColumnName("ID_categories");
            entity.Property(e => e.CategoryDescription)
                .HasMaxLength(500)
                .HasColumnName("category_description");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(100)
                .HasColumnName("category_name");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.IdOrders);
            entity.Property(e => e.IdOrders).HasColumnName("ID_orders");

            if (isSql)
            {
                entity.Property(e => e.OrderDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime")
                    .HasColumnName("order_date");
            }
            else
            {
                entity.Property(e => e.OrderDate).HasColumnName("order_date");
            }

            entity.Property(e => e.OrderStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Pending")
                .HasColumnName("order_status");
            entity.Property(e => e.OrderTotalAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("order_total_amount");
            entity.Property(e => e.UserId).HasColumnName("user_ID");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.IdOrderItems);
            entity.Property(e => e.IdOrderItems).HasColumnName("ID_order_items");
            entity.Property(e => e.OrderId).HasColumnName("order_ID");
            entity.Property(e => e.OrderItemPriceAtPurchase)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("order_item_price_at_purchase");
            entity.Property(e => e.OrderItemQuantity).HasColumnName("order_item_quantity");
            entity.Property(e => e.ProductId).HasColumnName("product_ID");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.IdProducts);
            entity.Property(e => e.IdProducts).HasColumnName("ID_products");
            entity.Property(e => e.CategoryId).HasColumnName("category_ID");
            entity.Property(e => e.ProductDescription).HasColumnName("product_description");
            entity.Property(e => e.ProductName)
                .HasMaxLength(200)
                .HasColumnName("product_name");
            entity.Property(e => e.ProductPrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("product_price");
            entity.Property(e => e.ProductStockQuantity).HasColumnName("product_stock_quantity");
            entity.Property(e => e.ProductImageUrl)
                .HasMaxLength(500)
                .HasColumnName("product_image_url");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRoles);
            entity.HasIndex(e => e.RoleName).IsUnique();
            entity.Property(e => e.IdRoles).HasColumnName("ID_roles");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUsers);
            entity.HasIndex(e => e.UserLogin).IsUnique();
            entity.Property(e => e.IdUsers).HasColumnName("ID_users");
            entity.Property(e => e.RoleId).HasColumnName("role_ID");

            if (isSql)
            {
                entity.Property(e => e.UserCreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime")
                    .HasColumnName("user_created_at");
            }
            else
            {
                entity.Property(e => e.UserCreatedAt).HasColumnName("user_created_at");
            }

            entity.Property(e => e.UserLogin)
                .HasMaxLength(50)
                .HasColumnName("user_login");
            entity.Property(e => e.UserPasswordHash)
                .HasColumnName("user_password_hash");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

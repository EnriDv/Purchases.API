using Microsoft.EntityFrameworkCore;
using Purchases.API.Domain.Entities;
using Purchases.API.Domain.Enums;

namespace Purchases.API.Infrastructure.Persistence;

public class PurchasesDbContext : DbContext
{
    public PurchasesDbContext(DbContextOptions<PurchasesDbContext> options) : base(options) { }

    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }

    public DbSet<Company> Companies { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("purchases");

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("suppliers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cen).HasColumnName("suppliers_cen");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(50);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255);
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.ToTable("purchase_orders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(50);
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.Cen).HasColumnName("purchase_orders_cen");
            entity.Property(e => e.WarehouseCen).HasColumnName("warehouse_cen");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<int>();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.ConfirmedAt).HasColumnName("confirmed_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();

            entity.HasOne(d => d.Supplier).WithMany(s => s.PurchaseOrders).HasForeignKey(d => d.SupplierId);
        });

        modelBuilder.Entity<PurchaseOrderItem>(entity =>
        {
            entity.ToTable("purchase_order_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PurchaseOrderId).HasColumnName("purchase_order_id");
            entity.Property(e => e.ProductCen).HasColumnName("product_cen");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.PurchaseOrder).WithMany(p => p.Items).HasForeignKey(d => d.PurchaseOrderId);
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("companies", "inventory");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cen).HasColumnName("companies_cen");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Active).HasColumnName("active");
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.ToTable("warehouses", "inventory");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.Cen).HasColumnName("warehouses_cen");
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Active).HasColumnName("active");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products", "inventory");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.Cen).HasColumnName("products_cen");
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Active).HasColumnName("active");
        });
    }
}

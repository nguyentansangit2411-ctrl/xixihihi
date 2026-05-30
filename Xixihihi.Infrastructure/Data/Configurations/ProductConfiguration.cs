using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Xixihihi.Domain.Entities;

namespace Xixihihi.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Description).IsRequired();
        builder.Property(x => x.Price).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Brand).HasMaxLength(100);

        builder.Property(x => x.Condition).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.TransactionType).HasConversion<string>().HasMaxLength(50);

        // Relationships
        builder.HasOne(x => x.Seller)
            .WithMany(u => u.Products)
            .HasForeignKey(x => x.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Province)
            .WithMany(p => p.Products)
            .HasForeignKey(x => x.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Ward)
            .WithMany(d => d.Products)
            .HasForeignKey(x => x.WardId)
            .OnDelete(DeleteBehavior.Restrict);

        // Global Query Filter for Soft Delete
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Indexes for performance optimization
        builder.HasIndex(p => new { p.ProvinceId, p.Status, p.IsDeleted })
            .HasDatabaseName("IX_Products_Province_Status_IsDeleted");
            
        builder.HasIndex(p => new { p.CategoryId, p.Status, p.IsDeleted })
            .HasDatabaseName("IX_Products_Category_Status_IsDeleted");
            
        builder.HasIndex(p => new { p.Status, p.IsDeleted, p.CreatedAt })
            .HasDatabaseName("IX_Products_Status_CreatedAt");
            
        builder.HasIndex(p => new { p.Status, p.IsDeleted, p.Price })
            .HasDatabaseName("IX_Products_Status_Price");
    }
}

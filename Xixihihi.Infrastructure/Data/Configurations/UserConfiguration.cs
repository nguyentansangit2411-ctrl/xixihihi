using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Xixihihi.Domain.Entities;

namespace Xixihihi.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
        builder.Property(x => x.DisplayName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.AvatarUrl).HasMaxLength(2000);
        
        builder.Property(x => x.PhoneNumber).HasMaxLength(20);
        builder.Property(x => x.ZaloLink).HasMaxLength(2000);
        builder.Property(x => x.FacebookLink).HasMaxLength(2000);

        builder.Property(x => x.Role).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);

        // Relationships
        builder.HasOne(x => x.Province)
            .WithMany(p => p.Users)
            .HasForeignKey(x => x.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Ward)
            .WithMany(d => d.Users)
            .HasForeignKey(x => x.WardId)
            .OnDelete(DeleteBehavior.Restrict);

        // Global Query Filter for Soft Delete
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Indexes
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(u => u.RefreshToken)
            .HasDatabaseName("IX_Users_RefreshToken")
            .IsUnique()
            .HasFilter("[RefreshToken] IS NOT NULL");
    }
}

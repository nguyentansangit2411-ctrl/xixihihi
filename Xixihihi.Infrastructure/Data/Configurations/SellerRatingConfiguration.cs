using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Xixihihi.Domain.Entities;

namespace Xixihihi.Infrastructure.Data.Configurations;

public class SellerRatingConfiguration : IEntityTypeConfiguration<SellerRating>
{
    public void Configure(EntityTypeBuilder<SellerRating> builder)
    {
        builder.ToTable("SellerRatings");

        builder.HasKey(sr => sr.Id);

        builder.Property(sr => sr.Rating)
            .IsRequired();

        builder.Property(sr => sr.Comment)
            .HasMaxLength(500);

        builder.HasOne(sr => sr.Seller)
            .WithMany(u => u.ReceivedRatings)
            .HasForeignKey(sr => sr.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sr => sr.Reviewer)
            .WithMany(u => u.GivenRatings)
            .HasForeignKey(sr => sr.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Đảm bảo 1 buyer chỉ đánh giá 1 seller 1 lần
        builder.HasIndex(sr => new { sr.SellerId, sr.ReviewerId })
            .IsUnique();

        builder.HasQueryFilter(sr => !sr.IsDeleted);
    }
}

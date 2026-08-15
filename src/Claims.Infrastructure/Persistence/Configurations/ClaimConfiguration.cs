using Claims.Domain.Entities;
using Claims.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Claims.Infrastructure.Persistence.Configurations;

public class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        builder.ToTable("Claims");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.PolicyNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.InsuredName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.ClaimType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.EstimatedAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired();

        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.PolicyNumber);

        builder.HasMany(c => c.StatusHistory)
            .WithOne(h => h.Claim)
            .HasForeignKey(h => h.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
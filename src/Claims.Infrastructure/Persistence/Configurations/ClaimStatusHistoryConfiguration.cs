using Claims.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Claims.Infrastructure.Persistence.Configurations;

public class ClaimStatusHistoryConfiguration : IEntityTypeConfiguration<ClaimStatusHistory>
{
    public void Configure(EntityTypeBuilder<ClaimStatusHistory> builder)
    {
        builder.ToTable("ClaimStatusHistory");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.ClaimId).IsRequired();

        builder.Property(h => h.PreviousStatus)
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(h => h.NewStatus)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(h => h.ChangedAt).IsRequired();

        builder.HasIndex(h => h.ClaimId);
    }
}
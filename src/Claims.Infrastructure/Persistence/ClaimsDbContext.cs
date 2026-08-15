using Claims.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Persistence;

public class ClaimsDbContext : DbContext
{
    public ClaimsDbContext(DbContextOptions<ClaimsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Claim> Claims => Set<Claim>();

    public DbSet<ClaimStatusHistory> ClaimStatusHistory => Set<ClaimStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClaimsDbContext).Assembly);
    }
}
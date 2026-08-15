using Claims.Domain.Entities;
using Claims.Domain.Enums;
using Claims.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Claims.Tests;

/// <summary>
/// Pruebas del repositorio sobre SQLite en memoria. Cubren la consulta real
/// generada por EF Core (filtros, historial) y la persistencia.
/// La estrategia de bloqueo UPDLOCK de ChangeStatusAsync es específica de
/// SQL Server y se valida a nivel de servicio con el repositorio en memoria.
/// </summary>
public class ClaimsRepositoryTests
{
    private static ClaimsDbContext CreateDbContext()
    {
        var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        connection.Open();

        var options = new DbContextOptionsBuilder<ClaimsDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new ClaimsDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private static Claim BuildClaim(string policyNumber, ClaimStatus status)
    {
        var now = DateTime.UtcNow;
        return new Claim
        {
            PolicyNumber = policyNumber,
            InsuredName = "Asegurado Test",
            ClaimType = "Auto",
            EstimatedAmount = 1000m,
            Status = status,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    [Fact]
    public async Task QueryAsync_WithStatusFilter_ReturnsOnlyMatchingStatus()
    {
        using var context = CreateDbContext();
        var repo = new ClaimsRepository(context);

        await repo.AddAsync(BuildClaim("POL-A", ClaimStatus.OPEN), CancellationToken.None);
        await repo.AddAsync(BuildClaim("POL-B", ClaimStatus.IN_REVIEW), CancellationToken.None);
        await repo.AddAsync(BuildClaim("POL-C", ClaimStatus.CLOSED), CancellationToken.None);

        var open = await repo.QueryAsync(ClaimStatus.OPEN, null, CancellationToken.None);

        Assert.Single(open);
        Assert.Equal("POL-A", open[0].PolicyNumber);
    }

    [Fact]
    public async Task QueryAsync_WithPolicyNumberFilter_ReturnsOnlyMatchingPolicy()
    {
        using var context = CreateDbContext();
        var repo = new ClaimsRepository(context);

        await repo.AddAsync(BuildClaim("POL-1001", ClaimStatus.OPEN), CancellationToken.None);
        await repo.AddAsync(BuildClaim("POL-2002", ClaimStatus.OPEN), CancellationToken.None);

        var result = await repo.QueryAsync(null, "1001", CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("POL-1001", result[0].PolicyNumber);
    }

    [Fact]
    public async Task QueryAsync_WithCombinedFilters_ReturnsIntersection()
    {
        using var context = CreateDbContext();
        var repo = new ClaimsRepository(context);

        await repo.AddAsync(BuildClaim("POL-1001", ClaimStatus.OPEN), CancellationToken.None);
        await repo.AddAsync(BuildClaim("POL-1001", ClaimStatus.CLOSED), CancellationToken.None);

        var result = await repo.QueryAsync(ClaimStatus.CLOSED, "1001", CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(ClaimStatus.CLOSED, result[0].Status);
    }

    [Fact]
    public async Task GetHistoryAsync_ReturnsHistoryOrderedByChangedAt()
    {
        using var context = CreateDbContext();
        var repo = new ClaimsRepository(context);

        var claim = BuildClaim("POL-H1", ClaimStatus.CLOSED);
        claim.StatusHistory.Add(new ClaimStatusHistory { PreviousStatus = null, NewStatus = ClaimStatus.OPEN, ChangedAt = DateTime.UtcNow.AddDays(-3) });
        claim.StatusHistory.Add(new ClaimStatusHistory { PreviousStatus = ClaimStatus.OPEN, NewStatus = ClaimStatus.IN_REVIEW, ChangedAt = DateTime.UtcNow.AddDays(-2) });
        claim.StatusHistory.Add(new ClaimStatusHistory { PreviousStatus = ClaimStatus.IN_REVIEW, NewStatus = ClaimStatus.CLOSED, ChangedAt = DateTime.UtcNow.AddDays(-1) });
        await repo.AddAsync(claim, CancellationToken.None);

        var history = await repo.GetHistoryAsync(claim.Id, CancellationToken.None);

        Assert.Equal(3, history.Count);
        Assert.Equal(ClaimStatus.OPEN, history[0].NewStatus);
        Assert.Null(history[0].PreviousStatus);
        Assert.Equal(ClaimStatus.IN_REVIEW, history[1].NewStatus);
        Assert.Equal(ClaimStatus.CLOSED, history[2].NewStatus);
    }

    [Fact]
    public async Task DeleteAsync_RemovesClaimAndCascadesHistory()
    {
        using var context = CreateDbContext();
        var repo = new ClaimsRepository(context);

        var claim = BuildClaim("POL-D1", ClaimStatus.OPEN);
        claim.StatusHistory.Add(new ClaimStatusHistory { PreviousStatus = null, NewStatus = ClaimStatus.OPEN, ChangedAt = DateTime.UtcNow });
        await repo.AddAsync(claim, CancellationToken.None);

        var loaded = await repo.GetByIdAsync(claim.Id, CancellationToken.None);
        Assert.NotNull(loaded);

        await repo.DeleteAsync(loaded!, CancellationToken.None);

        Assert.Null(await repo.GetByIdAsync(claim.Id, CancellationToken.None));
        Assert.Empty(await repo.GetHistoryAsync(claim.Id, CancellationToken.None));
    }
}
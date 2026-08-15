using Claims.Application.Abstractions;
using Claims.Application.Exceptions;
using Claims.Domain.Entities;
using Claims.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Persistence;

public class ClaimsRepository : IClaimRepository
{
    private readonly ClaimsDbContext _context;

    public ClaimsRepository(ClaimsDbContext context)
    {
        _context = context;
    }

    public async Task<List<Claim>> QueryAsync(ClaimStatus? status, string? policyNumber, CancellationToken ct)
    {
        var query = _context.Claims.AsNoTracking();

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(policyNumber))
            query = query.Where(c => c.PolicyNumber.Contains(policyNumber));

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Claim?> GetByIdAsync(int id, CancellationToken ct)
        => await _context.Claims
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Claim> AddAsync(Claim claim, CancellationToken ct)
    {
        _context.Claims.Add(claim);
        await _context.SaveChangesAsync(ct);
        return claim;
    }

    public async Task<Claim> UpdateAsync(Claim claim, CancellationToken ct)
    {
        DetachExisting(claim.Id);
        _context.Entry(claim).State = EntityState.Modified;
        await _context.SaveChangesAsync(ct);
        return claim;
    }

    public async Task DeleteAsync(Claim claim, CancellationToken ct)
    {
        DetachExisting(claim.Id);
        _context.Entry(claim).State = EntityState.Deleted;
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<ClaimStatusHistory>> GetHistoryAsync(int claimId, CancellationToken ct)
        => await _context.ClaimStatusHistory
            .AsNoTracking()
            .Where(h => h.ClaimId == claimId)
            .OrderBy(h => h.ChangedAt)
            .ToListAsync(ct);

    /// <summary>
    /// Actualiza el estado y registra el historial de forma atomica.
    ///
    /// Estrategia de concurrencia: se inicia una transaccion y se relee la fila
    /// con la sugerencia de bloqueo UPDLOCK (SQL Server). Dos peticiones que
    /// intenten cambiar el mismo siniestro de forma simultanea se serializan:
    /// la segunda relee el estado ya modificado por la primera y, como no
    /// coincide con <paramref name="expectedCurrent"/>, se rechaza con 409 en
    /// lugar de generar un historial duplicado e inconsistente.
    /// </summary>
    public async Task<Claim> ChangeStatusAsync(int claimId, ClaimStatus expectedCurrent, ClaimStatus newStatus, CancellationToken ct)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        var claim = await _context.Claims
            .FromSqlInterpolated($"SELECT * FROM dbo.Claims WITH (UPDLOCK) WHERE Id = {claimId}")
            .SingleOrDefaultAsync(ct)
            ?? throw new ClaimNotFoundException(claimId);

        if (claim.Status != expectedCurrent)
        {
            await transaction.RollbackAsync(ct);
            throw new InvalidStatusTransitionException(claim.Status, newStatus);
        }

        var previousStatus = claim.Status;
        var now = DateTime.UtcNow;

        claim.Status = newStatus;
        claim.UpdatedAt = now;

        _context.ClaimStatusHistory.Add(new ClaimStatusHistory
        {
            ClaimId = claim.Id,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            ChangedAt = now
        });

        await _context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return claim;
    }

    private void DetachExisting(int id)
    {
        var existing = _context.ChangeTracker.Entries<Claim>().FirstOrDefault(e => e.Entity.Id == id);
        if (existing is not null)
            existing.State = EntityState.Detached;
    }
}
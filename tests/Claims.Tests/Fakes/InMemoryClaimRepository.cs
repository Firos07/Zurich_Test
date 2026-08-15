using Claims.Application.Abstractions;
using Claims.Application.Exceptions;
using Claims.Domain.Entities;
using Claims.Domain.Enums;

namespace Claims.Tests.Fakes;

/// <summary>
/// Implementación en memoria de IClaimRepository para pruebas unitarias
/// del servicio. Simula la persistencia con una lista en memoria.
/// </summary>
public class InMemoryClaimRepository : IClaimRepository
{
    private readonly List<Claim> _claims = new();
    private readonly List<ClaimStatusHistory> _history = new();
    private int _nextClaimId = 1;
    private int _nextHistoryId = 1;

    public Task<List<Claim>> QueryAsync(ClaimStatus? status, string? policyNumber, CancellationToken ct)
    {
        IEnumerable<Claim> query = _claims;

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(policyNumber))
            query = query.Where(c => c.PolicyNumber.Contains(policyNumber, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(query.OrderByDescending(c => c.CreatedAt).ToList());
    }

    public Task<Claim?> GetByIdAsync(int id, CancellationToken ct)
        => Task.FromResult(_claims.FirstOrDefault(c => c.Id == id));

    public Task<Claim> AddAsync(Claim claim, CancellationToken ct)
    {
        claim.Id = _nextClaimId++;
        foreach (var history in claim.StatusHistory)
        {
            history.Id = _nextHistoryId++;
            history.ClaimId = claim.Id;
            _history.Add(history);
        }

        _claims.Add(claim);
        return Task.FromResult(claim);
    }

    public Task<Claim> UpdateAsync(Claim claim, CancellationToken ct)
        => Task.FromResult(claim);

    public Task DeleteAsync(Claim claim, CancellationToken ct)
    {
        _claims.Remove(claim);
        _history.RemoveAll(h => h.ClaimId == claim.Id);
        return Task.CompletedTask;
    }

    public Task<List<ClaimStatusHistory>> GetHistoryAsync(int claimId, CancellationToken ct)
        => Task.FromResult(_history.Where(h => h.ClaimId == claimId).OrderBy(h => h.ChangedAt).ToList());

    public Task<Claim> ChangeStatusAsync(int claimId, ClaimStatus expectedCurrent, ClaimStatus newStatus, CancellationToken ct)
    {
        var claim = _claims.FirstOrDefault(c => c.Id == claimId)
            ?? throw new ClaimNotFoundException(claimId);

        if (claim.Status != expectedCurrent)
            throw new InvalidStatusTransitionException(claim.Status, newStatus);

        var now = DateTime.UtcNow;
        _history.Add(new ClaimStatusHistory
        {
            Id = _nextHistoryId++,
            ClaimId = claim.Id,
            PreviousStatus = claim.Status,
            NewStatus = newStatus,
            ChangedAt = now
        });

        claim.Status = newStatus;
        claim.UpdatedAt = now;

        return Task.FromResult(claim);
    }
}
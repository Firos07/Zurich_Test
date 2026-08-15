using Claims.Domain.Entities;
using Claims.Domain.Enums;

namespace Claims.Application.Abstractions;

public interface IClaimRepository
{
    Task<List<Claim>> QueryAsync(ClaimStatus? status, string? policyNumber, CancellationToken ct);

    Task<Claim?> GetByIdAsync(int id, CancellationToken ct);

    Task<Claim> AddAsync(Claim claim, CancellationToken ct);

    Task<Claim> UpdateAsync(Claim claim, CancellationToken ct);

    Task DeleteAsync(Claim claim, CancellationToken ct);

    Task<List<ClaimStatusHistory>> GetHistoryAsync(int claimId, CancellationToken ct);

    /// <summary>
    /// Actualiza el estado y registra el historial de forma atomica.
    /// <paramref name="expectedCurrent"/> permite detectar cambios concurrentes.
    /// </summary>
    Task<Claim> ChangeStatusAsync(int claimId, ClaimStatus expectedCurrent, ClaimStatus newStatus, CancellationToken ct);
}
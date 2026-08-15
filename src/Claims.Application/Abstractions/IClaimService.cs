using Claims.Application.DTOs;
using Claims.Domain.Enums;

namespace Claims.Application.Abstractions;

public interface IClaimService
{
    Task<IReadOnlyList<ClaimResponse>> GetClaimsAsync(ClaimStatus? status, string? policyNumber, CancellationToken ct);

    Task<ClaimResponse> GetClaimAsync(int id, CancellationToken ct);

    Task<ClaimResponse> CreateClaimAsync(CreateClaimRequest request, CancellationToken ct);

    Task<ClaimResponse> UpdateClaimAsync(int id, UpdateClaimRequest request, CancellationToken ct);

    Task DeleteClaimAsync(int id, CancellationToken ct);

    Task<ClaimResponse> ChangeStatusAsync(int id, ChangeClaimStatusRequest request, CancellationToken ct);

    Task<IReadOnlyList<ClaimStatusHistoryResponse>> GetStatusHistoryAsync(int id, CancellationToken ct);
}
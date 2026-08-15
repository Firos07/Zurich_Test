using Claims.Application.Abstractions;
using Claims.Application.DTOs;
using Claims.Application.Exceptions;
using Claims.Domain.Entities;
using Claims.Domain.Enums;
using Claims.Domain.Rules;

namespace Claims.Application.Services;

public class ClaimService : IClaimService
{
    private readonly IClaimRepository _repository;

    public ClaimService(IClaimRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ClaimResponse>> GetClaimsAsync(ClaimStatus? status, string? policyNumber, CancellationToken ct)
    {
        var claims = await _repository.QueryAsync(status, policyNumber, ct);
        return claims.Select(ToResponse).ToList();
    }

    public async Task<ClaimResponse> GetClaimAsync(int id, CancellationToken ct)
    {
        var claim = await _repository.GetByIdAsync(id, ct) ?? throw new ClaimNotFoundException(id);
        return ToResponse(claim);
    }

    public async Task<ClaimResponse> CreateClaimAsync(CreateClaimRequest request, CancellationToken ct)
    {
        ValidateCreate(request);

        var now = DateTime.UtcNow;
        var claim = new Claim
        {
            PolicyNumber = request.PolicyNumber.Trim(),
            InsuredName = request.InsuredName.Trim(),
            ClaimType = request.ClaimType.Trim(),
            EstimatedAmount = request.EstimatedAmount,
            Status = ClaimStatus.OPEN,
            CreatedAt = now,
            UpdatedAt = now
        };
        claim.StatusHistory.Add(new ClaimStatusHistory
        {
            PreviousStatus = null,
            NewStatus = ClaimStatus.OPEN,
            ChangedAt = now
        });

        await _repository.AddAsync(claim, ct);
        return ToResponse(claim);
    }

    public async Task<ClaimResponse> UpdateClaimAsync(int id, UpdateClaimRequest request, CancellationToken ct)
    {
        ValidateUpdate(request);

        var claim = await _repository.GetByIdAsync(id, ct) ?? throw new ClaimNotFoundException(id);

        claim.PolicyNumber = request.PolicyNumber.Trim();
        claim.InsuredName = request.InsuredName.Trim();
        claim.ClaimType = request.ClaimType.Trim();
        claim.EstimatedAmount = request.EstimatedAmount;
        claim.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(claim, ct);
        return ToResponse(claim);
    }

    public async Task DeleteClaimAsync(int id, CancellationToken ct)
    {
        var claim = await _repository.GetByIdAsync(id, ct) ?? throw new ClaimNotFoundException(id);
        await _repository.DeleteAsync(claim, ct);
    }

    public async Task<ClaimResponse> ChangeStatusAsync(int id, ChangeClaimStatusRequest request, CancellationToken ct)
    {
        if (!Enum.IsDefined(typeof(ClaimStatus), request.NewStatus))
            throw new DomainValidationException(nameof(ChangeClaimStatusRequest.NewStatus), "El estado indicado no es válido.");

        var claim = await _repository.GetByIdAsync(id, ct) ?? throw new ClaimNotFoundException(id);

        if (!ClaimStatusTransitions.CanTransition(claim.Status, request.NewStatus))
            throw new InvalidStatusTransitionException(claim.Status, request.NewStatus);

        var updated = await _repository.ChangeStatusAsync(id, claim.Status, request.NewStatus, ct);
        return ToResponse(updated);
    }

    public async Task<IReadOnlyList<ClaimStatusHistoryResponse>> GetStatusHistoryAsync(int id, CancellationToken ct)
    {
        var claim = await _repository.GetByIdAsync(id, ct);
        if (claim is null)
            throw new ClaimNotFoundException(id);

        var history = await _repository.GetHistoryAsync(id, ct);
        return history.Select(h => new ClaimStatusHistoryResponse
        {
            Id = h.Id,
            ClaimId = h.ClaimId,
            PreviousStatus = h.PreviousStatus?.ToString(),
            NewStatus = h.NewStatus.ToString(),
            ChangedAt = DateTime.SpecifyKind(h.ChangedAt, DateTimeKind.Utc)
        }).ToList();
    }

    private static void ValidateCreate(CreateClaimRequest request)
    {
        var errors = new List<DomainValidationException.ValidationError>();

        if (string.IsNullOrWhiteSpace(request.PolicyNumber))
            errors.Add(new DomainValidationException.ValidationError("PolicyNumber", "El número de póliza es obligatorio."));
        if (string.IsNullOrWhiteSpace(request.InsuredName))
            errors.Add(new DomainValidationException.ValidationError("InsuredName", "El nombre del asegurado es obligatorio."));
        if (string.IsNullOrWhiteSpace(request.ClaimType))
            errors.Add(new DomainValidationException.ValidationError("ClaimType", "El tipo de siniestro es obligatorio."));
        if (request.EstimatedAmount <= 0)
            errors.Add(new DomainValidationException.ValidationError("EstimatedAmount", "El monto estimado debe ser mayor que cero."));

        if (errors.Count > 0)
            throw new DomainValidationException(errors);
    }

    private static void ValidateUpdate(UpdateClaimRequest request)
    {
        ValidateCreate(new CreateClaimRequest
        {
            PolicyNumber = request.PolicyNumber,
            InsuredName = request.InsuredName,
            ClaimType = request.ClaimType,
            EstimatedAmount = request.EstimatedAmount
        });
    }

    private static ClaimResponse ToResponse(Claim claim) => new()
    {
        Id = claim.Id,
        PolicyNumber = claim.PolicyNumber,
        InsuredName = claim.InsuredName,
        ClaimType = claim.ClaimType,
        EstimatedAmount = claim.EstimatedAmount,
        Status = claim.Status.ToString(),
        CreatedAt = DateTime.SpecifyKind(claim.CreatedAt, DateTimeKind.Utc),
        UpdatedAt = DateTime.SpecifyKind(claim.UpdatedAt, DateTimeKind.Utc)
    };
}
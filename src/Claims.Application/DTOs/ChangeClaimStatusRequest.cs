using Claims.Domain.Enums;

namespace Claims.Application.DTOs;

public record ChangeClaimStatusRequest
{
    public ClaimStatus NewStatus { get; init; }
}
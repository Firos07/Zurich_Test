namespace Claims.Application.DTOs;

public record ClaimStatusHistoryResponse
{
    public int Id { get; init; }

    public int ClaimId { get; init; }

    public string? PreviousStatus { get; init; }

    public string NewStatus { get; init; } = string.Empty;

    public DateTime ChangedAt { get; init; }
}
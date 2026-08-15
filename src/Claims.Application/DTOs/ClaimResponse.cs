namespace Claims.Application.DTOs;

public record ClaimResponse
{
    public int Id { get; init; }

    public string PolicyNumber { get; init; } = string.Empty;

    public string InsuredName { get; init; } = string.Empty;

    public string ClaimType { get; init; } = string.Empty;

    public decimal EstimatedAmount { get; init; }

    public string Status { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}
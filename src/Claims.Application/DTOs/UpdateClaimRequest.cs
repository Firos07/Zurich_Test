using System.ComponentModel.DataAnnotations;

namespace Claims.Application.DTOs;

public record UpdateClaimRequest
{
    [Required(AllowEmptyStrings = false)]
    [MaxLength(50)]
    public string PolicyNumber { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [MaxLength(200)]
    public string InsuredName { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [MaxLength(100)]
    public string ClaimType { get; init; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal EstimatedAmount { get; init; }
}
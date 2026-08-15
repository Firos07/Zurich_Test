using Claims.Domain.Enums;

namespace Claims.Domain.Entities;

public class Claim
{
    public int Id { get; set; }

    public string PolicyNumber { get; set; } = string.Empty;

    public string InsuredName { get; set; } = string.Empty;

    public string ClaimType { get; set; } = string.Empty;

    public decimal EstimatedAmount { get; set; }

    public ClaimStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<ClaimStatusHistory> StatusHistory { get; set; } = new List<ClaimStatusHistory>();
}
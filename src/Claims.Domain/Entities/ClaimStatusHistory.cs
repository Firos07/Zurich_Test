using Claims.Domain.Enums;

namespace Claims.Domain.Entities;

public class ClaimStatusHistory
{
    public int Id { get; set; }

    public int ClaimId { get; set; }

    public ClaimStatus? PreviousStatus { get; set; }

    public ClaimStatus NewStatus { get; set; }

    public DateTime ChangedAt { get; set; }

    public Claim? Claim { get; set; }
}
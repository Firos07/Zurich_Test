using Claims.Domain.Enums;

namespace Claims.Domain.Rules;

/// <summary>
/// Regla de negocio centralizada para las transiciones de estado permitidas.
/// OPEN → IN_REVIEW → CLOSED.
/// </summary>
public static class ClaimStatusTransitions
{
    public static bool CanTransition(ClaimStatus current, ClaimStatus next)
    {
        return (current, next) switch
        {
            (ClaimStatus.OPEN, ClaimStatus.IN_REVIEW) => true,
            (ClaimStatus.IN_REVIEW, ClaimStatus.CLOSED) => true,
            _ => false
        };
    }
}
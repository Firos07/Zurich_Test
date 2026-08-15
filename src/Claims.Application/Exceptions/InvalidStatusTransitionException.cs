using Claims.Domain.Enums;

namespace Claims.Application.Exceptions;

/// <summary>
/// Transicion de estado no permitida, se traduce a HTTP 409 Conflict.
/// </summary>
public class InvalidStatusTransitionException : Exception
{
    public ClaimStatus CurrentStatus { get; }

    public ClaimStatus NewStatus { get; }

    public InvalidStatusTransitionException(ClaimStatus currentStatus, ClaimStatus newStatus)
        : base($"No es posible pasar el estado de {currentStatus} a {newStatus}.")
    {
        CurrentStatus = currentStatus;
        NewStatus = newStatus;
    }
}
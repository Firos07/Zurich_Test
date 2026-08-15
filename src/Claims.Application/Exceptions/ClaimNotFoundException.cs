namespace Claims.Application.Exceptions;

/// <summary>
/// Recurso no encontrado, se traduce a HTTP 404 Not Found.
/// </summary>
public class ClaimNotFoundException : Exception
{
    public ClaimNotFoundException(int claimId)
        : base($"No existe un siniestro con Id {claimId}.")
    {
    }
}
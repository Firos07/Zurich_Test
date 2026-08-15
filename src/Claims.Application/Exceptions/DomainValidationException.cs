namespace Claims.Application.Exceptions;

/// <summary>
/// Error de validacion de datos o de regla de negocio que debe traducirse
/// a un HTTP 400 Bad Request con la lista de errores por campo.
/// </summary>
public class DomainValidationException : Exception
{
    public IReadOnlyList<ValidationError> Errors { get; }

    public DomainValidationException(string field, string message)
        : base(message)
    {
        Errors = new List<ValidationError> { new(field, message) };
    }

    public DomainValidationException(IEnumerable<ValidationError> errors)
        : base("La solicitud no supera las validaciones.")
    {
        Errors = errors.ToList();
    }

    public record ValidationError(string Field, string Message);
}
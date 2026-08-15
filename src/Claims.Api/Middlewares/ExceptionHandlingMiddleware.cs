using System.Text.Json;
using Claims.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ClaimNotFoundException ex)
        {
            _logger.LogInformation(ex, "Siniestro no encontrado.");
            await WriteProblemDetailsAsync(context, StatusCodes.Status404NotFound, "No encontrado", ex.Message);
        }
        catch (InvalidStatusTransitionException ex)
        {
            _logger.LogWarning(ex, "Transición de estado no válida.");
            await WriteProblemDetailsAsync(context, StatusCodes.Status409Conflict, "Conflicto", ex.Message);
        }
        catch (DomainValidationException ex)
        {
            _logger.LogWarning(ex, "Error de validación.");
            await WriteValidationProblemAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado.");
            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Error interno",
                "No fue posible completar la operación.");
        }
    }

    private static async Task WriteValidationProblemAsync(HttpContext context, DomainValidationException ex)
    {
        var errors = ex.Errors
            .GroupBy(e => e.Field)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray());

        var problem = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Error de validación",
            Detail = ex.Message
        };

        await WriteJsonAsync(context, StatusCodes.Status400BadRequest, problem);
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, int statusCode, string title, string detail)
    {
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };

        await WriteJsonAsync(context, statusCode, problem);
    }

    private static async Task WriteJsonAsync(HttpContext context, int statusCode, object body)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await JsonSerializer.SerializeAsync(context.Response.Body, body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}
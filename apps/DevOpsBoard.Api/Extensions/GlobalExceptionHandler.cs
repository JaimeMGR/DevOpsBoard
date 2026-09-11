using DevOpsBoard.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsBoard.Api.Extensions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ValidationException
            or ConflictException
            or NotFoundException
            or UnauthorizedAccessException
            or ForbiddenException)
        {
            _logger.LogWarning(
                "Request rejected: {Message}",
                exception.Message
            );
        }
        else
        {
            _logger.LogError(
                exception,
                "Unhandled exception: {Message}",
                exception.Message
            );
        }

        var (statusCode, title, detail) = exception switch
        {
            ValidationException => (
                StatusCodes.Status400BadRequest,
                "Solicitud no válida.",
                exception.Message
            ),

            ConflictException => (
                StatusCodes.Status409Conflict,
                "Conflicto.",
                exception.Message
            ),

            NotFoundException => (
                StatusCodes.Status404NotFound,
                "Recurso no encontrado.",
                exception.Message
            ),

            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "No autorizado.",
                "Las credenciales proporcionadas no son válidas."
            ),

            ForbiddenException => (
                StatusCodes.Status403Forbidden,
                "Acceso denegado.",
                exception.Message
            ),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Error interno del servidor.",
                "Ha ocurrido un error inesperado."
            )
        };

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problem,
            cancellationToken
        );

        return true;
    }
}
using AdminPanel.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace AdminPanel.Infrastructure.Middleware;

public class DomainExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DomainExceptionMiddleware> _logger;

    public DomainExceptionMiddleware(RequestDelegate next, ILogger<DomainExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var statusCode = exception switch
        {
            EntityNotFoundException => HttpStatusCode.NotFound,
            ValidationException => HttpStatusCode.BadRequest,
            DomainException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        response.StatusCode = (int)statusCode;

        var result = exception switch
        {
            ValidationException validationEx => JsonSerializer.Serialize(new
            {
                error = "Validation failed",
                message = validationEx.Message,
                errors = validationEx.Errors
            }),
            EntityNotFoundException entityEx => JsonSerializer.Serialize(new
            {
                error = "Entity not found",
                message = entityEx.Message,
                entityType = entityEx.EntityType,
                entityId = entityEx.EntityId
            }),
            DomainException domainEx => JsonSerializer.Serialize(new
            {
                error = "Domain error",
                message = domainEx.Message
            }),
            _ => JsonSerializer.Serialize(new
            {
                error = "An error occurred",
                message = "An unexpected error occurred"
            })
        };

        return response.WriteAsync(result);
    }
}


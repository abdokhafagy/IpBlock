using System.Net;
using System.Text.Json;
using IPBlocker.Application.DTOs;
using IPBlocker.Application.Exceptions;

namespace IPBlocke.Api.Middleware;

/// <summary>
/// Global exception handling middleware. Maps domain exceptions to HTTP status codes.
/// </summary>
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            DuplicateException e => (HttpStatusCode.Conflict, e.Message),
            NotFoundException e => (HttpStatusCode.NotFound, e.Message),
            IPBlocker.Application.Exceptions.ValidationException e => (HttpStatusCode.BadRequest, e.Message),
            ExternalApiException e => (HttpStatusCode.BadGateway, e.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        _logger.LogError(exception, "Exception caught by middleware: {StatusCode} — {Message}", (int)statusCode, message);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new ApiErrorResponse
        {
            StatusCode = (int)statusCode,
            Message = message,
            Timestamp = DateTime.UtcNow,
            Details = context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment()
                ? exception.ToString()
                : null
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

using System.Net;
using System.Text.Json;
using BatoClinic.Api.Helpers;

namespace BatoClinic.Api.Middleware;

// Middleware runs during every HTTP request.
// This one catches unexpected server errors and returns a clean JSON response.
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception occurred");

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var errors = _environment.IsDevelopment()
                ? new List<string> { exception.Message }
                : new List<string>();

            var response = new ApiErrorResponse(
                "An unexpected server error occurred",
                errors
            );

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
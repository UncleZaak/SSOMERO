using System.Net;
using System.Text.Json;
using Ssomero.Api.Dtos.Common;

namespace Ssomero.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ApiResponse<object>
        {
            Success = false,
            Message = GetUserFriendlyMessage(exception),
            ErrorCode = GetErrorCode(exception)
        };

        var statusCode = exception switch
        {
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            ArgumentException => HttpStatusCode.BadRequest,
            KeyNotFoundException => HttpStatusCode.NotFound,
            InvalidOperationException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = (int)statusCode;

        // Include stack trace only in development
        if (_env.IsDevelopment() && exception is not UnauthorizedAccessException)
        {
            response.Data = new
            {
                exception.Message,
                exception.StackTrace,
                InnerException = exception.InnerException?.Message
            };
        }

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private static string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => "You are not authorized to perform this action.",
            ArgumentException => exception.Message,
            KeyNotFoundException => "The requested resource was not found.",
            InvalidOperationException => exception.Message,
            _ => "An error occurred while processing your request. Please try again later."
        };
    }

    private static string? GetErrorCode(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => "UNAUTHORIZED",
            ArgumentException => "INVALID_ARGUMENT",
            KeyNotFoundException => "NOT_FOUND",
            InvalidOperationException => "INVALID_OPERATION",
            _ => "INTERNAL_ERROR"
        };
    }
}

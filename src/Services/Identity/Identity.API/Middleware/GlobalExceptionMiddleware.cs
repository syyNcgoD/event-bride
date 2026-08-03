using System.Net;
using System.Text.Json;
using FluentValidation;
using Identity.Application.Common.Models;

namespace Identity.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        var errors = exception.Errors.Select(e => e.ErrorMessage).ToList();

        var response = ApiResponse<object>.Fail("اعتبارسنجی ناموفق بود", errors);
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        await WriteResponseAsync(context, response);
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = ApiResponse<object>.Fail("خطای داخلی سرور رخ داد");
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        await WriteResponseAsync(context, response);
    }

    private static async Task WriteResponseAsync(HttpContext context, object response)
    {
        context.Response.ContentType = "application/json";
        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}

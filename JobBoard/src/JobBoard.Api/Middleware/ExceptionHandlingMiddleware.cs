using System.Net;
using System.Text.Json;
using FluentValidation;
using JobBoard.Application.Common.Exceptions;

namespace JobBoard.Api.Middleware;

// Central place to translate domain/application exceptions into HTTP responses,
// so controllers stay free of try/catch blocks.
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
        context.Response.ContentType = "application/json";

        var (statusCode, errors) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                (object)validationEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })),

            NotFoundException => (HttpStatusCode.NotFound, new { message = exception.Message }),
            ForbiddenAccessException => (HttpStatusCode.Forbidden, new { message = exception.Message }),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, new { message = exception.Message }),

            _ => (HttpStatusCode.InternalServerError, new { message = "An unexpected error occurred." })
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception");
        }

        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { errors }));
    }
}

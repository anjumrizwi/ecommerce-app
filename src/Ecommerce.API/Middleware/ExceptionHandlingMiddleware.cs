using Ecommerce.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Ecommerce.API.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var logLevel = ex switch
            {
                ValidationException => LogLevel.Warning,
                Ecommerce.Domain.Exceptions.NotFoundException => LogLevel.Information,
                Ecommerce.Domain.Exceptions.ConflictException => LogLevel.Information,
                OperationCanceledException => LogLevel.Information,
                UnauthorizedAccessException => LogLevel.Warning,
                _ => LogLevel.Error
            };

            logger.Log(logLevel, ex,
                "Unhandled exception. Method: {RequestMethod}, Path: {RequestPath}, TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);

            await HandleExceptionAsync(context, ex, env.IsDevelopment());
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, bool isDevelopment)
    {
        var (statusCode, title, errors) = exception switch
        {
            // Validation — 400
            ValidationException ve =>
                (HttpStatusCode.BadRequest, "Validation Error", ve.Errors),

            // Business: resource not found — 404
            Ecommerce.Domain.Exceptions.NotFoundException =>
                (HttpStatusCode.NotFound, "Not Found", (IDictionary<string, string[]>?)null),

            // Business: uniqueness / state conflict — 409
            Ecommerce.Domain.Exceptions.ConflictException =>
                (HttpStatusCode.Conflict, "Conflict", (IDictionary<string, string[]>?)null),

            // Operation cancelled by client — 499-ish, surface as 400
            OperationCanceledException =>
                (HttpStatusCode.BadRequest, "Request Cancelled", (IDictionary<string, string[]>?)null),

            // Authorization failure from token/user identity extraction
            UnauthorizedAccessException =>
                (HttpStatusCode.Unauthorized, "Unauthorized", (IDictionary<string, string[]>?)null),

            // System / unexpected — 500
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", (IDictionary<string, string[]>?)null)
        };

        // In production, hide internal exception messages for 500s to avoid leaking implementation details.
        var detail = statusCode == HttpStatusCode.InternalServerError && !isDevelopment
            ? "An internal server error occurred. Please try again later."
            : exception.Message;

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail
        };

        // Correlation handle so callers can cross-reference server logs.
        problem.Extensions["traceId"] = context.TraceIdentifier;

        if (errors is not null)
            problem.Extensions["errors"] = errors;

        await context.Response.WriteAsJsonAsync(problem);
    }
}

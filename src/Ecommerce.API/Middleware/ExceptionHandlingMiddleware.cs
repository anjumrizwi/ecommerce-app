using Ecommerce.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Ecommerce.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, errors) = exception switch
        {
            ValidationException ve => (HttpStatusCode.BadRequest, "Validation Error", ve.Errors),
            Ecommerce.Domain.Exceptions.NotFoundException => (HttpStatusCode.NotFound, "Not Found", (IDictionary<string, string[]>?)null),
            _ => (HttpStatusCode.InternalServerError, "Server Error", (IDictionary<string, string[]>?)null)
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = exception.Message
        };

        if (errors is not null)
            problem.Extensions["errors"] = errors;

        await context.Response.WriteAsJsonAsync(problem);
    }
}

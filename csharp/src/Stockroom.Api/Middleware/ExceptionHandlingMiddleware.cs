using Microsoft.AspNetCore.Mvc;
using Stockroom.Business.Exceptions;
using Stockroom.Domain.Exceptions;

namespace Stockroom.Api.Middleware;

/// <summary>
/// Maps application and domain exceptions onto RFC 9457 problem details. Anything unrecognised
/// is logged and surfaced as a 500 without leaking the exception to the caller.
/// </summary>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            await next(context);
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            var problem = Map(ex);
            if (problem.Status >= StatusCodes.Status500InternalServerError)
            {
                logger.LogError(ex, "Unhandled exception while processing {Method} {Path}", context.Request.Method, context.Request.Path);
            }
            else
            {
                logger.LogWarning("Request {Method} {Path} failed with {Status}: {Detail}", context.Request.Method, context.Request.Path, problem.Status, problem.Detail);
            }

            problem.Instance = context.Request.Path;
            problem.Extensions["traceId"] = context.TraceIdentifier;
            context.Response.StatusCode = problem.Status!.Value;
            await context.Response.WriteAsJsonAsync(problem, problem.GetType(), context.RequestAborted);
        }
    }

    private static ProblemDetails Map(Exception exception) => exception switch
    {
        ValidationException validation => new ValidationProblemDetails(validation.Errors.ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed",
        },
        NotFoundException notFound => new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Resource not found",
            Detail = notFound.Message,
        },
        ConflictException conflict => new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Conflict",
            Detail = conflict.Message,
        },
        InsufficientStockException stock => new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Insufficient stock",
            Detail = stock.Message,
            Extensions =
            {
                ["sku"] = stock.Sku,
                ["requested"] = stock.Requested,
                ["available"] = stock.Available,
            },
        },
        DomainException domain => new ProblemDetails
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "Business rule violated",
            Detail = domain.Message,
        },
        _ => new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred",
        },
    };
}

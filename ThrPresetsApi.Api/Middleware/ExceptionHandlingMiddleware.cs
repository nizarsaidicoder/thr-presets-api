using ThrPresetsApi.Api.Common.Exceptions;

namespace ThrPresetsApi.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var statusCode = exception switch
            {
                ValidationException => StatusCodes.Status400BadRequest,
                BadRequestException => StatusCodes.Status400BadRequest,
                InvalidDataException => StatusCodes.Status400BadRequest,
                BadHttpRequestException => StatusCodes.Status400BadRequest,
                ConflictException => StatusCodes.Status409Conflict,
                NotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                ForbiddenException => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(new
            {
                Status = statusCode,
                Title = GetTitle(exception),
                Detail = exception.Message
            });
        }
    }

    private static string GetTitle(Exception exception) => exception switch
    {
        ValidationException => "Validation Error",
        BadRequestException or BadHttpRequestException or InvalidDataException => "Bad Request",
        ConflictException => "Conflict",
        NotFoundException => "Not Found",
        UnauthorizedAccessException => "Unauthorized",
        ForbiddenException => "Forbidden",
        _ => "Server Error"
    };
}
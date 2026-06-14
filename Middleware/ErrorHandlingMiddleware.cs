using System.Net;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
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
        var code = HttpStatusCode.InternalServerError;
        var result = JsonSerializer.Serialize(new { error = "Internal server error" });

        if (exception is NotFoundException)
        {
            code = HttpStatusCode.NotFound;
            result = JsonSerializer.Serialize(new { error = exception.Message });
        }
        else if (exception is ArgumentException || exception is ValidationException)
        {
            code = HttpStatusCode.BadRequest;
            result = JsonSerializer.Serialize(new { error = exception.Message });
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;
        return context.Response.WriteAsync(result);
    }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

using System.Net;
using System.Text.Json;

namespace GestionClientes.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
            _logger.LogError(ex, "Ha ocurrido un error no controlado.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            ArgumentException argEx => ((int)HttpStatusCode.BadRequest, argEx.Message),
            InvalidOperationException invEx => ((int)HttpStatusCode.Conflict, invEx.Message),
            KeyNotFoundException knfEx => ((int)HttpStatusCode.NotFound, knfEx.Message),
            _ => ((int)HttpStatusCode.InternalServerError, "Ha ocurrido un error interno en el servidor.")
        };

        context.Response.StatusCode = statusCode;

        var response = new
        {
            status = statusCode,
            message = message,
            timestamp = DateTime.UtcNow
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}

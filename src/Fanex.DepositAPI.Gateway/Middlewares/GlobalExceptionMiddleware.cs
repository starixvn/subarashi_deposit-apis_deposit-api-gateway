using System.Net;
using Fanex.DepositAPI.Gateway.Utils;

namespace Fanex.DepositAPI.Gateway.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Logging.ILogger _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, Logging.ILogger logger)
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
        catch (Exception exception)
        {
            var message = ErrorMessageUtils.GenerateMessage(exception);
            _logger.Error(message);

            context.Response.Clear();
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var result = new
            {
                code = 500,
                type = "",
                message = "An internal server error occurred while a request was processed."
            };

            await context.Response.WriteAsJsonAsync(result);
        }
    }
}
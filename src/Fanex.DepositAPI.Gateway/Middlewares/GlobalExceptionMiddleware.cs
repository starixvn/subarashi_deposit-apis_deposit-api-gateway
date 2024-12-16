namespace Fanex.DepositAPI.Gateway.Middlewares;

using System.Net;
using Logging;
using Utils;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var message = ErrorMessageUtils.GenerateMessage(exception);
            await logger.ErrorAsync(message);

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
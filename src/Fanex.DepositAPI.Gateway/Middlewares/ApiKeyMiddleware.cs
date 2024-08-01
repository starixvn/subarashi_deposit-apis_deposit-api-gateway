using System.Net;
using Fanex.DepositAPI.Gateway.Settings;
using Microsoft.Extensions.Options;

namespace Fanex.DepositAPI.Gateway.Middlewares;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AppSetting _appSetting;
    private const string APIKEY = "ApiKey";

    public ApiKeyMiddleware(RequestDelegate next, IOptions<AppSetting> appSetting)
    {
        _next = next;
        _appSetting = appSetting.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(APIKEY, out var extractedApiKey))
        {
            context.Response.Clear();
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";

            var result = new
            {
                code = 401,
                type = "API_KEY_REQUIRED",
                message = "API key is required."
            };

            await context.Response.WriteAsJsonAsync(result);
            return;
        }

        if (!string.IsNullOrEmpty(_appSetting.ApiKey) && !_appSetting.ApiKey.Equals(extractedApiKey))
        {
            context.Response.Clear();
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";

            var result = new
            {
                code = 401,
                type = "API_KEY_INVALID",
                message = "API key is invalid."
            };

            await context.Response.WriteAsJsonAsync(result);
            return;
        }
        await _next(context);
    }
}
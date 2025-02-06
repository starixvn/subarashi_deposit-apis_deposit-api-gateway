namespace Fanex.DepositAPI.Gateway.Startup;

using System.Net;
using Ocelot.Middleware;

public static class OcelotMiddlewareConfiguration
{
    public static IApplicationBuilder UseOcelot(this IApplicationBuilder app, IConfiguration configuration)
    {
        var allApiKeys = configuration
            .GetSection("ApiKeys")
            .GetChildren()
            .AsEnumerable()
            .ToDictionary(x => x.GetValue<string>("Key"), x => x.GetValue<string>("Value"));

        var ocelotPipelineConfig = new OcelotPipelineConfiguration
        {
            AuthenticationMiddleware = async (context, next) =>
            {
                if (!context.Request.Headers.TryGetValue("ApiKey", out var extractedApiKey))
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

                string key = context.Items.DownstreamRoute().Key;
                string apiKey = allApiKeys.GetValueOrDefault(key, string.Empty);

                if (!string.IsNullOrEmpty(apiKey) && !apiKey.Equals(extractedApiKey))
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

                await next.Invoke();
            }
        };

        app.UseOcelot(ocelotPipelineConfig).Wait();
        return app;
    }
}
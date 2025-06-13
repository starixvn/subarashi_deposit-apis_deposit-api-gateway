namespace Fanex.DepositAPI.Gateway.Startup;

using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ocelot.Middleware;

public static class OcelotMiddlewareConfiguration
{
    public static IApplicationBuilder UseOcelot(this IApplicationBuilder app, IConfiguration configuration)
    {
        var pathBase = configuration["PathBase"];

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

        app.UseSwaggerForOcelotUI(opt =>
        {
            if (!string.IsNullOrEmpty(pathBase))
            {
                opt.DownstreamSwaggerEndPointBasePath = $"{pathBase}/swagger/docs";
            }

            opt.PathToSwaggerGenerator = "/swagger/docs";

            opt.ReConfigureUpstreamSwaggerJson = (context, swaggerJson) =>
            {
                var swagger = JObject.Parse(swaggerJson);

                var components = swagger["components"] as JObject;
                if (components == null)
                {
                    components = new JObject();
                    swagger["components"] = components;
                }

                var securitySchemes = components["securitySchemes"] as JObject;
                if (securitySchemes == null)
                {
                    securitySchemes = new JObject();
                    components["securitySchemes"] = securitySchemes;
                }

                if (securitySchemes["ApiKey"] == null)
                {
                    securitySchemes["ApiKey"] = JObject.Parse(@"{
                    'type': 'apiKey',
                    'name': 'ApiKey',
                    'in': 'header',
                    'description': 'Enter your `API Key`'
                }");
                }

                var securityRequirements = swagger["security"] as JArray;
                if (securityRequirements == null)
                {
                    securityRequirements = new JArray();
                    swagger["security"] = securityRequirements;
                }

                var apiKeyRequirement = JObject.Parse(@"{ 'ApiKey': [] }");

                if (!securityRequirements.Any(item => JToken.DeepEquals(item, apiKeyRequirement)))
                {
                    securityRequirements.Add(apiKeyRequirement);
                }

                var servers = swagger["servers"] as JArray;
                if (servers == null)
                {
                    servers = new JArray();
                    swagger["servers"] = servers;
                }

                var request = context.Request;
                var ocelotBaseUrl = $"{request.Scheme}://{request.Host}{pathBase}";

                servers.Clear();
                servers.Add(JObject.Parse($@"{{ 'url': '{ocelotBaseUrl}' }}"));

                return swagger.ToString(Formatting.Indented);
            };
        });

        app.UseOcelot(ocelotPipelineConfig).Wait();
        return app;
    }
}
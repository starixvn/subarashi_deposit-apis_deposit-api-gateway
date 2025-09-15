using Fanex.DepositAPI.Gateway.DelegatingHandlers;
using Fanex.DepositAPI.Gateway.Middlewares;
using Fanex.DepositAPI.Gateway.Settings;
using Fanex.DepositAPI.Gateway.Startup;
using Microsoft.AspNetCore.HttpOverrides;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Provider.Polly;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Application is starting");
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();
    var appSetting = builder.Configuration.GetSection(AppSetting.SettingName).Get<AppSetting>();

    builder.Configuration
        .SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile("ocelot.json", true, true)
        .AddJsonFile($"ocelot.{appSetting.Environment}.json", true, true)
        .AddEnvironmentVariables();

    builder.Services
        .AddEndpointsApiExplorer()
        .AddSwaggerForOcelot(builder.Configuration, options => { options.GenerateDocsForGatewayItSelf = true; })
        .AddOcelot()
        .AddPolly()
        .AddCacheManager(x => { x.WithDictionaryHandle(); })
        .AddDelegatingHandler<NoGzipDelegatingHandler>(true);

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policyBuilder =>
            policyBuilder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
    });

    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
        options.ForwardLimit = null;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    builder.Services
        .AddDependencyInjection(builder.Configuration)
        .LoggingConfigure(builder.Configuration);

    var app = builder.Build();

    app.UseMiddleware<GlobalExceptionMiddleware>();

    app.UseForwardedHeaders();

    app.UseCors();

    app.UseRouting();

    var pathBase = builder.Configuration["PathBase"];
    if (!string.IsNullOrEmpty(pathBase))
    {
        app.UsePathBase(pathBase);
    }

    app.MapGet("/", () => "Hi, I'm Deposit API Gateway!");

    app.UseEndpoints(_ => { });

    app.UseWhen(
        context => context.Request.Path.StartsWithSegments("/api"),
        config => config.UseHttpLogging().UseMiddleware<UMMiddleware>());

    await app.UseOcelot(builder.Configuration);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
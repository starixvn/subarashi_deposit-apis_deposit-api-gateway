namespace Fanex.DepositAPI.Gateway.Startup;

using Logging;
using Logging.Sentry;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sentry;
using Serilog;
using Settings;
using ILogger = Logging.ILogger;

public static class LoggingServiceConfiguration
{
    private const int DefaultResponseBodyLogLimit = 4096;
    public static IServiceCollection LoggingConfigure(this IServiceCollection services, IConfiguration configuration)
    {
        var sentrySetting = configuration.GetSection(SentrySetting.SettingName).Get<SentrySetting>();
        var appSetting = configuration.GetSection(AppSetting.SettingName).Get<AppSetting>();

        // Sentry
        LogManager
            .SetDefaultLogCategory(sentrySetting.Environment)
            .Use(new List<ILogEngine>
            {
                new SentryLogging(new SentryEngineOptions
                {
                    Dsn = new Dsn(sentrySetting.Dsn),
                    Environment = sentrySetting.Environment,
                    MaxBreadcrumbs = sentrySetting.MaxBreadcrumbs,
                    SendDefaultPii = sentrySetting.SendDefaultPii
                })
            });

        // Serilog
        services.AddSerilog((s, l) => l
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(s)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("Team", "Subarashi")
            .Enrich.WithProperty("Application", "Deposit API Gateway")
            .Enrich.WithProperty("Environment", appSetting.Environment)
            .Enrich.WithProperty("Version", appSetting.Version)
            .Filter.ByExcluding("RequestPath like '/favicon.ico%' or RequestPath like '/swagger%' or RequestPath like '/whoami%'"));

        services.AddHttpLogging(options =>
        {
            options.LoggingFields = HttpLoggingFields.All | HttpLoggingFields.RequestQuery;
            options.RequestHeaders.Add("sec-ch-ua");
            options.MediaTypeOptions.AddText("application/javascript");
            options.ResponseBodyLogLimit = DefaultResponseBodyLogLimit;
            options.CombineLogs = true;
        });

        services.TryAddSingleton<ILogger, Logger>();

        return services;
    }
}
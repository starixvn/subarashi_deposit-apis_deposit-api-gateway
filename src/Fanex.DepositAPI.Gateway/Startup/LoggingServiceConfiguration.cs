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
            .Enrich.WithProperty("Team", "Legion")
            .Enrich.WithProperty("Application", "DepositAPIGateway")
            .Enrich.WithProperty("Environment", appSetting.Environment)
            .Enrich.WithProperty("Version", appSetting.Version));

        services.AddHttpLogging(logging =>
        {
            logging.LoggingFields = HttpLoggingFields.All | HttpLoggingFields.RequestQuery;
            logging.RequestHeaders.Add("sec-ch-ua");
            logging.MediaTypeOptions.AddText("application/javascript");
            logging.RequestBodyLogLimit = 4096;
            logging.ResponseBodyLogLimit = 4096;
            logging.CombineLogs = true;
        });

        services.TryAddSingleton<ILogger, Logger>();

        return services;
    }
}
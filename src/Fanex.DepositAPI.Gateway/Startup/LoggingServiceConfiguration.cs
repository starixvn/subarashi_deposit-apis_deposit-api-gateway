using Fanex.DepositAPI.Gateway.Settings;
using Fanex.Logging;
using Fanex.Logging.Sentry;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sentry;
using Serilog;

namespace Fanex.DepositAPI.Gateway.Startup;

public static class LoggingServiceConfiguration
{
    public static IServiceCollection LoggingConfigure(this IServiceCollection services, IConfiguration configuration)
    {
        var sentrySetting = configuration.GetSection(SentrySetting.SettingName).Get<SentrySetting>();
        var appsetting = configuration.GetSection(AppSetting.SettingName).Get<AppSetting>();

        // Sentry
        LogManager
            .SetDefaultLogCategory(sentrySetting.Enviroment)
            .Use(new List<ILogEngine>
            {
                new SentryLogging(new SentryEngineOptions
                {
                    Dsn = new Dsn(sentrySetting.Dsn),
                    Environment = sentrySetting.Enviroment,
                    MaxBreadcrumbs = sentrySetting.MaxBreadcrumbs,
                    SendDefaultPii = sentrySetting.SendDefaultPii,
                }),
            });

        // Serilog
        services.AddSerilog((s, l) => l
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(s)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("Team", "Legion")
            .Enrich.WithProperty("Application", "DepositAPIGateway")
            .Enrich.WithProperty("Enviroment", appsetting.Enviroment)
            .Enrich.WithProperty("Version", appsetting.Version));

        services.AddHttpLogging(logging =>
        {
            logging.LoggingFields = HttpLoggingFields.All | HttpLoggingFields.RequestQuery;
            logging.RequestHeaders.Add("sec-ch-ua");
            logging.MediaTypeOptions.AddText("application/javascript");
            logging.RequestBodyLogLimit = 4096;
            logging.ResponseBodyLogLimit = 4096;
            logging.CombineLogs = true;
        });

        services.TryAddSingleton<Logging.ILogger, Logging.Logger>();

        return services;
    }
}
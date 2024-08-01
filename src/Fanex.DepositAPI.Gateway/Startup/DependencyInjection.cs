using Fanex.DepositAPI.Gateway.Settings;

namespace Fanex.DepositAPI.Gateway.Startup;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppSetting>(configuration.GetSection(AppSetting.SettingName));
        services.Configure<SentrySetting>(configuration.GetSection(SentrySetting.SettingName));

        return services;
    }
}
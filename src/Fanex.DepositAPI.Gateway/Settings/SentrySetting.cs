namespace Fanex.DepositAPI.Gateway.Settings;

public class SentrySetting
{
    public const string SettingName = "Sentry";

    public string Environment { get; set; }

    public string Dsn { get; set; }

    public int MaxBreadcrumbs { get; set; }

    public bool SendDefaultPii { get; set; }
}
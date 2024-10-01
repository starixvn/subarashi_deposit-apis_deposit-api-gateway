namespace Fanex.DepositAPI.Gateway.Settings;

public class AppSetting
{
    public const string SettingName = "AppSetting";

    public string Version { get; set; }

    public string Environment { get; set; }

    public string ApiKey { get; set; }
}
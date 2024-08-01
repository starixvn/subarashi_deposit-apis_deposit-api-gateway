namespace Fanex.DepositAPI.Gateway.Settings;

public class AppSetting
{
    public const string SettingName = "AppSetting";

    public bool UseErrorHandler { get; set; }

    public string Version { get; set; }

    public string Enviroment { get; set; }

    public string ApiKey { get; set; }
}
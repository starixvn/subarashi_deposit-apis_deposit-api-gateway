namespace Fanex.DepositAPI.Gateway.UnitTests
{
    using FluentAssertions;
    using Settings;

    public class AppSettingTests
    {
        [Fact]
        public void SettingName_ShouldBeAppSetting()
        {
            AppSetting.SettingName.Should().Be("AppSetting");
        }

        [Fact]
        public void Version_ShouldBeSettableAndGettable()
        {
            var appSetting = new AppSetting();
            var version = "1.0.0";

            appSetting.Version = version;

            appSetting.Version.Should().Be(version);
        }

        [Fact]
        public void Environment_ShouldBeSettableAndGettable()
        {
            var appSetting = new AppSetting();
            var environment = "Production";

            appSetting.Environment = environment;

            appSetting.Environment.Should().Be(environment);
        }

        [Fact]
        public void UseSwagger_ShouldBeSettableAndGettable()
        {
            var appSetting = new AppSetting();

            appSetting.UseSwagger = true;
            appSetting.UseSwagger.Should().BeTrue();

            appSetting.UseSwagger = false;
            appSetting.UseSwagger.Should().BeFalse();
        }

        [Fact]
        public void AppSetting_ShouldInitializeWithDefaultValues()
        {
            var appSetting = new AppSetting();

            appSetting.Version.Should().BeNull();
            appSetting.Environment.Should().BeNull();
            appSetting.UseSwagger.Should().BeFalse();
        }
    }
}
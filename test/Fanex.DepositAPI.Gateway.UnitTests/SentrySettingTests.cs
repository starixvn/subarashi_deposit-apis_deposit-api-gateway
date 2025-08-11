namespace Fanex.DepositAPI.Gateway.UnitTests
{
    using FluentAssertions;
    using Settings;

    public class SentrySettingTests
    {
        [Fact]
        public void SettingName_ShouldBeSentry()
        {
            SentrySetting.SettingName.Should().Be("Sentry");
        }

        [Fact]
        public void Environment_ShouldBeSettableAndGettable()
        {
            var sentrySetting = new SentrySetting();
            var environment = "Production";

            sentrySetting.Environment = environment;

            sentrySetting.Environment.Should().Be(environment);
        }

        [Fact]
        public void Dsn_ShouldBeSettableAndGettable()
        {
            var sentrySetting = new SentrySetting();
            var dsn = "https://test@sentry.io/123";

            sentrySetting.Dsn = dsn;

            sentrySetting.Dsn.Should().Be(dsn);
        }

        [Fact]
        public void MaxBreadcrumbs_ShouldBeSettableAndGettable()
        {
            var sentrySetting = new SentrySetting();
            var maxBreadcrumbs = 50;

            sentrySetting.MaxBreadcrumbs = maxBreadcrumbs;

            sentrySetting.MaxBreadcrumbs.Should().Be(maxBreadcrumbs);
        }

        [Fact]
        public void SendDefaultPii_ShouldBeSettableAndGettable()
        {
            var sentrySetting = new SentrySetting();

            sentrySetting.SendDefaultPii = true;
            sentrySetting.SendDefaultPii.Should().BeTrue();

            sentrySetting.SendDefaultPii = false;
            sentrySetting.SendDefaultPii.Should().BeFalse();
        }

        [Fact]
        public void SentrySetting_ShouldInitializeWithDefaultValues()
        {
            var sentrySetting = new SentrySetting();

            sentrySetting.Environment.Should().BeNull();
            sentrySetting.Dsn.Should().BeNull();
            sentrySetting.MaxBreadcrumbs.Should().Be(0);
            sentrySetting.SendDefaultPii.Should().BeFalse();
        }
    }
}
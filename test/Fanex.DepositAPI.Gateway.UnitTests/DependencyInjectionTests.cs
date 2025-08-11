namespace Fanex.DepositAPI.Gateway.UnitTests
{
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using NSubstitute;
    using ONEbook.UM.Configurations;
    using Settings;
    using Startup;

    public class DependencyInjectionTests
    {
        private readonly IServiceCollection _services;
        private readonly IConfiguration _configuration;
        private readonly IConfigurationSection _appSettingSection;
        private readonly IConfigurationSection _sentrySettingSection;

        public DependencyInjectionTests()
        {
            _services = new ServiceCollection();
            _configuration = Substitute.For<IConfiguration>();
            _appSettingSection = Substitute.For<IConfigurationSection>();
            _sentrySettingSection = Substitute.For<IConfigurationSection>();

            _configuration.GetSection(AppSetting.SettingName).Returns(_appSettingSection);
            _configuration.GetSection(SentrySetting.SettingName).Returns(_sentrySettingSection);
        }

        [Fact]
        public void AddDependencyInjection_ShouldReturnServiceCollection()
        {
            var result = _services.AddDependencyInjection(_configuration);

            result.Should().BeSameAs(_services);
        }

        [Fact]
        public void AddDependencyInjection_ShouldConfigureAppSetting()
        {
            _services.AddDependencyInjection(_configuration);

            _configuration.Received(1).GetSection(AppSetting.SettingName);
            
            var serviceProvider = _services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<AppSetting>>();
            options.Should().NotBeNull();
        }

        [Fact]
        public void AddDependencyInjection_ShouldConfigureSentrySetting()
        {
            _services.AddDependencyInjection(_configuration);

            _configuration.Received(1).GetSection(SentrySetting.SettingName);
            
            var serviceProvider = _services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<SentrySetting>>();
            options.Should().NotBeNull();
        }

        [Fact]
        public void AddDependencyInjection_ShouldRegisterUMClientConfigurationAsSingleton()
        {
            _services.AddDependencyInjection(_configuration);

            var serviceProvider = _services.BuildServiceProvider();
            var config1 = serviceProvider.GetService<UMClientConfiguration>();
            var config2 = serviceProvider.GetService<UMClientConfiguration>();

            config1.Should().NotBeNull();
            config2.Should().NotBeNull();
            config1.Should().BeSameAs(config2);
        }

        [Fact]
        public void AddDependencyInjection_ShouldRegisterUMServices()
        {
            _services.AddDependencyInjection(_configuration);

            var serviceProvider = _services.BuildServiceProvider();
            
            serviceProvider.Should().NotBeNull();
            _services.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public void AddDependencyInjection_ShouldCallConfigurationGetSection()
        {
            _services.AddDependencyInjection(_configuration);

            _configuration.Received().GetSection(AppSetting.SettingName);
            _configuration.Received().GetSection(SentrySetting.SettingName);
        }

        [Fact]
        public void AddDependencyInjection_ShouldHandleNullConfiguration()
        {
            var action = () => _services.AddDependencyInjection(null);

            action.Should().Throw<NullReferenceException>();
        }

        [Fact]
        public void AddDependencyInjection_ShouldHandleNullServices()
        {
            IServiceCollection nullServices = null;
            
            var action = () => nullServices.AddDependencyInjection(_configuration);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}
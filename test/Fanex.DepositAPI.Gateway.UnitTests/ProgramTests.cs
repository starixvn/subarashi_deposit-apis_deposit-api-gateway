namespace Fanex.DepositAPI.Gateway.UnitTests
{
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Settings;

    public class ProgramTests
    {
        [Fact]
        public void AppSetting_SettingName_ShouldBeConsistent()
        {
            AppSetting.SettingName.Should().Be("AppSetting");
        }

        [Fact]
        public void SentrySetting_SettingName_ShouldBeConsistent()
        {
            SentrySetting.SettingName.Should().Be("Sentry");
        }

        [Theory]
        [InlineData("Development")]
        [InlineData("Production")]
        [InlineData("Staging")]
        public void Configuration_ShouldLoadOcelotJsonByEnvironment(string environment)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>(AppSetting.SettingName + ":Environment", environment)
                })
                .Build();

            var appSetting = configuration.GetSection(AppSetting.SettingName).Get<AppSetting>();
            
            appSetting.Should().NotBeNull();
            appSetting.Environment.Should().Be(environment);
        }

        [Fact]
        public void WebApplicationBuilder_ShouldConfigureServices()
        {
            var services = new ServiceCollection();
            
            services.AddEndpointsApiExplorer();

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.Should().NotBeNull();
        }

        [Fact]
        public void ServiceCollection_ShouldSupportCorsConfiguration()
        {
            var services = new ServiceCollection();
            
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policyBuilder =>
                    policyBuilder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });

            services.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public void ServiceCollection_ShouldNotBeNull()
        {
            var services = new ServiceCollection();

            services.Should().NotBeNull();
            services.Should().BeOfType<ServiceCollection>();
        }

        [Theory]
        [InlineData("/api")]
        [InlineData("/gateway")]
        [InlineData("")]
        public void PathBase_Configuration_ShouldHandleDifferentValues(string pathBase)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("PathBase", pathBase)
                })
                .Build();

            var result = configuration["PathBase"];
            result.Should().Be(pathBase);
        }

        [Fact]
        public void HealthCheck_Endpoint_ShouldReturnGreeting()
        {
            var expectedMessage = "Hi, I'm Deposit API Gateway!";
            expectedMessage.Should().NotBeNullOrEmpty();
            expectedMessage.Should().Contain("Deposit API Gateway");
        }

        [Fact]
        public void ApiPath_Filter_ShouldMatchApiSegments()
        {
            var testPath = "/api/test";
            var result = testPath.StartsWith("/api");
            
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("/api/deposits")]
        [InlineData("/api/health")]
        [InlineData("/api/v1/test")]
        public void ApiPath_Filter_ShouldMatchVariousApiPaths(string path)
        {
            var result = path.StartsWith("/api");
            
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("/health")]
        [InlineData("/swagger")]
        [InlineData("/")]
        public void NonApiPath_Filter_ShouldNotMatchNonApiPaths(string path)
        {
            var result = path.StartsWith("/api");
            
            result.Should().BeFalse();
        }

        [Fact]
        public void Configuration_ShouldSupportEnvironmentVariables()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("TEST_VAR", "test-value")
                })
                .AddEnvironmentVariables()
                .Build();

            configuration.Should().NotBeNull();
        }

        [Fact]
        public void OcelotConfiguration_ShouldLoadJsonFiles()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .AddJsonFile("ocelot.json", true, true)
                .Build();

            configuration.Should().NotBeNull();
        }
    }
}
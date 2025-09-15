namespace Fanex.DepositAPI.Gateway.UnitTests
{
    using System.Globalization;
    using AutoFixture;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Middlewares;
    using NSubstitute;
    using ONEbook.UM.Models;
    using ONEbook.UM.Services;

    public class UMMiddlewareTests
    {
        private readonly Fixture _fixture;
        private readonly RequestDelegate _next;
        private readonly IMaintenanceService _maintenanceService;
        private readonly UMMiddleware _sut;
        private readonly DefaultHttpContext _httpContext;

        public UMMiddlewareTests()
        {
            _fixture = new Fixture();
            _next = Substitute.For<RequestDelegate>();
            _maintenanceService = Substitute.For<IMaintenanceService>();
            _sut = new UMMiddleware(_next, _maintenanceService);
            _httpContext = new DefaultHttpContext
            {
                Response =
                {
                    Body = new MemoryStream()
                }
            };
        }

        [Fact]
        public async Task InvokeAsync_ShouldCallNext_WhenNotUnderMaintenance()
        {
            // Arrange
            var umInfo = new UnderMaintenanceTime
            {
                IsUnderMaintenanceTime = false,
                From = _fixture.Create<DateTime>().ToString(CultureInfo.InvariantCulture),
                To = _fixture.Create<DateTime>().ToString(CultureInfo.InvariantCulture)
            };
            _maintenanceService.GetUnderMaintenanceTimeAsync().Returns(umInfo);

            // Act
            await _sut.InvokeAsync(_httpContext);

            // Assert
            await _next.Received(1).Invoke(_httpContext);
        }

        [Fact]
        public async Task InvokeAsync_ShouldReturnServiceUnavailable_WhenUnderMaintenance()
        {
            // Arrange
            var fromTime = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
            var toTime = DateTime.UtcNow.AddHours(2).ToString(CultureInfo.InvariantCulture);
            var umInfo = new UnderMaintenanceTime
            {
                IsUnderMaintenanceTime = true,
                From = fromTime,
                To = toTime
            };
            _maintenanceService.GetUnderMaintenanceTimeAsync().Returns(umInfo);

            // Act
            await _sut.InvokeAsync(_httpContext);

            // Assert
            _httpContext.Response.StatusCode.Should().Be(503);
            _httpContext.Response.ContentType.Should().Be("application/json");
            await _next.DidNotReceive().Invoke(Arg.Any<HttpContext>());
        }

        [Fact]
        public async Task InvokeAsync_ShouldWriteMaintenanceMessage_WhenUnderMaintenance()
        {
            // Arrange
            var fromTime = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
            var toTime = DateTime.UtcNow.AddHours(2).ToString(CultureInfo.InvariantCulture);
            var umInfo = new UnderMaintenanceTime
            {
                IsUnderMaintenanceTime = true,
                From = fromTime,
                To = toTime
            };
            _maintenanceService.GetUnderMaintenanceTimeAsync().Returns(umInfo);

            // Act
            await _sut.InvokeAsync(_httpContext);

            // Assert
            _httpContext.Response.Body.Position = 0;
            var responseContent = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            var expectedMessage = $"Sorry, the system is under maintenance from {umInfo.From} to {umInfo.To} GMT+8. Please come back later.";
            
            responseContent.Should().Be(expectedMessage);
        }

        [Fact]
        public async Task InvokeAsync_ShouldGetMaintenanceInfo_Always()
        {
            // Arrange
            var umInfo = new UnderMaintenanceTime
            {
                IsUnderMaintenanceTime = false,
                From = _fixture.Create<DateTime>().ToString(CultureInfo.InvariantCulture),
                To = _fixture.Create<DateTime>().ToString(CultureInfo.InvariantCulture)
            };
            _maintenanceService.GetUnderMaintenanceTimeAsync().Returns(umInfo);

            // Act
            await _sut.InvokeAsync(_httpContext);

            // Assert
            await _maintenanceService.Received(1).GetUnderMaintenanceTimeAsync();
        }
    }
}
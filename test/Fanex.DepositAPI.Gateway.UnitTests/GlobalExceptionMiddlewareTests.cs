namespace Fanex.DepositAPI.Gateway.UnitTests
{
    using System.Text.Json;
    using AutoFixture;
    using FluentAssertions;
    using Logging;
    using Microsoft.AspNetCore.Http;
    using Middlewares;
    using NSubstitute;

    public class GlobalExceptionMiddlewareTests
    {
        private readonly Fixture _fixture;
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly GlobalExceptionMiddleware _sut;
        private readonly DefaultHttpContext _httpContext;

        public GlobalExceptionMiddlewareTests()
        {
            _fixture = new Fixture();
            _next = Substitute.For<RequestDelegate>();
            _logger = Substitute.For<ILogger>();
            _sut = new GlobalExceptionMiddleware(_next, _logger);
            _httpContext = new DefaultHttpContext();
            _httpContext.Response.Body = new MemoryStream();
        }

        [Fact]
        public async Task InvokeAsync_ShouldCallNext_WhenNoExceptionThrown()
        {
            // Arrange
            _next.When(x => x(_httpContext)).Do(x => { });

            // Act
            await _sut.InvokeAsync(_httpContext);

            // Assert
            await _next.Received(1).Invoke(_httpContext);
        }

        [Fact]
        public async Task InvokeAsync_ShouldHandleExceptionAndReturnError_WhenExceptionThrown()
        {
            // Arrange
            var exception = _fixture.Create<Exception>();
            _next.When(x => x(_httpContext)).Do(x => { throw exception; });

            // Act
            await _sut.InvokeAsync(_httpContext);

            // Assert
            _httpContext.Response.StatusCode.Should().Be(500);
            _httpContext.Response.ContentType.Should().Be("application/json; charset=utf-8");
            await _logger.Received(1).ErrorAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task InvokeAsync_ShouldLogErrorMessage_WhenExceptionThrown()
        {
            // Arrange
            var exception = _fixture.Create<Exception>();
            _next.When(x => x(_httpContext)).Do(x => { throw exception; });

            // Act
            await _sut.InvokeAsync(_httpContext);

            // Assert
            await _logger.Received(1).ErrorAsync(Arg.Is<string>(msg => 
                msg.Contains($"ErrMsg: {exception.Message}")));
        }

        [Fact]
        public async Task InvokeAsync_ShouldWriteJsonErrorResponse_WhenExceptionThrown()
        {
            // Arrange
            var exception = _fixture.Create<Exception>();
            _next.When(x => x(_httpContext)).Do(x => { throw exception; });

            // Act
            await _sut.InvokeAsync(_httpContext);

            // Assert
            _httpContext.Response.Body.Position = 0;
            var responseContent = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            errorResponse.GetProperty("code").GetInt32().Should().Be(500);
            errorResponse.GetProperty("type").GetString().Should().Be("");
            errorResponse.GetProperty("message").GetString().Should().Be("An internal server error occurred while a request was processed.");
        }
    }
}
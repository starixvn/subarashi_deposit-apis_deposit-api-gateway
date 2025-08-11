namespace Fanex.DepositAPI.Gateway.UnitTests
{
    using System.Net;
    using System.Text;
    using AutoFixture;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Utils;

    public class ErrorMessageUtilsTests
    {
        private readonly Fixture _fixture;
        private readonly DefaultHttpContext _httpContext;

        public ErrorMessageUtilsTests()
        {
            _fixture = new Fixture();
            _httpContext = new DefaultHttpContext();
        }

        [Fact]
        public void GenerateMessage_ShouldIncludeExceptionMessage_Always()
        {
            // Arrange
            var exception = new Exception(_fixture.Create<string>());

            // Act
            var result = ErrorMessageUtils.GenerateMessage(exception);

            // Assert
            result.Should().Contain($"ErrMsg: {exception.Message}");
        }

        [Fact]
        public void GenerateMessage_ShouldNotIncludeRequestInfo_WhenContextIsNull()
        {
            // Arrange
            var exception = _fixture.Create<Exception>();

            // Act
            var result = ErrorMessageUtils.GenerateMessage(exception);

            // Assert
            result.Should().NotContain("REQUEST INFO");
            result.Should().NotContain("Host:");
            result.Should().NotContain("Path:");
        }

        [Fact]
        public void GenerateMessage_ShouldIncludeRequestInfo_WhenContextProvided()
        {
            // Arrange
            var exception = _fixture.Create<Exception>();
            _httpContext.Request.Host = new HostString("test.com");
            _httpContext.Request.Path = "/api/test";
            _httpContext.Request.Method = "GET";

            // Act
            var result = ErrorMessageUtils.GenerateMessage(exception, _httpContext);

            // Assert
            result.Should().Contain("REQUEST INFO");
            result.Should().Contain("Host: test.com");
            result.Should().Contain("Path: /api/test");
            result.Should().Contain("Method: GET");
        }

        [Fact]
        public void GenerateMessage_ShouldIncludeExceptionInfo_Always()
        {
            // Arrange
            var exception = _fixture.Create<Exception>();

            // Act
            var result = ErrorMessageUtils.GenerateMessage(exception);

            // Assert
            result.Should().Contain("EXCEPTION INFO");
            result.Should().Contain($"Message: {exception.Message}");
            result.Should().Contain($"Source: {exception.Source}");
        }

        [Fact]
        public void GenerateMessage_ShouldIncludeInnerException_WhenPresent()
        {
            // Arrange
            var innerException = new ArgumentException(_fixture.Create<string>());
            var outerException = new InvalidOperationException(_fixture.Create<string>(), innerException);

            // Act
            var result = ErrorMessageUtils.GenerateMessage(outerException);

            // Assert
            result.Should().Contain($"Message: {outerException.Message}");
            result.Should().Contain($"Message: {innerException.Message}");
        }

        [Fact]
        public void GenerateMessage_ShouldLimitInnerExceptions_ToThreeLevels()
        {
            // Arrange
            var level5Exception = new Exception("Level 5");
            var level4Exception = new Exception("Level 4", level5Exception);
            var level3Exception = new Exception("Level 3", level4Exception);
            var level2Exception = new Exception("Level 2", level3Exception);
            var level1Exception = new Exception("Level 1", level2Exception);

            // Act
            var result = ErrorMessageUtils.GenerateMessage(level1Exception);

            // Assert
            result.Should().Contain("Level 1");
            result.Should().Contain("Level 2");
            result.Should().Contain("Level 3");
            result.Should().Contain("Level 4");
            result.Should().NotContain("Level 5");
        }

        [Fact]
        public void GenerateMessage_ShouldIncludeRequestHeaders_WhenContextProvided()
        {
            // Arrange
            var exception = _fixture.Create<Exception>();
            _httpContext.Request.Headers["Authorization"] = "Bearer token";
            _httpContext.Request.Headers["User-Agent"] = "TestAgent";

            // Act
            var result = ErrorMessageUtils.GenerateMessage(exception, _httpContext);

            // Assert
            result.Should().Contain("Authorization:Bearer token");
            result.Should().Contain("User-Agent:TestAgent");
        }

        [Fact]
        public void GenerateMessage_ShouldIncludeRequestTime_WhenContextProvided()
        {
            // Arrange
            var exception = _fixture.Create<Exception>();

            // Act
            var result = ErrorMessageUtils.GenerateMessage(exception, _httpContext);

            // Assert
            result.Should().Contain("Request Time:");
        }

        [Theory]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        public void GenerateMessage_ShouldIncludeHttpMethod_WhenContextProvided(string httpMethod)
        {
            // Arrange
            var exception = _fixture.Create<Exception>();
            _httpContext.Request.Method = httpMethod;

            // Act
            var result = ErrorMessageUtils.GenerateMessage(exception, _httpContext);

            // Assert
            result.Should().Contain($"Method: {httpMethod}");
        }

        [Fact]
        public void GenerateMessage_ShouldIncludeRequestBody_WhenContextProvided()
        {
            // Arrange
            var exception = _fixture.Create<Exception>();
            var bodyContent = "test body content";
            var bodyBytes = Encoding.UTF8.GetBytes(bodyContent);
            _httpContext.Request.Body = new MemoryStream(bodyBytes);
            _httpContext.Request.ContentLength = bodyBytes.Length;

            // Act
            var result = ErrorMessageUtils.GenerateMessage(exception, _httpContext);

            // Assert
            result.Should().Contain("Body:");
        }

        [Fact]
        public void GenerateMessage_ShouldIncludeCookies_WhenContextProvided()
        {
            // Arrange
            var exception = _fixture.Create<Exception>();
            // DefaultHttpContext already has an empty cookie collection

            // Act
            var result = ErrorMessageUtils.GenerateMessage(exception, _httpContext);

            // Assert
            result.Should().Contain("Cookies:");
        }

        [Fact]
        public void GenerateMessage_ShouldIncludeConnectionInfo_WhenContextProvided()
        {
            // Arrange
            var exception = _fixture.Create<Exception>();
            _httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");
            _httpContext.Connection.LocalIpAddress = IPAddress.Parse("127.0.0.1");

            // Act
            var result = ErrorMessageUtils.GenerateMessage(exception, _httpContext);

            // Assert
            result.Should().Contain("Remote IP: 192.168.1.1");
            result.Should().Contain("Local IP: 127.0.0.1");
        }

        [Fact]
        public void GenerateMessage_ShouldIncludeContentTypeAndLength_WhenContextProvided()
        {
            // Arrange
            var exception = _fixture.Create<Exception>();
            _httpContext.Request.ContentType = "application/json";
            _httpContext.Request.ContentLength = 1024;

            // Act
            var result = ErrorMessageUtils.GenerateMessage(exception, _httpContext);

            // Assert
            result.Should().Contain("Content Type: application/json");
            result.Should().Contain("Content Length: 1024");
        }

        [Fact]
        public void GenerateMessage_ShouldHandleNullContentLength_WhenContextProvided()
        {
            // Arrange
            var exception = _fixture.Create<Exception>();
            _httpContext.Request.ContentLength = null;

            // Act
            var result = ErrorMessageUtils.GenerateMessage(exception, _httpContext);

            // Assert
            result.Should().Contain("Content Length: N/A");
        }

        [Fact]
        public void GenerateMessage_ShouldIncludeQueryString_WhenContextProvided()
        {
            // Arrange
            var exception = _fixture.Create<Exception>();
            _httpContext.Request.QueryString = new QueryString("?param1=value1&param2=value2");

            // Act
            var result = ErrorMessageUtils.GenerateMessage(exception, _httpContext);

            // Assert
            result.Should().Contain("Query String: ?param1=value1&param2=value2");
        }
    }
}
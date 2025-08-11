namespace Fanex.DepositAPI.Gateway.UnitTests
{
    using System.Net.Http.Headers;
    using AutoFixture;
    using DelegatingHandlers;
    using FluentAssertions;

    public class NoGzipDelegatingHandlerTests
    {
        private readonly Fixture _fixture;

        public NoGzipDelegatingHandlerTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public async Task SendAsync_ShouldClearAndSetCorrectEncodingHeaders_Always()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            var handler = new NoGzipDelegatingHandler
            {
                InnerHandler = mockHandler
            };

            using var client = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("identity"));

            // Act
            await client.SendAsync(request);

            // Assert
            var capturedRequest = mockHandler.CapturedRequest;
            capturedRequest.Headers.AcceptEncoding.Should().NotContain(x => x.Value == "gzip");
            capturedRequest.Headers.AcceptEncoding.Should().NotContain(x => x.Value == "identity");
            capturedRequest.Headers.AcceptEncoding.Should().Contain(x => x.Value == "deflate");
            capturedRequest.Headers.AcceptEncoding.Should().Contain(x => x.Value == "br");
        }

        [Fact]
        public async Task SendAsync_ShouldPreserveRequestMethod_Always()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            var handler = new NoGzipDelegatingHandler
            {
                InnerHandler = mockHandler
            };

            using var client = new HttpClient(handler);
            var httpMethod = HttpMethod.Post;
            var request = new HttpRequestMessage(httpMethod, "http://test.com");

            // Act
            await client.SendAsync(request);

            // Assert
            mockHandler.CapturedRequest.Method.Should().Be(httpMethod);
        }

        [Fact]
        public async Task SendAsync_ShouldCallInnerHandler_Always()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            var handler = new NoGzipDelegatingHandler
            {
                InnerHandler = mockHandler
            };

            using var client = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            mockHandler.WasCalled.Should().BeTrue();
            response.Should().NotBeNull();
        }

        private class MockHttpMessageHandler : HttpMessageHandler
        {
            public HttpRequestMessage CapturedRequest { get; private set; }
            public bool WasCalled { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                CapturedRequest = request;
                WasCalled = true;
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            }
        }
    }
}
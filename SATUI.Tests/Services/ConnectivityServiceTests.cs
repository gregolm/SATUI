using System.Net;
using FluentAssertions;
using Moq;
using SATUI.Services;

namespace SATUI.Tests.Services;

public class ConnectivityServiceTests
{
    private static ConnectivityService CreateSut(HttpMessageHandler handler)
    {
        var client = new HttpClient(handler);
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock
            .Setup(f => f.CreateClient(nameof(ConnectivityService)))
            .Returns(client);
        return new ConnectivityService(factoryMock.Object);
    }

    [Fact]
    public async Task IsReachableAsync_WhenServerReturns200_ReturnsTrue()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var sut = CreateSut(handler);

        var result = await sut.IsReachableAsync("https://www.example.com");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsReachableAsync_WhenServerReturns404_ReturnsTrue()
    {
        // 404 means the server is reachable even if the page isn't found
        var handler = new FakeHttpMessageHandler(HttpStatusCode.NotFound);
        var sut = CreateSut(handler);

        var result = await sut.IsReachableAsync("https://www.example.com");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsReachableAsync_WhenServerReturns500_ReturnsFalse()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.InternalServerError);
        var sut = CreateSut(handler);

        var result = await sut.IsReachableAsync("https://www.example.com");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsReachableAsync_WhenNetworkThrows_ReturnsFalse()
    {
        var handler = new ThrowingHttpMessageHandler(new HttpRequestException("Network error"));
        var sut = CreateSut(handler);

        var result = await sut.IsReachableAsync("https://www.example.com");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsReachableAsync_WhenUrlIsMalformed_ReturnsFalse()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var sut = CreateSut(handler);

        var result = await sut.IsReachableAsync("not-a-valid-url");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsReachableAsync_WhenUrlIsEmpty_ReturnsFalse()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var sut = CreateSut(handler);

        var result = await sut.IsReachableAsync(string.Empty);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsReachableAsync_WhenCancelled_ReturnsFalse()
    {
        var handler = new ThrowingHttpMessageHandler(new OperationCanceledException());
        var sut = CreateSut(handler);

        var result = await sut.IsReachableAsync("https://www.example.com");

        result.Should().BeFalse();
    }

    // Minimal fake handlers to avoid heavy mocking of HttpMessageHandler internals
    private sealed class FakeHttpMessageHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(statusCode));
    }

    private sealed class ThrowingHttpMessageHandler(Exception exception) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => throw exception;
    }
}

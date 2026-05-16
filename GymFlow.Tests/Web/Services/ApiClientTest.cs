#nullable disable

using System.Net;
using System.Text;
using System.Text.Json;
using GymFlow.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace GymFlow.Tests.Web.Services;

/// <summary>
/// پیاده‌سازی واقعی ISession برای تست
/// </summary>
public class TestSession : ISession
{
    private readonly Dictionary<string, byte[]> _store = new Dictionary<string, byte[]>();

    public bool IsAvailable => true;
    public string Id => Guid.NewGuid().ToString();
    public IEnumerable<string> Keys => _store.Keys;

    public void Clear() => _store.Clear();
    
    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    
    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    
    public void Remove(string key) => _store.Remove(key);
    
    public void Set(string key, byte[] value) => _store[key] = value;
    
    public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
}

public class ApiClientTest
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILogger<ApiClient>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly TestSession _testSession;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly ApiClient _apiClient;

    public ApiClientTest()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockLogger = new Mock<ILogger<ApiClient>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _testSession = new TestSession();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        _mockConfiguration.Setup(c => c["ApiBaseUrl"]).Returns("http://test-api.com/");

        var httpContext = new DefaultHttpContext();
        httpContext.Session = _testSession;
        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

        _httpClient = new HttpClient(_mockHttpMessageHandler.Object) { BaseAddress = new Uri("http://test-api.com/") };
        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(_httpClient);

        _apiClient = new ApiClient(
            _mockHttpClientFactory.Object,
            _mockLogger.Object,
            _mockConfiguration.Object,
            _mockHttpContextAccessor.Object);
    }

    private void SetupHttpResponse(HttpMethod method, string requestUrl, HttpStatusCode statusCode, string content)
    {
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == method &&
                    req.RequestUri!.ToString() == $"http://test-api.com/{requestUrl}"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            });
    }

    private void SetupHttpResponseWithAuth(HttpMethod method, string requestUrl, HttpStatusCode statusCode, string content, string expectedAuthToken)
    {
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == method &&
                    req.RequestUri!.ToString() == $"http://test-api.com/{requestUrl}" &&
                    req.Headers.Authorization != null &&
                    req.Headers.Authorization.ToString() == expectedAuthToken),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            });
    }

    private void SetupHttpResponseBytes(HttpMethod method, string requestUrl, HttpStatusCode statusCode, byte[] contentBytes)
    {
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == method &&
                    req.RequestUri!.ToString() == $"http://test-api.com/{requestUrl}"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new ByteArrayContent(contentBytes)
            });
    }

    [Fact]
    public void Constructor_ShouldNotThrow()
    {
        var client = new ApiClient(
            Mock.Of<IHttpClientFactory>(),
            Mock.Of<ILogger<ApiClient>>(),
            Mock.Of<IConfiguration>(),
            Mock.Of<IHttpContextAccessor>());
        Assert.NotNull(client);
    }

    [Fact]
    public void IsLoggedIn_NoToken_ReturnsFalse()
    {
        // Session بدون توکن
        Assert.False(_apiClient.IsLoggedIn);
    }

    [Fact]
    public void IsLoggedIn_WithToken_ReturnsTrue()
    {
        // تنظیم توکن در session
        _testSession.SetString("AuthToken", "Basic token");
        Assert.True(_apiClient.IsLoggedIn);
    }

    [Fact]
    public async Task LoginAsync_Success_ReturnsTrueAndUserId()
    {
        var responseContent = JsonSerializer.Serialize(new
        {
            success = true,
            data = new { id = 123, username = "test", role = "Member" }
        });
        SetupHttpResponse(HttpMethod.Post, "api/auth/login", HttpStatusCode.OK, responseContent);

        var (success, userId) = await _apiClient.LoginAsync("testuser", "testpass");

        Assert.True(success);
        Assert.Equal(123, userId);
        Assert.True(_apiClient.IsLoggedIn);
    }

    [Fact]
    public async Task LoginAsync_Failure_ReturnsFalseAndUserIdZero()
    {
        SetupHttpResponse(HttpMethod.Post, "api/auth/login", HttpStatusCode.Unauthorized, "Unauthorized");

        var (success, userId) = await _apiClient.LoginAsync("wrong", "wrong");

        Assert.False(success);
        Assert.Equal(0, userId);
        Assert.False(_apiClient.IsLoggedIn);
    }

    [Fact]
    public async Task LoginAsync_DeserializationFailure_StillReturnsSuccessWithZeroUserId()
    {
        var responseContent = "{ invalid json }";
        SetupHttpResponse(HttpMethod.Post, "api/auth/login", HttpStatusCode.OK, responseContent);

        var (success, userId) = await _apiClient.LoginAsync("test", "pass");

        Assert.True(success);
        Assert.Equal(0, userId);
    }

    [Fact]
    public void Logout_ShouldClearToken()
    {
        _testSession.SetString("AuthToken", "Basic token");
        Assert.True(_apiClient.IsLoggedIn);
        
        _apiClient.Logout();
        Assert.False(_apiClient.IsLoggedIn);
    }

    [Fact]
    public async Task GetAsync_Success_ReturnsData()
    {
        var responseData = new { Name = "Test" };
        var apiResponse = new { success = true, data = responseData };
        var json = JsonSerializer.Serialize(apiResponse);
        SetupHttpResponse(HttpMethod.Get, "api/test", HttpStatusCode.OK, json);

        var result = await _apiClient.GetAsync<dynamic>("api/test");

        Assert.NotNull(result);
        var resultStr = JsonSerializer.Serialize(result);
        Assert.Contains("Test", resultStr);
    }

    [Fact]
    public async Task GetAsync_Unauthorized_ReturnsDefault()
    {
        SetupHttpResponse(HttpMethod.Get, "api/secure", HttpStatusCode.Unauthorized, "");

        var result = await _apiClient.GetAsync<object>("api/secure");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_Exception_ReturnsDefault()
    {
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var result = await _apiClient.GetAsync<object>("api/test");

        Assert.Null(result);
    }

    [Fact]
    public async Task PostAsync_Success_ReturnsData()
    {
        var responseData = new { Id = 5 };
        var apiResponse = new { success = true, data = responseData };
        var json = JsonSerializer.Serialize(apiResponse);
        SetupHttpResponse(HttpMethod.Post, "api/post", HttpStatusCode.OK, json);

        var result = await _apiClient.PostAsync<dynamic>("api/post", new { Name = "Test" });

        Assert.NotNull(result);
        var resultStr = JsonSerializer.Serialize(result);
        Assert.Contains("5", resultStr);
    }

    [Fact]
    public async Task PostAsync_Failure_ReturnsDefault()
    {
        SetupHttpResponse(HttpMethod.Post, "api/post", HttpStatusCode.BadRequest, "Error");

        var result = await _apiClient.PostAsync<object>("api/post", new { });

        Assert.Null(result);
    }

    [Fact]
    public async Task PutAsync_Success_ReturnsTrue()
    {
        SetupHttpResponse(HttpMethod.Put, "api/put/1", HttpStatusCode.OK, "{}");

        var result = await _apiClient.PutAsync("api/put/1", new { Name = "Updated" });

        Assert.True(result);
    }

    [Fact]
    public async Task PutAsync_Failure_ReturnsFalse()
    {
        SetupHttpResponse(HttpMethod.Put, "api/put/1", HttpStatusCode.NotFound, "");

        var result = await _apiClient.PutAsync("api/put/1", new { });

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_Success_ReturnsTrue()
    {
        SetupHttpResponse(HttpMethod.Delete, "api/delete/1", HttpStatusCode.OK, "{}");

        var result = await _apiClient.DeleteAsync("api/delete/1");

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_Failure_ReturnsFalse()
    {
        SetupHttpResponse(HttpMethod.Delete, "api/delete/1", HttpStatusCode.InternalServerError, "");

        var result = await _apiClient.DeleteAsync("api/delete/1");

        Assert.False(result);
    }

    [Fact]
    public async Task DownloadPdfAsync_Success_ReturnsByteArray()
    {
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        SetupHttpResponseBytes(HttpMethod.Get, "api/pdf", HttpStatusCode.OK, pdfBytes);

        var result = await _apiClient.DownloadPdfAsync("api/pdf");

        Assert.NotNull(result);
        Assert.Equal(pdfBytes, result);
    }

    [Fact]
    public async Task DownloadPdfAsync_Failure_ReturnsNull()
    {
        SetupHttpResponse(HttpMethod.Get, "api/pdf", HttpStatusCode.NotFound, "");

        var result = await _apiClient.DownloadPdfAsync("api/pdf");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserIdAsync_WithValidResponse_ReturnsUserId()
    {
        var responseContent = JsonSerializer.Serialize(new
        {
            success = true,
            data = new { id = 42 }
        });
        SetupHttpResponse(HttpMethod.Get, "api/auth/me", HttpStatusCode.OK, responseContent);

        var userId = await _apiClient.GetUserIdAsync();

        Assert.Equal(42, userId);
    }

    [Fact]
    public async Task GetUserIdAsync_NoData_ReturnsZero()
    {
        var responseContent = JsonSerializer.Serialize(new { success = true, data = new { } });
        SetupHttpResponse(HttpMethod.Get, "api/auth/me", HttpStatusCode.OK, responseContent);

        var userId = await _apiClient.GetUserIdAsync();

        Assert.Equal(0, userId);
    }

    [Fact]
    public async Task GetUserIdAsync_HttpError_ReturnsZero()
    {
        SetupHttpResponse(HttpMethod.Get, "api/auth/me", HttpStatusCode.Unauthorized, "");

        var userId = await _apiClient.GetUserIdAsync();

        Assert.Equal(0, userId);
    }

    [Fact]
    public async Task GetAsync_ShouldUseAuthToken_WhenLoggedIn()
    {
        // تنظیم توکن در session
        var token = "Basic testtoken";
        _testSession.SetString("AuthToken", token);

        var responseData = new { Id = 99 };
        var apiResponse = new { success = true, data = responseData };
        var json = JsonSerializer.Serialize(apiResponse);
        SetupHttpResponseWithAuth(HttpMethod.Get, "api/secure", HttpStatusCode.OK, json, token);

        var result = await _apiClient.GetAsync<dynamic>("api/secure");

        Assert.NotNull(result);
        var resultStr = JsonSerializer.Serialize(result);
        Assert.Contains("99", resultStr);
    }
}
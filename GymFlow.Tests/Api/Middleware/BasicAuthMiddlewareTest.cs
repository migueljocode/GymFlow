namespace GymFlow.Tests.Api.Middleware;

public class BasicAuthMiddlewareTest
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly BasicAuthMiddleware _middleware;
    private bool _nextCalled;

    public BasicAuthMiddlewareTest()
    {
        _mockAuthService = new Mock<IAuthService>();
        _nextCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            _nextCalled = true;
            return Task.CompletedTask;
        };
        _middleware = new BasicAuthMiddleware(next);
    }

    private HttpContext CreateHttpContext(string path, string? authHeader = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        if (authHeader != null)
            context.Request.Headers["Authorization"] = authHeader;
        return context;
    }

    private string EncodeCredentials(string username, string password)
    {
        var credentials = $"{username}:{password}";
        return $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials))}";
    }

    [Fact]
    public async Task InvokeAsync_NonProtectedPath_ShouldCallNextWithoutAuth()
    {
        var context = CreateHttpContext("/api/users");
        await _middleware.InvokeAsync(context, _mockAuthService.Object);
        Assert.True(_nextCalled);
        _mockAuthService.Verify(s => s.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("/api/workoutplans")]
    [InlineData("/api/workoutdays")]
    [InlineData("/api/workoutsessions")]
    [InlineData("/api/progress")]
    [InlineData("/api/statistics")]
    [InlineData("/api/predictions")]
    [InlineData("/api/workoutplans/123")]
    [InlineData("/api/workoutdays/abc")]
    public async Task InvokeAsync_ProtectedPath_ShouldRequireAuth(string protectedPath)
    {
        var context = CreateHttpContext(protectedPath);
        await _middleware.InvokeAsync(context, _mockAuthService.Object);
        Assert.False(_nextCalled);
        Assert.Equal(401, context.Response.StatusCode);
        Assert.Contains("Basic", context.Response.Headers["WWW-Authenticate"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_NoAuthorizationHeader_ShouldReturnUnauthorized()
    {
        var context = CreateHttpContext("/api/workoutplans");
        await _middleware.InvokeAsync(context, _mockAuthService.Object);
        Assert.Equal(401, context.Response.StatusCode);
        Assert.Contains("Basic", context.Response.Headers["WWW-Authenticate"].ToString());
        Assert.False(_nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_InvalidAuthorizationHeader_NotBasic_ShouldReturnUnauthorized()
    {
        var context = CreateHttpContext("/api/workoutplans", "Bearer token");
        await _middleware.InvokeAsync(context, _mockAuthService.Object);
        Assert.Equal(401, context.Response.StatusCode);
        Assert.Contains("Basic", context.Response.Headers["WWW-Authenticate"].ToString());
        Assert.False(_nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_MalformedBase64_ShouldReturnUnauthorized()
    {
        var context = CreateHttpContext("/api/workoutplans", "Basic invalid-base64!");
        await _middleware.InvokeAsync(context, _mockAuthService.Object);
        Assert.Equal(401, context.Response.StatusCode);
        Assert.False(_nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_InvalidCredentials_ShouldReturnUnauthorized()
    {
        var credentials = EncodeCredentials("wrong", "pass");
        var context = CreateHttpContext("/api/workoutplans", credentials);
        _mockAuthService.Setup(s => s.AuthenticateAsync("wrong", "pass"))
            .ReturnsAsync((User?)null);

        await _middleware.InvokeAsync(context, _mockAuthService.Object);
        Assert.Equal(401, context.Response.StatusCode);
        Assert.False(_nextCalled);
        _mockAuthService.Verify(s => s.AuthenticateAsync("wrong", "pass"), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ValidCredentials_ShouldSetItemsAndCallNext()
    {
        var user = new User
        {
            Id = 42,
            Person = new Person { Username = "testuser" }
        };
        var credentials = EncodeCredentials("testuser", "secret");
        var context = CreateHttpContext("/api/workoutplans", credentials);
        _mockAuthService.Setup(s => s.AuthenticateAsync("testuser", "secret"))
            .ReturnsAsync(user);

        await _middleware.InvokeAsync(context, _mockAuthService.Object);
        Assert.True(_nextCalled);
        Assert.Equal(user, context.Items["User"]);
        Assert.Equal(42, context.Items["UserId"]);
        Assert.Equal("testuser", context.Items["Username"]);
        Assert.Equal("Member", context.Items["UserRole"]);
    }

    [Fact]
    public async Task InvokeAsync_CoachCredentials_ShouldSetRoleCoach()
    {
        var user = new User
        {
            Id = 1,
            Person = new Person { Username = "coach" }
        };
        var credentials = EncodeCredentials("coach", "coach123");
        var context = CreateHttpContext("/api/workoutplans", credentials);
        _mockAuthService.Setup(s => s.AuthenticateAsync("coach", "coach123"))
            .ReturnsAsync(user);

        await _middleware.InvokeAsync(context, _mockAuthService.Object);
        Assert.Equal("Coach", context.Items["UserRole"]);
        Assert.True(_nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_UsernameFromPerson_ShouldUsePersonUsername()
    {
        var user = new User
        {
            Id = 10,
            Person = new Person { Username = "customusername" }
        };
        var credentials = EncodeCredentials("loginuser", "pass");
        var context = CreateHttpContext("/api/workoutplans", credentials);
        _mockAuthService.Setup(s => s.AuthenticateAsync("loginuser", "pass"))
            .ReturnsAsync(user);

        await _middleware.InvokeAsync(context, _mockAuthService.Object);
        Assert.Equal("customusername", context.Items["Username"]);
        Assert.True(_nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_UserWithoutPerson_ShouldUseProvidedUsername()
    {
        var user = new User
        {
            Id = 5,
            Person = null!
        };
        var credentials = EncodeCredentials("nopersonuser", "pass");
        var context = CreateHttpContext("/api/workoutplans", credentials);
        _mockAuthService.Setup(s => s.AuthenticateAsync("nopersonuser", "pass"))
            .ReturnsAsync(user);

        await _middleware.InvokeAsync(context, _mockAuthService.Object);
        Assert.Equal("nopersonuser", context.Items["Username"]);
        Assert.True(_nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_ExceptionInAuth_ShouldReturnUnauthorized()
    {
        var credentials = EncodeCredentials("user", "pass");
        var context = CreateHttpContext("/api/workoutplans", credentials);
        _mockAuthService.Setup(s => s.AuthenticateAsync("user", "pass"))
            .ThrowsAsync(new Exception("DB error"));

        await _middleware.InvokeAsync(context, _mockAuthService.Object);
        Assert.Equal(401, context.Response.StatusCode);
        Assert.False(_nextCalled);
    }

    [Theory]
    [InlineData("/API/WORKOUTPLANS")]
    [InlineData("/Api/WorkoutDays")]
    [InlineData("/API/WORKOUTSESSIONS")]
    [InlineData("/Api/Progress")]
    [InlineData("/API/STATISTICS")]
    public async Task InvokeAsync_CaseInsensitivePath_ShouldBeProtected(string mixedCasePath)
    {
        var context = CreateHttpContext(mixedCasePath);
        await _middleware.InvokeAsync(context, _mockAuthService.Object);
        Assert.Equal(401, context.Response.StatusCode);
        Assert.False(_nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_ValidCredentials_WithProtectedPath_ShouldAllowAndCallNext()
    {
        var user = new User { Id = 7, Person = new Person { Username = "member" } };
        var credentials = EncodeCredentials("member", "member123");
        var context = CreateHttpContext("/api/workoutplans", credentials);
        _mockAuthService.Setup(s => s.AuthenticateAsync("member", "member123"))
            .ReturnsAsync(user);

        await _middleware.InvokeAsync(context, _mockAuthService.Object);
        Assert.True(_nextCalled);
        // Status code default is 200 after next is called
    }
}
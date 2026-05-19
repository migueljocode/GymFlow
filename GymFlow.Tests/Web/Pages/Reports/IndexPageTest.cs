#nullable disable
using IndexModel = GymFlow.Web.Pages.Reports.IndexModel;

namespace GymFlow.Tests.Web.Pages.Reports;

// ================== Stub ApiClient ==================
public class StubApiClient : ApiClient
{
    private readonly Dictionary<string, object> _responses = new();
    private readonly Dictionary<string, byte[]> _pdfResponses = new();
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    public StubApiClient() : base(
        Mock.Of<IHttpClientFactory>(),
        Mock.Of<ILogger<ApiClient>>(),
        Mock.Of<IConfiguration>(),
        Mock.Of<IHttpContextAccessor>())
    { }

    public void AddResponse<T>(string url, T data)
    {
        _responses[url] = data;
    }

    public void AddPdfResponse(string url, byte[] data)
    {
        _pdfResponses[url] = data;
    }

    public override async Task<T> GetAsync<T>(string url)
    {
        if (_responses.TryGetValue(url, out var response))
        {
            // تبدیل با JsonSerializer برای تطابق نوع‌ها
            var json = JsonSerializer.Serialize(response, _jsonOptions);
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        return default;
    }

    public override async Task<byte[]> DownloadPdfAsync(string url)
    {
        if (_pdfResponses.TryGetValue(url, out var pdf))
            return pdf;
        return null;
    }
}

// ================== DTOها (برای استفاده در تست - دقیقاً مشابه کنترلر اصلی) ==================
// این DTOها باید با نوع‌های واقعی در کنترلر یکی باشند، اما چون در کنترلر public نیستند،
// آن‌ها را بازنویسی می‌کنیم. با JsonSerializer تطابق داده می‌شود.
public class ActivePlanDto
{
    public int Id { get; set; }
    public int Phase { get; set; }
    public bool IsActive { get; set; }
}

public class ClientSummaryDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
}

// ================== Test Class ==================
public class IndexPageTest : PageModelTestFixture
{
    private readonly StubApiClient _apiClient;
    private readonly IndexModel _pageModel;

    public IndexPageTest()
    {
        _apiClient = new StubApiClient();
        _pageModel = CreatePageModel<IndexModel>(_apiClient);
    }

    #region OnGetAsync

    [Fact]
    public async Task OnGetAsync_WhenUserNotLoggedIn_RedirectsToLogin()
    {
        var result = await _pageModel.OnGetAsync(null);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Login", redirectResult.PageName);
    }

    [Fact]
    public async Task OnGetAsync_WhenUserLoggedIn_NoActivePlan_SetsUserIdAndZeroPlanId()
    {
        SetAuthenticatedUser(_pageModel, userId: 5, username: "testuser");

        var result = await _pageModel.OnGetAsync(null);
        Assert.IsType<PageResult>(result);
        Assert.Equal(5, _pageModel.CurrentUserId);
        Assert.Equal(0, _pageModel.ActivePlanId);
    }

    [Fact]
    public async Task OnGetAsync_WhenUserLoggedIn_HasActivePlan_SetsUserIdAndPlanId()
    {
        SetAuthenticatedUser(_pageModel, userId: 7, username: "testuser");
        var activePlan = new ActivePlanDto { Id = 42, Phase = 2, IsActive = true };
        _apiClient.AddResponse($"api/workoutplans/user/7/active", activePlan);

        var result = await _pageModel.OnGetAsync(null);
        Assert.IsType<PageResult>(result);
        Assert.Equal(7, _pageModel.CurrentUserId);
        Assert.Equal(42, _pageModel.ActivePlanId);
    }

    [Fact]
    public async Task OnGetAsync_WhenCoach_WithClientId_SetsSelectedClient()
    {
        SetAuthenticatedUser(_pageModel, userId: 2, username: "coach", role: "Coach");

        var clients = new List<ClientSummaryDto>
        {
            new() { Id = 10, FullName = "John Doe" },
            new() { Id = 20, FullName = "Jane Smith" }
        };
        _apiClient.AddResponse($"api/coaches/2/clients", clients);

        var activePlan = new ActivePlanDto { Id = 99, Phase = 1, IsActive = true };
        _apiClient.AddResponse($"api/workoutplans/user/10/active", activePlan);

        var result = await _pageModel.OnGetAsync(10);
        Assert.IsType<PageResult>(result);
        Assert.Equal(10, _pageModel.SelectedClientId);
        Assert.Equal(99, _pageModel.ActivePlanId);
        Assert.Equal(2, _pageModel.Clients.Count);
    }

    [Fact]
    public async Task OnGetAsync_WhenCoach_NoClientId_SelectsFirstClient()
    {
        SetAuthenticatedUser(_pageModel, userId: 2, username: "coach", role: "Coach");

        var clients = new List<ClientSummaryDto>
        {
            new() { Id = 10, FullName = "John Doe" },
            new() { Id = 20, FullName = "Jane Smith" }
        };
        _apiClient.AddResponse($"api/coaches/2/clients", clients);

        var activePlan = new ActivePlanDto { Id = 99, Phase = 1, IsActive = true };
        _apiClient.AddResponse($"api/workoutplans/user/10/active", activePlan);

        var result = await _pageModel.OnGetAsync(null);
        Assert.IsType<PageResult>(result);
        Assert.Equal(10, _pageModel.SelectedClientId);
        Assert.Equal(99, _pageModel.ActivePlanId);
        Assert.Equal(2, _pageModel.Clients.Count);
    }

    #endregion

    #region OnPostDownloadWorkoutPlanAsync

    [Fact]
    public async Task OnPostDownloadWorkoutPlanAsync_WhenPdfExists_ReturnsFileResult()
    {
        var planId = 10;
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        _apiClient.AddPdfResponse($"api/export/workout-plan/{planId}", pdfBytes);

        var result = await _pageModel.OnPostDownloadWorkoutPlanAsync(planId, userId: 1);
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal($"WorkoutPlan_{planId}.pdf", fileResult.FileDownloadName);
        Assert.Equal(pdfBytes, fileResult.FileContents);
    }

    [Fact]
    public async Task OnPostDownloadWorkoutPlanAsync_WhenPdfMissing_ReturnsNotFound()
    {
        var planId = 99;
        var result = await _pageModel.OnPostDownloadWorkoutPlanAsync(planId, userId: 1);
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region OnPostDownloadProgressAsync

    [Fact]
    public async Task OnPostDownloadProgressAsync_Success_ReturnsFileResult()
    {
        int userId = 5;
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        _apiClient.AddPdfResponse($"api/export/progress/{userId}", pdfBytes);

        var result = await _pageModel.OnPostDownloadProgressAsync(userId);
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal($"ProgressReport_User_{userId}.pdf", fileResult.FileDownloadName);
        Assert.Equal(pdfBytes, fileResult.FileContents);
    }

    [Fact]
    public async Task OnPostDownloadProgressAsync_Failure_ReturnsNotFound()
    {
        int userId = 99;
        var result = await _pageModel.OnPostDownloadProgressAsync(userId);
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region OnPostDownloadCertificateAsync

    [Fact]
    public async Task OnPostDownloadCertificateAsync_Success_ReturnsFileResult()
    {
        int userId = 3;
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        _apiClient.AddPdfResponse($"api/export/certificate/{userId}", pdfBytes);

        var result = await _pageModel.OnPostDownloadCertificateAsync(userId);
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal($"Certificate_User_{userId}.pdf", fileResult.FileDownloadName);
        Assert.Equal(pdfBytes, fileResult.FileContents);
    }

    [Fact]
    public async Task OnPostDownloadCertificateAsync_Failure_ReturnsNotFound()
    {
        int userId = 99;
        var result = await _pageModel.OnPostDownloadCertificateAsync(userId);
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region OnPostDownloadWeeklySummaryAsync

    [Fact]
    public async Task OnPostDownloadWeeklySummaryAsync_Success_ReturnsFileResult()
    {
        int userId = 7;
        string weekStart = "2024-01-01";
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        _apiClient.AddPdfResponse($"api/export/weekly-summary/{userId}?weekStart={weekStart}", pdfBytes);

        var result = await _pageModel.OnPostDownloadWeeklySummaryAsync(userId, weekStart);
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal($"WeeklySummary_User_{userId}.pdf", fileResult.FileDownloadName);
        Assert.Equal(pdfBytes, fileResult.FileContents);
    }

    [Fact]
    public async Task OnPostDownloadWeeklySummaryAsync_Failure_ReturnsNotFound()
    {
        int userId = 99;
        string weekStart = "2024-01-01";
        var result = await _pageModel.OnPostDownloadWeeklySummaryAsync(userId, weekStart);
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion
}
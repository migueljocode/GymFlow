#nullable disable

using GymFlow.Tests.Web.Pages.TestBase;
using GymFlow.Web.Pages.Reports;
using GymFlow.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymFlow.Tests.Web.Pages.Reports;

public class IndexPageTest : PageModelTestFixture
{
    private readonly Mock<ApiClient> _mockApiClient;
    private readonly IndexModel _pageModel;

    public IndexPageTest()
    {
        _mockApiClient = new Mock<ApiClient>(
            Mock.Of<IHttpClientFactory>(),
            Mock.Of<ILogger<ApiClient>>(),
            Mock.Of<IConfiguration>(),
            Mock.Of<IHttpContextAccessor>())
        { CallBase = true };

        _pageModel = CreatePageModel<IndexModel>(_mockApiClient.Object);
    }

    #region OnGetAsync

    [Fact]
    public async Task OnGetAsync_WhenUserNotLoggedIn_RedirectsToLogin()
    {
        // بدون تنظیم Session

        var result = await _pageModel.OnGetAsync();

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Login", redirectResult.PageName);
    }

    [Fact]
    public async Task OnGetAsync_WhenUserLoggedIn_NoActivePlan_SetsUserIdAndZeroPlanId()
    {
        SetAuthenticatedUser(_pageModel, userId: 5, username: "testuser");
        _mockApiClient.Setup(c => c.GetAsync<ActivePlanDto>("api/workoutplans/user/5/active"))
            .ReturnsAsync((ActivePlanDto)null);

        await _pageModel.OnGetAsync();

        Assert.Equal(5, _pageModel.CurrentUserId);
        Assert.Equal(0, _pageModel.ActivePlanId);
    }

    [Fact]
    public async Task OnGetAsync_WhenUserLoggedIn_HasActivePlan_SetsUserIdAndPlanId()
    {
        SetAuthenticatedUser(_pageModel, userId: 7, username: "testuser");
        var activePlan = new ActivePlanDto { Id = 42, Phase = 2, IsActive = true };
        _mockApiClient.Setup(c => c.GetAsync<ActivePlanDto>("api/workoutplans/user/7/active"))
            .ReturnsAsync(activePlan);

        await _pageModel.OnGetAsync();

        Assert.Equal(7, _pageModel.CurrentUserId);
        Assert.Equal(42, _pageModel.ActivePlanId);
    }

    #endregion

    #region OnPostDownloadWorkoutPlanAsync

    [Fact]
    public async Task OnPostDownloadWorkoutPlanAsync_WhenPdfExists_ReturnsFileResult()
    {
        var planId = 10;
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        _mockApiClient.Setup(c => c.DownloadPdfAsync($"api/export/workout-plan/{planId}"))
            .ReturnsAsync(pdfBytes);

        var result = await _pageModel.OnPostDownloadWorkoutPlanAsync(planId);

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal($"WorkoutPlan_{planId}.pdf", fileResult.FileDownloadName);
        Assert.Equal(pdfBytes, fileResult.FileContents);
    }

    [Fact]
    public async Task OnPostDownloadWorkoutPlanAsync_WhenPdfMissing_ReturnsNotFound()
    {
        var planId = 99;
        _mockApiClient.Setup(c => c.DownloadPdfAsync($"api/export/workout-plan/{planId}"))
            .ReturnsAsync((byte[])null);

        var result = await _pageModel.OnPostDownloadWorkoutPlanAsync(planId);

        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region OnPostDownloadProgressAsync

    [Fact]
    public async Task OnPostDownloadProgressAsync_Success_ReturnsFileResult()
    {
        int userId = 5;
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        _mockApiClient.Setup(c => c.DownloadPdfAsync($"api/export/progress/{userId}"))
            .ReturnsAsync(pdfBytes);

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
        _mockApiClient.Setup(c => c.DownloadPdfAsync($"api/export/progress/{userId}"))
            .ReturnsAsync((byte[])null);

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
        _mockApiClient.Setup(c => c.DownloadPdfAsync($"api/export/certificate/{userId}"))
            .ReturnsAsync(pdfBytes);

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
        _mockApiClient.Setup(c => c.DownloadPdfAsync($"api/export/certificate/{userId}"))
            .ReturnsAsync((byte[])null);

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
        _mockApiClient.Setup(c => c.DownloadPdfAsync($"api/export/weekly-summary/{userId}?weekStart={weekStart}"))
            .ReturnsAsync(pdfBytes);

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
        _mockApiClient.Setup(c => c.DownloadPdfAsync($"api/export/weekly-summary/{userId}?weekStart={weekStart}"))
            .ReturnsAsync((byte[])null);

        var result = await _pageModel.OnPostDownloadWeeklySummaryAsync(userId, weekStart);

        Assert.IsType<NotFoundResult>(result);
    }

    #endregion
}
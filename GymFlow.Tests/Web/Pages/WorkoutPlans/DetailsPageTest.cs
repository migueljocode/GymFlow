#nullable disable

namespace GymFlow.Tests.Web.Pages.WorkoutPlans;

public class DetailsPageTest : PageModelTestFixture
{
    private readonly Mock<ApiClient> _mockApiClient;
    private readonly DetailsModel _pageModel;

    public DetailsPageTest()
    {
        _mockApiClient = new Mock<ApiClient>(
            Mock.Of<IHttpClientFactory>(),
            Mock.Of<ILogger<ApiClient>>(),
            Mock.Of<IConfiguration>(),
            Mock.Of<IHttpContextAccessor>())
        { CallBase = true };

        _pageModel = CreatePageModel<DetailsModel>(_mockApiClient.Object);
    }

    #region OnGetAsync

    [Fact]
    public async Task OnGetAsync_WhenPlanExists_SetsPlanAndCalculatesSessions()
    {
        // Arrange
        SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser", role: "Member");
        var planId = 5;
        var planDetails = new WorkoutPlanDetailsResponse
        {
            Id = planId,
            Phase = 2,
            SessionsPerWeek = 3,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            IsActive = true,
            WorkoutDays = new List<WorkoutDayDetailResponse>
            {
                new() { Id = 1, DayOfWeek = DayOfWeek.Monday, DurationMinutes = 60 },
                new() { Id = 2, DayOfWeek = DayOfWeek.Wednesday, DurationMinutes = 60 },
                new() { Id = 3, DayOfWeek = DayOfWeek.Friday, DurationMinutes = 60 }
            }
        };
        _mockApiClient.Setup(c => c.GetAsync<WorkoutPlanDetailsResponse>($"api/workoutplans/{planId}/details"))
            .ReturnsAsync(planDetails);
        var result = await _pageModel.OnGetAsync(planId);
    
        // Act
        await _pageModel.OnGetAsync(planId);

        // Assert
        Assert.NotNull(_pageModel.Plan);
        Assert.Equal(planId, _pageModel.Plan.Id);
        Assert.Equal(3, _pageModel.Plan.WorkoutDays.Count);
        Assert.Equal(3 * 4, _pageModel.TotalSessions); // 3 days * 4 weeks = 12
        Assert.Equal(0, _pageModel.CompletedSessions);
        Assert.Equal(0, _pageModel.CompletionPercentage); // CompletedSessions = 0
    }

    [Fact]
    public async Task OnGetAsync_WhenPlanHasNoWorkoutDays_SetsTotalSessionsZero()
    {
        // Arrange
        SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser", role: "Member");
        var planId = 5;
        var planDetails = new WorkoutPlanDetailsResponse
        {
            Id = planId,
            WorkoutDays = new List<WorkoutDayDetailResponse>() // خالی
        };
        _mockApiClient.Setup(c => c.GetAsync<WorkoutPlanDetailsResponse>($"api/workoutplans/{planId}/details"))
            .ReturnsAsync(planDetails);

        // Act
        await _pageModel.OnGetAsync(planId);

        // Assert
        Assert.NotNull(_pageModel.Plan);
        Assert.Equal(0, _pageModel.TotalSessions);
    }

    [Fact]
    public async Task OnGetAsync_WhenPlanNotFound_SetsPlanToNull()
    {
        // Arrange
        var planId = 99;
        _mockApiClient.Setup(c => c.GetAsync<WorkoutPlanDetailsResponse>($"api/workoutplans/{planId}/details"))
            .ReturnsAsync((WorkoutPlanDetailsResponse)null);

        // Act
        await _pageModel.OnGetAsync(planId);

        // Assert
        Assert.Null(_pageModel.Plan);
        Assert.Equal(0, _pageModel.TotalSessions);
        Assert.Equal(0, _pageModel.CompletionPercentage);
    }

    #endregion

    #region OnPostDownloadPdfAsync

    [Fact]
    public async Task OnPostDownloadPdfAsync_WhenPdfExists_ReturnsFileResult()
    {
        // Arrange
        var planId = 10;
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF
        _mockApiClient.Setup(c => c.DownloadPdfAsync($"api/export/workout-plan/{planId}"))
            .ReturnsAsync(pdfBytes);

        // Act
        var result = await _pageModel.OnPostDownloadPdfAsync(planId);

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal($"WorkoutPlan_{planId}.pdf", fileResult.FileDownloadName);
        Assert.Equal(pdfBytes, fileResult.FileContents);
    }

    [Fact]
    public async Task OnPostDownloadPdfAsync_WhenPdfNotFound_ReturnsNotFound()
    {
        // Arrange
        var planId = 99;
        _mockApiClient.Setup(c => c.DownloadPdfAsync($"api/export/workout-plan/{planId}"))
            .ReturnsAsync((byte[])null);

        // Act
        var result = await _pageModel.OnPostDownloadPdfAsync(planId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion
}
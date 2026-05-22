#nullable disable

namespace GymFlow.Tests.Web.Pages.WorkoutPlans;

public class CreatePageTest : PageModelTestFixture
{
    private readonly Mock<ApiClient> _mockApiClient;
    private readonly CreateModel _pageModel;

    public CreatePageTest()
    {
        _mockApiClient = new Mock<ApiClient>(
            Mock.Of<IHttpClientFactory>(),
            Mock.Of<ILogger<ApiClient>>(),
            Mock.Of<IConfiguration>(),
            Mock.Of<IHttpContextAccessor>())
        { CallBase = true };

        _pageModel = CreatePageModel<CreateModel>(_mockApiClient.Object);
    }

    #region OnGetAsync

    [Fact]
    public async Task OnGetAsync_WhenUserNotLoggedIn_ReturnsPageResult()
    {
        // Arrange - بدون تنظیم Session (userId وجود ندارد)
        // در پیاده‌سازی فعلی، OnGetAsync ریدایرکت نمی‌کند و فقط صفحه را برمی‌گرداند.

        // Act
        await _pageModel.OnGetAsync();

        // Assert - بررسی می‌کنیم که ریدایرکتی رخ نداده
        Assert.Null(_pageModel.Response.Headers["Location"].FirstOrDefault());
    }

    [Fact]
    public async Task OnGetAsync_WhenUserLoggedIn_NoPreviousPlans_PhaseRemainsOne()
    {
        // Arrange
        SetAuthenticatedUser(_pageModel, 1, "testuser");
        _mockApiClient.Setup(c => c.GetAsync<List<WorkoutPlanListResponse>>("api/workoutplans/user/1"))
            .ReturnsAsync((List<WorkoutPlanListResponse>)null);

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        Assert.Equal(1, _pageModel.Phase);
    }

    [Fact]
    public async Task OnGetAsync_WhenUserLoggedIn_HasPreviousPlans_PhaseSetToMaxPlusOne()
    {
        // Arrange
        SetAuthenticatedUser(_pageModel, userId: 1, username: "coach", role: "Coach");
        var clientId = 1;
        _pageModel.ClientId = clientId;

        var existingPlans = new List<WorkoutPlanListResponse>
        {
            new() { Id = 1, Phase = 1, IsActive = false },
            new() { Id = 2, Phase = 2, IsActive = false },
            new() { Id = 3, Phase = 3, IsActive = true }
        };
        _mockApiClient.Setup(c => c.GetAsync<List<WorkoutPlanListResponse>>($"api/workoutplans/user/{clientId}"))
            .ReturnsAsync(existingPlans);

        // Act
        await _pageModel.OnGetAsync(clientId);

        // Assert
        Assert.Equal(4, _pageModel.Phase); // ماکزیمم فاز قبلی (3) + 1
    }

    #endregion

    #region OnPostAsync - Validation and Redirects

    [Fact]
    public async Task OnPostAsync_InvalidModelState_ReturnsPageWithError()
    {
        // Arrange
        SetAuthenticatedUser(_pageModel, userId: 1, username: "coach", role: "Coach");
        _pageModel.ModelState.AddModelError("SessionsPerWeek", "Required");
        _pageModel.SelectedDays = new List<int> { 6 }; // روز انتخاب شده باشد تا به دلیل خطای مدل استیت ریجکت شود

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert: باید PageResult باشد
        Assert.IsType<PageResult>(result);
        Assert.Equal("اطلاعات وارد شده معتبر نیست", _pageModel.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_NoSelectedDays_ReturnsPageWithError()
    {
        // Arrange
        SetAuthenticatedUser(_pageModel, userId: 1, username: "coach", role: "Coach");
        _pageModel.SelectedDays = new List<int>(); // هیچ روزی انتخاب نشده

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert: باید PageResult باشد (همان صفحه با خطا)
        Assert.IsType<PageResult>(result);
        Assert.Equal("حداقل یک روز تمرینی را انتخاب کنید", _pageModel.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_UserNotLoggedIn_RedirectsToLogin()
    {
        // Arrange
        _pageModel.SelectedDays = new List<int> { 6 };
        // بدون تنظیم Session

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Login", redirectResult.PageName);
    }

    [Fact]
    public async Task OnPostAsync_PhaseAlreadyExists_ReturnsPageWithError()
    {
        // Arrange
        SetAuthenticatedUser(_pageModel, userId: 1, username: "coach", role: "Coach");
        _pageModel.Phase = 2;
        _pageModel.SelectedDays = new List<int> { 6 };

        var existingPlans = new List<WorkoutPlanListResponse>
        {
            new() { Id = 1, Phase = 1, IsActive = false },
            new() { Id = 2, Phase = 2, IsActive = true } // فاز ۲ قبلاً وجود دارد
        };
        _mockApiClient.Setup(c => c.GetAsync<List<WorkoutPlanListResponse>>(It.IsAny<string>()))
            .ReturnsAsync(existingPlans);

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert: باید PageResult باشد
        Assert.IsType<PageResult>(result);
        Assert.Equal("فاز 2 قبلاً ایجاد شده است!", _pageModel.ErrorMessage);
    }

    #endregion

    #region OnPostAsync - Successful creation

    [Fact]
    public async Task OnPostAsync_SuccessfulCreation_RedirectsToAddExercises()
    {
        // Arrange
        SetAuthenticatedUser(_pageModel, 1, "testuser");
        _pageModel.Phase = 1;
        _pageModel.SessionsPerWeek = 4;
        _pageModel.StartDate = DateOnly.FromDateTime(DateTime.UtcNow);
        _pageModel.SelectedDays = new List<int> { 6, 0, 1 }; // شنبه، یکشنبه، دوشنبه

        var existingPlans = new List<WorkoutPlanListResponse>(); // هیچ برنامه قبلی
        _mockApiClient.Setup(c => c.GetAsync<List<WorkoutPlanListResponse>>("api/workoutplans/user/1"))
            .ReturnsAsync(existingPlans);

        var createdPlan = new WorkoutPlanResponse { Id = 5, Phase = 1 };
        _mockApiClient.Setup(c => c.PostAsync<WorkoutPlanResponse>("api/workoutplans", It.IsAny<CreateWorkoutPlanRequest>()))
            .ReturnsAsync(createdPlan);

        // موک کردن غیرفعال کردن برنامه فعال (وجود ندارد)
        _mockApiClient.Setup(c => c.PostAsync<object>($"api/workoutplans/0/deactivate", It.IsAny<object>()))
            .ReturnsAsync((object)null);

        // موک کردن ایجاد روزهای تمرینی
        _mockApiClient.Setup(c => c.PostAsync<object>("api/workoutdays", It.IsAny<object>()))
            .ReturnsAsync(new { });

        var workoutDays = new List<WorkoutDayResponse>
        {
            new() { Id = 10, DayOfWeek = DayOfWeek.Saturday }
        };
        _mockApiClient.Setup(c => c.GetAsync<List<WorkoutDayResponse>>($"api/workoutdays/plan/{createdPlan.Id}"))
            .ReturnsAsync(workoutDays);

        // Act
        SetAuthenticatedUser(_pageModel, userId: 1, username: "coach", role: "Coach");
        var result = await _pageModel.OnPostAsync();

        // Assert
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/WorkoutPlans/AddExercises", redirectResult.PageName);
        Assert.Equal(10, redirectResult.RouteValues["workoutDayId"]);
        Assert.Equal(5, redirectResult.RouteValues["workoutPlanId"]);
        Assert.Equal("Saturday", redirectResult.RouteValues["dayOfWeek"]);

        // بررسی اینکه تعداد دفعات فراخوانی PostAsync برای workoutdays برابر تعداد روزهای انتخاب شده است
        _mockApiClient.Verify(c => c.PostAsync<object>("api/workoutdays", It.IsAny<object>()), Times.Exactly(_pageModel.SelectedDays.Count));
    }

    [Fact]
    public async Task OnPostAsync_WhenActivePlanExists_DeactivatesItFirst()
    {
        // Arrange
        SetAuthenticatedUser(_pageModel, userId: 1, username: "coach", role: "Coach");
        var clientId = 5;
        _pageModel.ClientId = clientId;
        _pageModel.SelectedDays = new List<int> { 6 };
        _pageModel.Phase = 2;
        _pageModel.SessionsPerWeek = 3;
        _pageModel.StartDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var existingPlans = new List<WorkoutPlanListResponse>
        {
            new() { Id = 10, IsActive = true, Phase = 1 }
        };
        _mockApiClient.Setup(c => c.GetAsync<List<WorkoutPlanListResponse>>($"api/workoutplans/user/{clientId}"))
            .ReturnsAsync(existingPlans);

        var createdPlan = new WorkoutPlanResponse { Id = 15, Phase = 2 };
        _mockApiClient.Setup(c => c.PostAsync<WorkoutPlanResponse>("api/workoutplans", It.IsAny<CreateWorkoutPlanRequest>()))
            .ReturnsAsync(createdPlan);

        // مهم: موک غیرفعال کردن پلن فعال
        _mockApiClient.Setup(c => c.PostAsync<object>($"api/workoutplans/10/deactivate", It.IsAny<object>()))
            .ReturnsAsync(new { });

        // موک ایجاد روزهای تمرینی
        _mockApiClient.Setup(c => c.PostAsync<object>("api/workoutdays", It.IsAny<CreateWorkoutDayRequest>()))
            .ReturnsAsync(new { });

        // موک برگرداندن یک روز تمرینی برای ریدایرکت به AddExercises
        var workoutDays = new List<WorkoutDayResponse>
        {
            new() { Id = 99, DayOfWeek = DayOfWeek.Monday }
        };
        _mockApiClient.Setup(c => c.GetAsync<List<WorkoutDayResponse>>($"api/workoutdays/plan/{createdPlan.Id}"))
            .ReturnsAsync(workoutDays);

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        _mockApiClient.Verify(c => c.PostAsync<object>("api/workoutplans/10/deactivate", It.IsAny<object>()), Times.Once);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/WorkoutPlans/AddExercises", redirectResult.PageName);
        Assert.Equal(99, redirectResult.RouteValues["workoutDayId"]);
        Assert.Equal(15, redirectResult.RouteValues["workoutPlanId"]);
        Assert.Equal("Monday", redirectResult.RouteValues["dayOfWeek"]);
    }

    [Fact]
    public async Task OnPostAsync_CreatePlanFails_ReturnsPageWithError()
    {
        // Arrange
        SetAuthenticatedUser(_pageModel, userId: 1, username: "coach", role: "Coach");
        _pageModel.SelectedDays = new List<int> { 6 };
        _pageModel.Phase = 1;
        _pageModel.SessionsPerWeek = 3;
        _pageModel.StartDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // شبیه‌سازی شکست ایجاد پلن (API null برمی‌گرداند)
        _mockApiClient.Setup(c => c.GetAsync<List<WorkoutPlanListResponse>>(It.IsAny<string>()))
            .ReturnsAsync(new List<WorkoutPlanListResponse>());
        _mockApiClient.Setup(c => c.PostAsync<WorkoutPlanResponse>("api/workoutplans", It.IsAny<CreateWorkoutPlanRequest>()))
            .ReturnsAsync((WorkoutPlanResponse)null);

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert: باید PageResult باشد (همان صفحه با خطا)
        var pageResult = Assert.IsType<PageResult>(result);
        Assert.Equal("خطا در ایجاد برنامه تمرینی", _pageModel.ErrorMessage);
    }

    #endregion
}
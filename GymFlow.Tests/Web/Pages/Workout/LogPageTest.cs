    #nullable disable

    using GymFlow.Tests.Web.Pages.TestBase;
    using GymFlow.Web.Pages.Workout;
    using GymFlow.Web.Services;
    using GymFlow.Models.DTOs.Requests;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;

    namespace GymFlow.Tests.Web.Pages.Workout;

    public class LogPageTest : PageModelTestFixture
    {
        private readonly Mock<ApiClient> _mockApiClient;
        private readonly LogModel _pageModel;

        public LogPageTest()
        {
            _mockApiClient = new Mock<ApiClient>(
                Mock.Of<IHttpClientFactory>(),
                Mock.Of<ILogger<ApiClient>>(),
                Mock.Of<IConfiguration>(),
                Mock.Of<IHttpContextAccessor>())
            { CallBase = true };

            _pageModel = CreatePageModel<LogModel>(_mockApiClient.Object);
        }

        #region OnPostAsync

        [Fact]
        public async Task OnPostAsync_WhenUserNotLoggedIn_RedirectsToLogin()
        {
            // بدون تنظیم Session

            var result = await _pageModel.OnPostAsync();

            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/Login", redirectResult.PageName);
            Assert.Equal("لطفاً مجدداً وارد شوید.", _pageModel.ErrorMessage);
            _mockApiClient.Verify(c => c.GetAsync<ActivePlanDto>(It.IsAny<string>()), Times.Never);
        }
        
        [Fact]
        public async Task OnPostAsync_WhenNoActivePlan_ReturnsPageWithError()
        {
            SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser");
            _mockApiClient.Setup(c => c.GetAsync<ActivePlanDto>("api/workoutplans/user/1/active"))
                .ReturnsAsync((ActivePlanDto)null);

            var result = await _pageModel.OnPostAsync();

            Assert.IsType<PageResult>(result);
            Assert.Equal("❌ برنامه تمرینی فعالی ندارید! لطفاً ابتدا یک برنامه تمرینی ایجاد کنید.", _pageModel.ErrorMessage);
            Assert.Null(_pageModel.Message);
            _mockApiClient.Verify(c => c.GetAsync<List<WorkoutDayDto>>(It.IsAny<string>()), Times.Never);
            _mockApiClient.Verify(c => c.PostAsync<object>(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task OnPostAsync_WhenActivePlanExistsButNoWorkoutForToday_ReturnsPageWithError()
        {
            SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser");
            var activePlan = new ActivePlanDto { Id = 5, Phase = 1, IsActive = true };
            _mockApiClient.Setup(c => c.GetAsync<ActivePlanDto>("api/workoutplans/user/1/active"))
                .ReturnsAsync(activePlan);

            var workoutDays = new List<WorkoutDayDto>
            {
                new() { Id = 1, DayOfWeek = DayOfWeek.Monday },
                new() { Id = 2, DayOfWeek = DayOfWeek.Wednesday }
            };
            _mockApiClient.Setup(c => c.GetAsync<List<WorkoutDayDto>>($"api/workoutdays/plan/{activePlan.Id}"))
                .ReturnsAsync(workoutDays);

            _pageModel.ActualDate = new DateOnly(2024, 1, 9); // Tuesday

            var result = await _pageModel.OnPostAsync();

            Assert.IsType<PageResult>(result);
            Assert.Equal($"❌ برای روز Tuesday برنامه تمرینی ندارید!", _pageModel.ErrorMessage);
            _mockApiClient.Verify(c => c.PostAsync<object>(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task OnPostAsync_SuccessfulLog_ReturnsPageWithSuccessMessage()
        {
            SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser");
            var activePlan = new ActivePlanDto { Id = 5, Phase = 1, IsActive = true };
            _mockApiClient.Setup(c => c.GetAsync<ActivePlanDto>("api/workoutplans/user/1/active"))
                .ReturnsAsync(activePlan);

            var workoutDays = new List<WorkoutDayDto>
            {
                new() { Id = 1, DayOfWeek = DayOfWeek.Monday },
                new() { Id = 2, DayOfWeek = DayOfWeek.Tuesday },
                new() { Id = 3, DayOfWeek = DayOfWeek.Wednesday }
            };
            _mockApiClient.Setup(c => c.GetAsync<List<WorkoutDayDto>>($"api/workoutdays/plan/{activePlan.Id}"))
                .ReturnsAsync(workoutDays);

            _pageModel.ActualDate = new DateOnly(2024, 1, 9); // Tuesday
            _pageModel.DurationMinutes = 60;
            _pageModel.Feeling = "Great!";

            _mockApiClient.Setup(c => c.PostAsync<object>("api/workoutsessions/log", It.IsAny<LogWorkoutRequest>()))
                .ReturnsAsync(new { Id = 100 });

            var result = await _pageModel.OnPostAsync();

            Assert.IsType<PageResult>(result);
            Assert.Equal($"✅ تمرین برای تاریخ {_pageModel.ActualDate} با موفقیت ثبت شد! 🔥", _pageModel.Message);
            Assert.Null(_pageModel.ErrorMessage);
            _mockApiClient.Verify(c => c.PostAsync<object>("api/workoutsessions/log", It.Is<LogWorkoutRequest>(req =>
                req.WorkoutDayId == 2 &&
                req.ActualDate == _pageModel.ActualDate &&
                req.ActualDurationMinutes == 60 &&
                req.Feeling == "Great!"
            )), Times.Once);
        }

        [Fact]
        public async Task OnPostAsync_WhenConflict409_ReturnsPageWithConflictMessage()
        {
            SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser");
            var activePlan = new ActivePlanDto { Id = 5, Phase = 1, IsActive = true };
            _mockApiClient.Setup(c => c.GetAsync<ActivePlanDto>("api/workoutplans/user/1/active"))
                .ReturnsAsync(activePlan);

            var workoutDays = new List<WorkoutDayDto>
            {
                new() { Id = 1, DayOfWeek = DayOfWeek.Monday },
                new() { Id = 2, DayOfWeek = DayOfWeek.Tuesday }
            };
            _mockApiClient.Setup(c => c.GetAsync<List<WorkoutDayDto>>($"api/workoutdays/plan/{activePlan.Id}"))
                .ReturnsAsync(workoutDays);

            _pageModel.ActualDate = new DateOnly(2024, 1, 9); // Tuesday

            _mockApiClient.Setup(c => c.PostAsync<object>("api/workoutsessions/log", It.IsAny<LogWorkoutRequest>()))
                .ThrowsAsync(new Exception("Conflict 409 - Already logged"));

            var result = await _pageModel.OnPostAsync();

            Assert.IsType<PageResult>(result);
            Assert.Equal($"⚠️ تمرین برای تاریخ {_pageModel.ActualDate} قبلاً ثبت شده است! نمی‌توانید دوباره ثبت کنید.", _pageModel.ErrorMessage);
            Assert.Null(_pageModel.Message);
        }

        [Fact]
        public async Task OnPostAsync_WhenGeneralException_ReturnsPageWithGeneralError()
        {
            SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser");
            var activePlan = new ActivePlanDto { Id = 5, Phase = 1, IsActive = true };
            _mockApiClient.Setup(c => c.GetAsync<ActivePlanDto>("api/workoutplans/user/1/active"))
                .ReturnsAsync(activePlan);

            var workoutDays = new List<WorkoutDayDto>
            {
                new() { Id = 1, DayOfWeek = DayOfWeek.Monday },
                new() { Id = 2, DayOfWeek = DayOfWeek.Tuesday }
            };
            _mockApiClient.Setup(c => c.GetAsync<List<WorkoutDayDto>>($"api/workoutdays/plan/{activePlan.Id}"))
                .ReturnsAsync(workoutDays);

            _pageModel.ActualDate = new DateOnly(2024, 1, 9); // Tuesday

            _mockApiClient.Setup(c => c.PostAsync<object>("api/workoutsessions/log", It.IsAny<LogWorkoutRequest>()))
                .ThrowsAsync(new Exception("Internal server error"));

            var result = await _pageModel.OnPostAsync();

            Assert.IsType<PageResult>(result);
            Assert.Equal("❌ خطا در ثبت تمرین: Internal server error", _pageModel.ErrorMessage);
            Assert.Null(_pageModel.Message);
        }

        #endregion
    }
using GymFlow.Api.Controllers;
using GymFlow.Dal.Repositories.Interfaces;
using GymFlow.Models.DTOs.Requests;
using GymFlow.Models.DTOs.Responses;
using GymFlow.Models.Entities;
using GymFlow.Models.Enums;
using GymFlow.Tests.Api.Controllers.TestBase;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Text.Json;

namespace GymFlow.Tests.Api.Controllers;

public class WorkoutSessionsControllerTest : ControllerTestFixture
{
    private readonly Mock<IWorkoutSessionRepository> _mockSessionRepo;
    private readonly Mock<IWorkoutDayRepository> _mockWorkoutDayRepo;
    private readonly Mock<IWorkoutPlanRepository> _mockWorkoutPlanRepo;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly WorkoutSessionsController _controller;

    public WorkoutSessionsControllerTest()
    {
        _mockSessionRepo = new Mock<IWorkoutSessionRepository>();
        _mockWorkoutDayRepo = new Mock<IWorkoutDayRepository>();
        _mockWorkoutPlanRepo = new Mock<IWorkoutPlanRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _controller = CreateController<WorkoutSessionsController>(
            _mockSessionRepo.Object,
            _mockWorkoutDayRepo.Object,
            _mockWorkoutPlanRepo.Object,
            _mockUserRepo.Object);
    }

    #region Helper Methods

    private User CreateTestUser(int id = 1)
    {
        var person = new Person
        {
            Id = id,
            FirstName = "Test",
            LastName = $"User{id}",
            Username = $"user{id}",
            Email = $"user{id}@test.com",
            Gender = Gender.Male,
            Age = 30,
            Weight = 80f,
            Height = 180f,
            BodyType = BodyType.Fit,
            CreatedAt = DateTime.UtcNow.AddMonths(-3)
        };

        return new User
        {
            Id = id,
            PersonId = person.Id,
            Person = person,
            Goal = Goal.Fitness,
            CreatedAt = DateTime.UtcNow.AddMonths(-3)
        };
    }

    private WorkoutDay CreateTestWorkoutDay(int id = 1, int planId = 1, DayOfWeek day = DayOfWeek.Monday)
    {
        return new WorkoutDay
        {
            Id = id,
            WorkoutPlanId = planId,
            DayOfWeek = day,
            TargetMuscles = MuscleGroup.Chest,
            DurationMinutes = 60,
            Intensity = Intensity.Medium,
            Notes = "Test notes",
            CreatedAt = DateTime.UtcNow
        };
    }

    private WorkoutSession CreateTestWorkoutSession(int id = 1, int workoutDayId = 1, DateOnly? date = null, int duration = 60)
    {
        var workoutDay = CreateTestWorkoutDay(workoutDayId);
        return new WorkoutSession
        {
            Id = id,
            WorkoutDayId = workoutDayId,
            WorkoutDay = workoutDay,
            ActualDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow),
            ActualDurationMinutes = duration,
            Feeling = "Great!",
            CreatedAt = DateTime.UtcNow
        };
    }

    private WorkoutPlan CreateTestWorkoutPlan(int id = 1, int userId = 1, int sessionsPerWeek = 3)
    {
        return new WorkoutPlan
        {
            Id = id,
            UserId = userId,
            Phase = 1,
            SessionsPerWeek = sessionsPerWeek,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)),
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };
    }

    #endregion

    #region GetByUserAsync

    [Fact]
    public async Task GetByUserAsync_UserExists_ReturnsSessions()
    {
        // Arrange
        var user = CreateTestUser(1);
        var sessions = new List<WorkoutSession>
        {
            CreateTestWorkoutSession(1, 1, DateOnly.FromDateTime(DateTime.UtcNow), 60),
            CreateTestWorkoutSession(2, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), 55)
        };
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockSessionRepo.Setup(r => r.GetSessionsByUserAsync(1)).ReturnsAsync(sessions);

        // Act
        var result = await _controller.GetByUserAsync(1);

        // Assert
        var response = ParseSuccessResponse<IEnumerable<WorkoutSessionResponse>>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data.Count());
    }

    [Fact]
    public async Task GetByUserAsync_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetByUserAsync(99);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("User with identifier '99' not found", errorResponse.Error);
    }

    #endregion

    #region GetByDateRangeAsync

    [Fact]
    public async Task GetByDateRangeAsync_UserExists_ReturnsSessionsInRange()
    {
        // Arrange
        var user = CreateTestUser(1);
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var sessions = new List<WorkoutSession>
        {
            CreateTestWorkoutSession(1, 1, DateOnly.FromDateTime(DateTime.UtcNow), 60),
            CreateTestWorkoutSession(2, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)), 55)
        };
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockSessionRepo.Setup(r => r.GetSessionsByDateRangeAsync(1, startDate, endDate)).ReturnsAsync(sessions);

        // Act
        var result = await _controller.GetByDateRangeAsync(1, startDate, endDate);

        // Assert
        var response = ParseSuccessResponse<IEnumerable<WorkoutSessionResponse>>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data.Count());
    }

    [Fact]
    public async Task GetByDateRangeAsync_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetByDateRangeAsync(99, null, null);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("User with identifier '99' not found", errorResponse.Error);
    }

    #endregion

    #region GetWeeklySummaryAsync

    [Fact]
    public async Task GetWeeklySummaryAsync_UserExists_ReturnsSummary()
    {
        // Arrange
        var user = CreateTestUser(1);
        var activePlan = CreateTestWorkoutPlan(1, 1, 3);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        var sessions = new List<WorkoutSession>
        {
            CreateTestWorkoutSession(1, 1, startOfWeek, 60),
            CreateTestWorkoutSession(2, 1, startOfWeek.AddDays(2), 55)
        };

        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockSessionRepo.Setup(r => r.GetSessionsByDateRangeAsync(1, startOfWeek, startOfWeek.AddDays(6))).ReturnsAsync(sessions);
        _mockWorkoutPlanRepo.Setup(r => r.GetActiveWorkoutPlanAsync(1)).ReturnsAsync(activePlan);

        // Act
        var result = await _controller.GetWeeklySummaryAsync(1);

        // Assert
        var response = ParseSuccessResponse<WeeklySummaryResponse>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data.TotalSessions);
        Assert.Equal(115, response.Data.TotalDurationMinutes);
        Assert.Equal(57.5, response.Data.AverageDurationMinutes, 1);
        Assert.Equal(66, response.Data.CompletedPlanPercentage); // 2/3 = 66%
    }

    [Fact]
    public async Task GetWeeklySummaryAsync_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetWeeklySummaryAsync(99);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("User with identifier '99' not found", errorResponse.Error);
    }

    [Fact]
    public async Task GetWeeklySummaryAsync_NoActivePlan_UsesDefaultThreeSessions()
    {
        // Arrange
        var user = CreateTestUser(1);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        var sessions = new List<WorkoutSession>
        {
            CreateTestWorkoutSession(1, 1, startOfWeek, 60)
        };

        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockSessionRepo.Setup(r => r.GetSessionsByDateRangeAsync(1, startOfWeek, startOfWeek.AddDays(6))).ReturnsAsync(sessions);
        _mockWorkoutPlanRepo.Setup(r => r.GetActiveWorkoutPlanAsync(1)).ReturnsAsync((WorkoutPlan?)null);

        // Act
        var result = await _controller.GetWeeklySummaryAsync(1);

        // Assert
        var response = ParseSuccessResponse<WeeklySummaryResponse>(result);
        Assert.NotNull(response.Data);
        Assert.Equal(33, response.Data.CompletedPlanPercentage); // 1/3 = 33%
    }

    #endregion

    #region LogWorkoutAsync

    [Fact]
    public async Task LogWorkoutAsync_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var workoutDay = CreateTestWorkoutDay(1, 1, DayOfWeek.Monday);
        var request = new LogWorkoutRequest
        {
            WorkoutDayId = 1,
            ActualDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ActualDurationMinutes = 65,
            Feeling = "Amazing!"
        };
        var createdSession = CreateTestWorkoutSession(10, 1, request.ActualDate, 65);
        createdSession.Feeling = "Amazing!";

        _mockWorkoutDayRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(workoutDay);
        _mockSessionRepo.Setup(r => r.HasUserCompletedWorkoutDayAsync(1, request.ActualDate)).ReturnsAsync(false);
        _mockSessionRepo.Setup(r => r.AddAsync(It.IsAny<WorkoutSession>())).ReturnsAsync(createdSession);

        // Act
        var result = await _controller.LogWorkoutAsync(request);

        // Assert
        var response = ParseCreatedResponse<WorkoutSessionResponse>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal("Workout logged successfully", response.Message);
        Assert.Equal(10, response.Data.Id);
        Assert.Equal(65, response.Data.ActualDurationMinutes);
        Assert.Equal("Amazing!", response.Data.Feeling);
    }

    [Fact]
    public async Task LogWorkoutAsync_WorkoutDayNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = new LogWorkoutRequest { WorkoutDayId = 99 };
        _mockWorkoutDayRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((WorkoutDay?)null);

        // Act
        var result = await _controller.LogWorkoutAsync(request);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("WorkoutDay with identifier '99' not found", errorResponse.Error);
    }

    [Fact]
    public async Task LogWorkoutAsync_AlreadyLogged_ReturnsConflict()
    {
        // Arrange
        var workoutDay = CreateTestWorkoutDay(1);
        var request = new LogWorkoutRequest
        {
            WorkoutDayId = 1,
            ActualDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ActualDurationMinutes = 60
        };

        _mockWorkoutDayRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(workoutDay);
        _mockSessionRepo.Setup(r => r.HasUserCompletedWorkoutDayAsync(1, request.ActualDate)).ReturnsAsync(true);

        // Act
        var result = await _controller.LogWorkoutAsync(request);

        // Assert
        var errorResponse = ParseErrorResponse(result, 409);
        Assert.False(errorResponse.Success);
        Assert.Equal("Workout already logged for this date", errorResponse.Error);
        _mockSessionRepo.Verify(r => r.AddAsync(It.IsAny<WorkoutSession>()), Times.Never);
    }

    [Fact]
    public async Task LogWorkoutAsync_InvalidModelState_ReturnsValidationError()
    {
        // Arrange
        var request = new LogWorkoutRequest();
        _controller.ModelState.AddModelError("ActualDurationMinutes", "Duration is required");

        // Act
        var result = await _controller.LogWorkoutAsync(request);

        // Assert
        var errorResponse = ParseErrorResponse(result, 400);
        Assert.False(errorResponse.Success);
        Assert.Equal("Validation failed", errorResponse.Error);
        Assert.NotNull(errorResponse.Errors);
    }

    #endregion

    #region GetStreakAsync

    [Fact]
    public async Task GetStreakAsync_UserExists_ReturnsStreakInfo()
    {
        // Arrange
        var user = CreateTestUser(1);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var sessions = new List<WorkoutSession>
        {
            CreateTestWorkoutSession(1, 1, today, 60),
            CreateTestWorkoutSession(2, 1, today.AddDays(-1), 55),
            CreateTestWorkoutSession(3, 1, today.AddDays(-2), 50),
            CreateTestWorkoutSession(4, 1, today.AddDays(-4), 45) // gap on day -3
        };

        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockSessionRepo.Setup(r => r.GetSessionsByUserAsync(1)).ReturnsAsync(sessions);

        // Act
        var result = await _controller.GetStreakAsync(1);

        // Assert
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);

        Assert.Equal(3, response.Data.GetProperty("currentStreak").GetInt32()); // today, -1, -2
        Assert.Equal(3, response.Data.GetProperty("longestStreak").GetInt32()); // 3-day streak
        var workoutDays = response.Data.GetProperty("workoutDays");
        Assert.Equal(4, workoutDays.GetProperty("total").GetInt32());
    }

    [Fact]
    public async Task GetStreakAsync_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetStreakAsync(99);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("User with identifier '99' not found", errorResponse.Error);
    }

    [Fact]
    public async Task GetStreakAsync_NoSessions_ReturnsZeroStreak()
    {
        // Arrange
        var user = CreateTestUser(1);
        var sessions = new List<WorkoutSession>();

        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockSessionRepo.Setup(r => r.GetSessionsByUserAsync(1)).ReturnsAsync(sessions);

        // Act
        var result = await _controller.GetStreakAsync(1);

        // Assert
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        Assert.Equal(0, response.Data.GetProperty("currentStreak").GetInt32());
        Assert.Equal(0, response.Data.GetProperty("longestStreak").GetInt32());
        Assert.Equal(0, response.Data.GetProperty("workoutDays").GetProperty("total").GetInt32());
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_SessionExists_ReturnsSuccess()
    {
        // Arrange
        var session = CreateTestWorkoutSession(1, 1);
        _mockSessionRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(session);
        _mockSessionRepo.Setup(r => r.SoftDeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteAsync(1);

        // Assert
        var response = ParseSuccessResponse<object>(result);
        Assert.True(response.Success);
        Assert.Equal("Workout session deleted successfully", response.Message);
        _mockSessionRepo.Verify(r => r.SoftDeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_SessionNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockSessionRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((WorkoutSession?)null);

        // Act
        var result = await _controller.DeleteAsync(99);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("WorkoutSession with identifier '99' not found", errorResponse.Error);
        _mockSessionRepo.Verify(r => r.SoftDeleteAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion
}
using GymFlow.Api.Controllers;
using GymFlow.Dal.Repositories.Interfaces;
using GymFlow.Models.DTOs.Responses;
using GymFlow.Models.Entities;
using GymFlow.Models.Enums;
using GymFlow.Tests.Api.Controllers.TestBase;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Text.Json;

namespace GymFlow.Tests.Api.Controllers;

public class StatisticsControllerTest : ControllerTestFixture
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IWorkoutPlanRepository> _mockPlanRepo;
    private readonly Mock<IWorkoutSessionRepository> _mockSessionRepo;
    private readonly Mock<IExerciseRepository> _mockExerciseRepo;
    private readonly Mock<IProgressLogRepository> _mockProgressRepo;
    private readonly StatisticsController _controller;

    public StatisticsControllerTest()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockPlanRepo = new Mock<IWorkoutPlanRepository>();
        _mockSessionRepo = new Mock<IWorkoutSessionRepository>();
        _mockExerciseRepo = new Mock<IExerciseRepository>();
        _mockProgressRepo = new Mock<IProgressLogRepository>();
        _controller = CreateController<StatisticsController>(
            _mockUserRepo.Object,
            _mockPlanRepo.Object,
            _mockSessionRepo.Object,
            _mockExerciseRepo.Object,
            _mockProgressRepo.Object);
    }

    #region Helper Methods

    private User CreateTestUser(int id = 1, Goal goal = Goal.Fitness, float weight = 80f, int age = 30, bool hasActivePlan = true)
    {
        var person = new Person
        {
            Id = id,
            FirstName = "Test",
            LastName = $"User{id}",
            Username = $"user{id}",
            Gender = Gender.Male,
            Age = age,
            Weight = weight,
            Height = 180f,
            BodyType = BodyType.Fit,
            CreatedAt = DateTime.UtcNow.AddMonths(-2)
        };

        var user = new User
        {
            Id = id,
            PersonId = person.Id,
            Person = person,
            Goal = goal,
            CreatedAt = DateTime.UtcNow.AddMonths(-2),
            WorkoutPlans = hasActivePlan ? new List<WorkoutPlan> { new WorkoutPlan { Id = id, IsActive = true } } : new List<WorkoutPlan>()
        };
        person.User = user;
        
        return user;
    }

    private WorkoutSession CreateTestWorkoutSession(int id = 1, int userId = 1, DateOnly? date = null, int duration = 60)
    {
        return new WorkoutSession
        {
            Id = id,
            WorkoutDayId = 1,
            ActualDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow),
            ActualDurationMinutes = duration,
            Feeling = "Good",
            CreatedAt = DateTime.UtcNow,
            WorkoutDay = new WorkoutDay { WorkoutPlan = new WorkoutPlan { UserId = userId } }
        };
    }

    private Exercise CreateTestExercise(int id = 1, string name = "Bench Press", MuscleGroup muscleGroup = MuscleGroup.Chest)
    {
        return new Exercise
        {
            Id = id,
            Name = name,
            PrimaryMuscleGroup = muscleGroup,
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion

    #region GetDashboardStatsAsync

    [Fact]
    public async Task GetDashboardStatsAsync_ShouldReturnDashboardStats()
    {
        var users = new List<User>
        {
            CreateTestUser(1, Goal.Fitness, 80f, 25, true),
            CreateTestUser(2, Goal.MuscleGain, 85f, 30, true),
            CreateTestUser(3, Goal.FatLoss, 70f, 35, false)
        };
        var activePlans = new List<WorkoutPlan> { new WorkoutPlan { Id = 1, IsActive = true } };
        var popularExercises = new List<Exercise>
        {
            CreateTestExercise(1, "Bench Press", MuscleGroup.Chest),
            CreateTestExercise(2, "Squat", MuscleGroup.Legs)
        };

        _mockUserRepo.Setup(r => r.GetAllUsersWithPersonAsync()).ReturnsAsync(users);
        _mockPlanRepo.Setup(r => r.FindAsync(p => p.IsActive)).ReturnsAsync(activePlans);
        _mockExerciseRepo.Setup(r => r.GetMostUsedExercisesAsync(5)).ReturnsAsync(popularExercises);

        var result = await _controller.GetDashboardStatsAsync();
        var response = ParseSuccessResponse<DashboardStatsResponse>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal(3, response.Data.TotalUsers);
        Assert.Equal(2, response.Data.ActiveUsers);
        Assert.Equal(1, response.Data.ActiveWorkoutPlans);  // اصلاح شده
        Assert.Equal(2, response.Data.MostUsedExercises.Count);
        Assert.Contains(response.Data.UsersByGoal, kv => kv.Key == "Fitness");
        Assert.Contains(response.Data.UsersByGender, kv => kv.Key == "Male");
        Assert.Equal(30, response.Data.AverageAge, 1);
        Assert.Equal(78.3f, response.Data.AverageWeight, 1);
    }
    #endregion

    #region GetUserDashboardAsync

    [Fact]
    public async Task GetUserDashboardAsync_UserNotFound_ReturnsNotFound()
    {
        _mockUserRepo.Setup(r => r.GetUserWithCompleteHistoryAsync(99)).ReturnsAsync((User?)null);

        var result = await _controller.GetUserDashboardAsync(99);
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("User with identifier '99' not found", errorResponse.Error);
    }

    [Fact]
    public async Task GetUserDashboardAsync_UserHasData_ReturnsDashboard()
    {
        var user = CreateTestUser(1, Goal.Fitness, 80f, 30, true);
        var activePlan = new WorkoutPlan { Id = 1, UserId = 1, Phase = 1, SessionsPerWeek = 3, StartDate = DateOnly.FromDateTime(DateTime.UtcNow) };
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var sessions = new List<WorkoutSession>
        {
            CreateTestWorkoutSession(1, 1, today, 60),
            CreateTestWorkoutSession(2, 1, today.AddDays(-1), 55),
            CreateTestWorkoutSession(3, 1, today.AddDays(-2), 65)
        };
        var logs = new List<ProgressLog>
        {
            new ProgressLog { Id = 1, UserId = 1, LogDate = today, Weight = 78f },
            new ProgressLog { Id = 2, UserId = 1, LogDate = today.AddDays(-7), Weight = 80f }
        };

        _mockUserRepo.Setup(r => r.GetUserWithCompleteHistoryAsync(1)).ReturnsAsync(user);
        _mockPlanRepo.Setup(r => r.GetActiveWorkoutPlanAsync(1)).ReturnsAsync(activePlan);
        _mockSessionRepo.Setup(r => r.GetSessionsByUserAsync(1)).ReturnsAsync(sessions);
        _mockProgressRepo.Setup(r => r.GetUserProgressHistoryAsync(1)).ReturnsAsync(logs);

        var result = await _controller.GetUserDashboardAsync(1);
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        
        var userData = response.Data.GetProperty("user");
        Assert.Equal(1, userData.GetProperty("Id").GetInt32());
        // Goal به صورت عدد (enum value) برمی‌گردد
        Assert.Equal(2, userData.GetProperty("Goal").GetInt32()); // Goal.Fitness = 2
        
        var stats = response.Data.GetProperty("stats");
        Assert.Equal(3, stats.GetProperty("totalWorkouts").GetInt32());
        Assert.Equal(78f, stats.GetProperty("latestWeight").GetSingle());
        Assert.Equal(80f, stats.GetProperty("startingWeight").GetSingle());
        
        // totalWeightChange بررسی
        if (stats.TryGetProperty("totalWeightChange", out var totalWeightChange))
        {
            Assert.Equal(JsonValueKind.Number, totalWeightChange.ValueKind);
            Assert.Equal(-2f, totalWeightChange.GetSingle());
        }
    }

    [Fact]
    public async Task GetUserDashboardAsync_UserHasNoActivePlan_ReturnsNullActivePlan()
    {
        var user = CreateTestUser(1, Goal.Fitness, 80f, 30, false);
        var sessions = new List<WorkoutSession>();
        var logs = new List<ProgressLog>();

        _mockUserRepo.Setup(r => r.GetUserWithCompleteHistoryAsync(1)).ReturnsAsync(user);
        _mockPlanRepo.Setup(r => r.GetActiveWorkoutPlanAsync(1)).ReturnsAsync((WorkoutPlan?)null);
        _mockSessionRepo.Setup(r => r.GetSessionsByUserAsync(1)).ReturnsAsync(sessions);
        _mockProgressRepo.Setup(r => r.GetUserProgressHistoryAsync(1)).ReturnsAsync(logs);

        var result = await _controller.GetUserDashboardAsync(1);
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        
        var activePlan = response.Data.GetProperty("activePlan");
        Assert.Equal(JsonValueKind.Null, activePlan.ValueKind);
    }

    #endregion

    #region GetQuickStatsAsync

    [Fact]
    public async Task GetQuickStatsAsync_UserNotFound_ReturnsNotFound()
    {
        _mockUserRepo.Setup(r => r.GetUserWithPersonAsync(99)).ReturnsAsync((User?)null);

        var result = await _controller.GetQuickStatsAsync(99);
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("User with identifier '99' not found", errorResponse.Error);
    }

    [Fact]
    public async Task GetQuickStatsAsync_UserHasData_ReturnsQuickStats()
    {
        var user = CreateTestUser(1, Goal.Fitness, 80f);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var sessions = new List<WorkoutSession>
        {
            CreateTestWorkoutSession(1, 1, today, 60),
            CreateTestWorkoutSession(2, 1, today.AddDays(-1), 55),
            CreateTestWorkoutSession(3, 1, today.AddDays(-2), 65)
        };
        var logs = new List<ProgressLog>
        {
            new ProgressLog { Id = 1, UserId = 1, LogDate = today, Weight = 78f },
            new ProgressLog { Id = 2, UserId = 1, LogDate = today.AddDays(-7), Weight = 80f }
        };

        _mockUserRepo.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockSessionRepo.Setup(r => r.GetSessionsByUserAsync(1)).ReturnsAsync(sessions);
        _mockProgressRepo.Setup(r => r.GetUserProgressHistoryAsync(1)).ReturnsAsync(logs);

        var result = await _controller.GetQuickStatsAsync(1);
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        
        Assert.Equal(3, response.Data.GetProperty("TotalWorkouts").GetInt32());
        Assert.Equal(78f, response.Data.GetProperty("CurrentWeight").GetSingle());
        Assert.Equal(-2f, response.Data.GetProperty("TotalWeightChange").GetSingle());
        Assert.Equal(180, response.Data.GetProperty("TotalWorkoutMinutes").GetInt32());
    }

    [Fact]
    public async Task GetQuickStatsAsync_UserHasNoLogs_UsesUserWeight()
    {
        var user = CreateTestUser(1, Goal.Fitness, 82f);
        var sessions = new List<WorkoutSession>();
        var logs = new List<ProgressLog>();

        _mockUserRepo.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockSessionRepo.Setup(r => r.GetSessionsByUserAsync(1)).ReturnsAsync(sessions);
        _mockProgressRepo.Setup(r => r.GetUserProgressHistoryAsync(1)).ReturnsAsync(logs);

        var result = await _controller.GetQuickStatsAsync(1);
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        
        Assert.Equal(0, response.Data.GetProperty("TotalWorkouts").GetInt32());
        Assert.Equal(82f, response.Data.GetProperty("CurrentWeight").GetSingle());
    }

    #endregion

    #region GetAchievementsAsync

    [Fact]
    public async Task GetAchievementsAsync_UserNotFound_ReturnsNotFound()
    {
        _mockUserRepo.Setup(r => r.GetUserWithPersonAsync(99)).ReturnsAsync((User?)null);

        var result = await _controller.GetAchievementsAsync(99);
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("User with identifier '99' not found", errorResponse.Error);
    }

    [Fact]
    public async Task GetAchievementsAsync_NoWorkouts_ReturnsFirstStepAchievement()
    {
        var user = CreateTestUser(1);
        var sessions = new List<WorkoutSession>();

        _mockUserRepo.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockSessionRepo.Setup(r => r.GetSessionsByUserAsync(1)).ReturnsAsync(sessions);

        var result = await _controller.GetAchievementsAsync(1);
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        
        var achievements = response.Data.EnumerateArray().ToList();
        Assert.Single(achievements);
        Assert.Equal("First Step", achievements[0].GetProperty("Name").GetString());
    }

    [Fact]
    public async Task GetAchievementsAsync_TenWorkouts_ReturnsGettingStarted()
    {
        var user = CreateTestUser(1);
        var sessions = new List<WorkoutSession>();
        for (int i = 0; i < 10; i++)
        {
            sessions.Add(CreateTestWorkoutSession(i + 1, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-i))));
        }

        _mockUserRepo.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockSessionRepo.Setup(r => r.GetSessionsByUserAsync(1)).ReturnsAsync(sessions);

        var result = await _controller.GetAchievementsAsync(1);
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        
        var achievements = response.Data.EnumerateArray().ToList();
        Assert.Contains(achievements, a => a.GetProperty("Name").GetString() == "Getting Started");
    }

    [Fact]
    public async Task GetAchievementsAsync_FiftyWorkouts_ReturnsDedicatedAthlete()
    {
        var user = CreateTestUser(1);
        var sessions = new List<WorkoutSession>();
        for (int i = 0; i < 50; i++)
        {
            sessions.Add(CreateTestWorkoutSession(i + 1, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-i))));
        }

        _mockUserRepo.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockSessionRepo.Setup(r => r.GetSessionsByUserAsync(1)).ReturnsAsync(sessions);

        var result = await _controller.GetAchievementsAsync(1);
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        
        var achievements = response.Data.EnumerateArray().ToList();
        Assert.Contains(achievements, a => a.GetProperty("Name").GetString() == "Dedicated Athlete");
    }

    [Fact]
    public async Task GetAchievementsAsync_SevenDayStreak_ReturnsConsistencyKing()
    {
        var user = CreateTestUser(1);
        var sessions = new List<WorkoutSession>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        for (int i = 0; i < 7; i++)
        {
            sessions.Add(CreateTestWorkoutSession(i + 1, 1, today.AddDays(-i)));
        }

        _mockUserRepo.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockSessionRepo.Setup(r => r.GetSessionsByUserAsync(1)).ReturnsAsync(sessions);

        var result = await _controller.GetAchievementsAsync(1);
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        
        var achievements = response.Data.EnumerateArray().ToList();
        Assert.Contains(achievements, a => a.GetProperty("Name").GetString() == "Consistency King");
    }

    #endregion
}
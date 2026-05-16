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

public class WorkoutPlansControllerTest : ControllerTestFixture
{
    private readonly Mock<IWorkoutPlanRepository> _mockWorkoutPlanRepo;
    private readonly Mock<IWorkoutDayRepository> _mockWorkoutDayRepo;
    private readonly Mock<IWorkoutSessionRepository> _mockWorkoutSessionRepo;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly WorkoutPlansController _controller;

    public WorkoutPlansControllerTest()
    {
        _mockWorkoutPlanRepo = new Mock<IWorkoutPlanRepository>();
        _mockWorkoutDayRepo = new Mock<IWorkoutDayRepository>();
        _mockWorkoutSessionRepo = new Mock<IWorkoutSessionRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _controller = CreateController<WorkoutPlansController>(
            _mockWorkoutPlanRepo.Object,
            _mockWorkoutDayRepo.Object,
            _mockWorkoutSessionRepo.Object,
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

    private WorkoutPlan CreateTestWorkoutPlan(int id = 1, int userId = 1, int phase = 1, bool isActive = true)
    {
        return new WorkoutPlan
        {
            Id = id,
            UserId = userId,
            Phase = phase,
            SessionsPerWeek = 3,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)),
            IsActive = isActive,
            Notes = "Test plan notes",
            CreatedAt = DateTime.UtcNow.AddMonths(-1),
            WorkoutDays = new List<WorkoutDay>()
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
            Notes = "Workout day notes",
            CreatedAt = DateTime.UtcNow,
            WorkoutDayExercises = new List<WorkoutDayExercise>()
        };
    }

    private WorkoutDayExercise CreateTestWorkoutDayExercise(int id = 1, int workoutDayId = 1, int exerciseId = 1)
    {
        return new WorkoutDayExercise
        {
            Id = id,
            WorkoutDayId = workoutDayId,
            ExerciseId = exerciseId,
            Sets = 3,
            Reps = "10,10,8",
            RestSeconds = 60,
            Notes = "Exercise notes",
            CreatedAt = DateTime.UtcNow,
            Exercise = new Exercise { Id = exerciseId, Name = "Bench Press", PrimaryMuscleGroup = MuscleGroup.Chest }
        };
    }

    #endregion

    #region GetByUserAsync

    [Fact]
    public async Task GetByUserAsync_UserExists_ReturnsPlans()
    {
        // Arrange
        var user = CreateTestUser(1);
        var plans = new List<WorkoutPlan>
        {
            CreateTestWorkoutPlan(1, 1, 1),
            CreateTestWorkoutPlan(2, 1, 2)
        };
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockWorkoutPlanRepo.Setup(r => r.GetUserWorkoutPlansAsync(1)).ReturnsAsync(plans);

        // Act
        var result = await _controller.GetByUserAsync(1);

        // Assert
        var response = ParseSuccessResponse<IEnumerable<WorkoutPlanResponse>>(result);
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

    #region GetDetailsAsync

    [Fact]
    public async Task GetDetailsAsync_PlanExists_ReturnsPlanWithDetails()
    {
        // Arrange
        var plan = CreateTestWorkoutPlan(1, 1, 1);
        var workoutDay = CreateTestWorkoutDay(1, 1, DayOfWeek.Monday);
        var exercise = CreateTestWorkoutDayExercise(1, 1, 1);
        workoutDay.WorkoutDayExercises.Add(exercise);
        plan.WorkoutDays.Add(workoutDay);

        _mockWorkoutPlanRepo.Setup(r => r.GetWorkoutPlanWithDetailsAsync(1)).ReturnsAsync(plan);

        // Act
        var result = await _controller.GetDetailsAsync(1);

        // Assert
        var response = ParseSuccessResponse<WorkoutPlanResponse>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal(1, response.Data.Id);
        Assert.Equal(1, response.Data.Phase);
        Assert.Equal(3, response.Data.SessionsPerWeek);
        Assert.NotNull(response.Data.WorkoutDays);
        Assert.Single(response.Data.WorkoutDays);
    }

    [Fact]
    public async Task GetDetailsAsync_PlanNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockWorkoutPlanRepo.Setup(r => r.GetWorkoutPlanWithDetailsAsync(99)).ReturnsAsync((WorkoutPlan?)null);

        // Act
        var result = await _controller.GetDetailsAsync(99);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("WorkoutPlan with identifier '99' not found", errorResponse.Error);
    }

    #endregion

    #region GetActivePlanAsync

    [Fact]
    public async Task GetActivePlanAsync_ActivePlanExists_ReturnsPlan()
    {
        // Arrange
        var user = CreateTestUser(1);
        var activePlan = CreateTestWorkoutPlan(1, 1, 1, true);
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockWorkoutPlanRepo.Setup(r => r.GetActiveWorkoutPlanAsync(1)).ReturnsAsync(activePlan);

        // Act
        var result = await _controller.GetActivePlanAsync(1);

        // Assert
        var response = ParseSuccessResponse<WorkoutPlanResponse>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal(1, response.Data.Id);
        Assert.True(response.Data.IsActive);
    }

    [Fact]
    public async Task GetActivePlanAsync_NoActivePlan_ReturnsSuccessWithNullMessage()
    {
        // Arrange
        var user = CreateTestUser(1);
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockWorkoutPlanRepo.Setup(r => r.GetActiveWorkoutPlanAsync(1)).ReturnsAsync((WorkoutPlan?)null);

        // Act
        var result = await _controller.GetActivePlanAsync(1);

        // Assert
        var response = ParseSuccessResponse<object>(result);
        Assert.True(response.Success);
        Assert.Null(response.Data);
        Assert.Equal("No active workout plan found", response.Message);
    }

    [Fact]
    public async Task GetActivePlanAsync_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetActivePlanAsync(99);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("User with identifier '99' not found", errorResponse.Error);
    }

    #endregion

    #region GetTodaysWorkoutAsync

    [Fact]
    public async Task GetTodaysWorkoutAsync_UserHasActivePlanWithTodayWorkout_ReturnsWorkout()
    {
        // Arrange
        var user = CreateTestUser(1);
        var activePlan = CreateTestWorkoutPlan(1, 1, 1, true);
        var today = DateTime.UtcNow.DayOfWeek;
        var workoutDay = CreateTestWorkoutDay(1, 1, today);
        var fullDay = CreateTestWorkoutDay(1, 1, today);
        var exercise = CreateTestWorkoutDayExercise(1, 1, 1);
        fullDay.WorkoutDayExercises.Add(exercise);

        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockWorkoutPlanRepo.Setup(r => r.GetActiveWorkoutPlanAsync(1)).ReturnsAsync(activePlan);
        _mockWorkoutDayRepo.Setup(r => r.GetWorkoutDaysByPlanAsync(1)).ReturnsAsync(new List<WorkoutDay> { workoutDay });
        _mockWorkoutDayRepo.Setup(r => r.GetWorkoutDayWithExercisesAsync(1)).ReturnsAsync(fullDay);
        _mockWorkoutSessionRepo.Setup(r => r.HasUserCompletedWorkoutDayAsync(1, It.IsAny<DateOnly>())).ReturnsAsync(false);

        // Act
        var result = await _controller.GetTodaysWorkoutAsync(1);

        // Assert
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        // بررسی وجود property به جای Assert.NotNull
        Assert.True(response.Data.TryGetProperty("workoutDay", out _));
        Assert.False(response.Data.GetProperty("isCompleted").GetBoolean());
    }

    [Fact]
    public async Task GetTodaysWorkoutAsync_NoActivePlan_ReturnsError()
    {
        // Arrange
        var user = CreateTestUser(1);
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockWorkoutPlanRepo.Setup(r => r.GetActiveWorkoutPlanAsync(1)).ReturnsAsync((WorkoutPlan?)null);

        // Act
        var result = await _controller.GetTodaysWorkoutAsync(1);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("No active workout plan found", errorResponse.Error);
    }

    [Fact]
    public async Task GetTodaysWorkoutAsync_NoWorkoutScheduledToday_ReturnsSuccessWithNull()
    {
        // Arrange
        var user = CreateTestUser(1);
        var activePlan = CreateTestWorkoutPlan(1, 1, 1, true);
        var today = DateTime.UtcNow.DayOfWeek;
        // روز دیگری غیر از امروز
        var otherDay = today == DayOfWeek.Monday ? DayOfWeek.Tuesday : DayOfWeek.Monday;
        var workoutDay = CreateTestWorkoutDay(1, 1, otherDay);

        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockWorkoutPlanRepo.Setup(r => r.GetActiveWorkoutPlanAsync(1)).ReturnsAsync(activePlan);
        _mockWorkoutDayRepo.Setup(r => r.GetWorkoutDaysByPlanAsync(1)).ReturnsAsync(new List<WorkoutDay> { workoutDay });

        // Act
        var result = await _controller.GetTodaysWorkoutAsync(1);

        // Assert
        var response = ParseSuccessResponse<object>(result);
        Assert.True(response.Success);
        Assert.Null(response.Data);
        Assert.Equal("No workout scheduled for today", response.Message);
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var user = CreateTestUser(1);
        var request = new CreateWorkoutPlanRequest
        {
            UserId = 1,
            Phase = 1,
            SessionsPerWeek = 4,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(2)),
            Notes = "New plan"
        };
        var createdPlan = CreateTestWorkoutPlan(10, 1, 1, true);
        createdPlan.SessionsPerWeek = 4; // تنظیم مقدار صحیح

        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockWorkoutPlanRepo.Setup(r => r.DeactivateAllAndAddAsync(It.IsAny<WorkoutPlan>())).ReturnsAsync(createdPlan);

        // Act
        var result = await _controller.CreateAsync(request);

        // Assert
        var response = ParseCreatedResponse<WorkoutPlanResponse>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal("Workout plan created successfully", response.Message);
        Assert.Equal(10, response.Data.Id);
        Assert.Equal(4, response.Data.SessionsPerWeek);
    }

    [Fact]
    public async Task CreateAsync_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = new CreateWorkoutPlanRequest { UserId = 99 };
        _mockUserRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        // Act
        var result = await _controller.CreateAsync(request);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("User with identifier '99' not found", errorResponse.Error);
    }

    [Fact]
    public async Task CreateAsync_InvalidModelState_ReturnsValidationError()
    {
        // Arrange
        var request = new CreateWorkoutPlanRequest();
        _controller.ModelState.AddModelError("SessionsPerWeek", "Invalid value");

        // Act
        var result = await _controller.CreateAsync(request);

        // Assert
        var errorResponse = ParseErrorResponse(result, 400);
        Assert.False(errorResponse.Success);
        Assert.Equal("Validation failed", errorResponse.Error);
        Assert.NotNull(errorResponse.Errors);
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var existingPlan = CreateTestWorkoutPlan(1, 1, 1);
        var request = new UpdateWorkoutPlanRequest
        {
            Phase = 2,
            SessionsPerWeek = 5,
            IsActive = false,
            Notes = "Updated notes"
        };

        _mockWorkoutPlanRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingPlan);
        _mockWorkoutPlanRepo.Setup(r => r.UpdateAsync(It.IsAny<WorkoutPlan>())).ReturnsAsync((WorkoutPlan p) => p);

        // Act
        var result = await _controller.UpdateAsync(1, request);

        // Assert
        var response = ParseSuccessResponse<WorkoutPlanResponse>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal("Workout plan updated successfully", response.Message);
        Assert.Equal(2, response.Data.Phase);
        Assert.Equal(5, response.Data.SessionsPerWeek);
        Assert.False(response.Data.IsActive);
        Assert.Equal("Updated notes", response.Data.Notes);
    }

    [Fact]
    public async Task UpdateAsync_PlanNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = new UpdateWorkoutPlanRequest { Phase = 2 };
        _mockWorkoutPlanRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((WorkoutPlan?)null);

        // Act
        var result = await _controller.UpdateAsync(99, request);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("WorkoutPlan with identifier '99' not found", errorResponse.Error);
    }

    [Fact]
    public async Task UpdateAsync_PartialUpdate_OnlyUpdatesProvidedFields()
    {
        // Arrange
        var existingPlan = CreateTestWorkoutPlan(1, 1, 1);
        existingPlan.SessionsPerWeek = 3;
        var request = new UpdateWorkoutPlanRequest { Phase = 3 };

        _mockWorkoutPlanRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingPlan);
        _mockWorkoutPlanRepo.Setup(r => r.UpdateAsync(It.IsAny<WorkoutPlan>())).ReturnsAsync((WorkoutPlan p) => p);

        // Act
        var result = await _controller.UpdateAsync(1, request);

        // Assert
        var response = ParseSuccessResponse<WorkoutPlanResponse>(result);
        Assert.NotNull(response.Data);
        Assert.Equal(3, response.Data.Phase);
        Assert.Equal(3, response.Data.SessionsPerWeek); // unchanged
        Assert.True(response.Data.IsActive); // unchanged
    }

    #endregion

    #region ActivateAsync

    [Fact]
    public async Task ActivateAsync_PlanExists_ReturnsSuccess()
    {
        // Arrange
        var plan = CreateTestWorkoutPlan(1, 1, 1, false);
        _mockWorkoutPlanRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(plan);
        _mockWorkoutPlanRepo.Setup(r => r.DeactivateAllUserPlansAsync(1)).ReturnsAsync(true);
        _mockWorkoutPlanRepo.Setup(r => r.ActivateWorkoutPlanAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.ActivateAsync(1);

        // Assert
        var response = ParseSuccessResponse<object>(result);
        Assert.True(response.Success);
        Assert.Equal("Workout plan activated successfully", response.Message);
        _mockWorkoutPlanRepo.Verify(r => r.DeactivateAllUserPlansAsync(1), Times.Once);
        _mockWorkoutPlanRepo.Verify(r => r.ActivateWorkoutPlanAsync(1), Times.Once);
    }

    [Fact]
    public async Task ActivateAsync_PlanNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockWorkoutPlanRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((WorkoutPlan?)null);

        // Act
        var result = await _controller.ActivateAsync(99);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("WorkoutPlan with identifier '99' not found", errorResponse.Error);
    }

    #endregion

    #region DeactivateAsync

    [Fact]
    public async Task DeactivateAsync_PlanExists_ReturnsSuccess()
    {
        // Arrange
        var plan = CreateTestWorkoutPlan(1, 1, 1, true);
        _mockWorkoutPlanRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(plan);
        _mockWorkoutPlanRepo.Setup(r => r.UpdateAsync(It.IsAny<WorkoutPlan>())).ReturnsAsync(plan);

        // Act
        var result = await _controller.DeactivateAsync(1);

        // Assert
        var response = ParseSuccessResponse<object>(result);
        Assert.True(response.Success);
        Assert.Equal("Workout plan deactivated successfully", response.Message);
        _mockWorkoutPlanRepo.Verify(r => r.UpdateAsync(It.Is<WorkoutPlan>(p => !p.IsActive)), Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_PlanNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockWorkoutPlanRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((WorkoutPlan?)null);

        // Act
        var result = await _controller.DeactivateAsync(99);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("WorkoutPlan with identifier '99' not found", errorResponse.Error);
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_PlanExists_ReturnsSuccess()
    {
        // Arrange
        var plan = CreateTestWorkoutPlan(1);
        _mockWorkoutPlanRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(plan);
        _mockWorkoutPlanRepo.Setup(r => r.SoftDeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteAsync(1);

        // Assert
        var response = ParseSuccessResponse<object>(result);
        Assert.True(response.Success);
        Assert.Equal("Workout plan deleted successfully", response.Message);
        _mockWorkoutPlanRepo.Verify(r => r.SoftDeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_PlanNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockWorkoutPlanRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((WorkoutPlan?)null);

        // Act
        var result = await _controller.DeleteAsync(99);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("WorkoutPlan with identifier '99' not found", errorResponse.Error);
    }

    #endregion
}
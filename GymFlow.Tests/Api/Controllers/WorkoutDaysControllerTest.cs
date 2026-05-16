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

public class WorkoutDaysControllerTest : ControllerTestFixture
{
    private readonly Mock<IWorkoutDayRepository> _mockWorkoutDayRepo;
    private readonly Mock<IWorkoutPlanRepository> _mockWorkoutPlanRepo;
    private readonly Mock<IExerciseRepository> _mockExerciseRepo;
    private readonly WorkoutDaysController _controller;

    public WorkoutDaysControllerTest()
    {
        _mockWorkoutDayRepo = new Mock<IWorkoutDayRepository>();
        _mockWorkoutPlanRepo = new Mock<IWorkoutPlanRepository>();
        _mockExerciseRepo = new Mock<IExerciseRepository>();
        _controller = CreateController<WorkoutDaysController>(
            _mockWorkoutDayRepo.Object,
            _mockWorkoutPlanRepo.Object,
            _mockExerciseRepo.Object);
    }

    #region Helper Methods

    private WorkoutPlan CreateTestWorkoutPlan(int id = 1, int userId = 1)
    {
        return new WorkoutPlan
        {
            Id = id,
            UserId = userId,
            Phase = 1,
            SessionsPerWeek = 3,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
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
            Notes = "Test exercise",
            CreatedAt = DateTime.UtcNow,
            Exercise = new Exercise { Id = exerciseId, Name = "Bench Press", PrimaryMuscleGroup = MuscleGroup.Chest }
        };
    }

    #endregion

    #region GetByPlanAsync

    [Fact]
    public async Task GetByPlanAsync_PlanExists_ReturnsWorkoutDays()
    {
        // Arrange
        var plan = CreateTestWorkoutPlan(1);
        var days = new List<WorkoutDay>
        {
            CreateTestWorkoutDay(1, 1, DayOfWeek.Monday),
            CreateTestWorkoutDay(2, 1, DayOfWeek.Wednesday),
            CreateTestWorkoutDay(3, 1, DayOfWeek.Friday)
        };
        _mockWorkoutPlanRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(plan);
        _mockWorkoutDayRepo.Setup(r => r.GetWorkoutDaysByPlanAsync(1)).ReturnsAsync(days);

        // Act
        var result = await _controller.GetByPlanAsync(1);

        // Assert
        var response = ParseSuccessResponse<IEnumerable<WorkoutDayResponse>>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal(3, response.Data.Count());
    }

    [Fact]
    public async Task GetByPlanAsync_PlanNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockWorkoutPlanRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((WorkoutPlan?)null);

        // Act
        var result = await _controller.GetByPlanAsync(99);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("WorkoutPlan with identifier '99' not found", errorResponse.Error);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WorkoutDayExists_ReturnsWorkoutDay()
    {
        // Arrange
        var workoutDay = CreateTestWorkoutDay(1, 1, DayOfWeek.Monday);
        var exercise = CreateTestWorkoutDayExercise(1, 1, 1);
        workoutDay.WorkoutDayExercises.Add(exercise);
        _mockWorkoutDayRepo.Setup(r => r.GetWorkoutDayWithExercisesAsync(1)).ReturnsAsync(workoutDay);

        // Act
        var result = await _controller.GetByIdAsync(1);

        // Assert
        var response = ParseSuccessResponse<WorkoutDayResponse>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal(1, response.Data.Id);
        Assert.Equal(DayOfWeek.Monday, response.Data.DayOfWeek);
        Assert.Equal(MuscleGroup.Chest, response.Data.TargetMuscles);
        Assert.Equal(60, response.Data.DurationMinutes);
        Assert.Equal(Intensity.Medium, response.Data.Intensity);
        Assert.Equal("Test notes", response.Data.Notes);
        Assert.NotNull(response.Data.Exercises);
        Assert.Single(response.Data.Exercises);
    }

    [Fact]
    public async Task GetByIdAsync_WorkoutDayNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockWorkoutDayRepo.Setup(r => r.GetWorkoutDayWithExercisesAsync(99)).ReturnsAsync((WorkoutDay?)null);

        // Act
        var result = await _controller.GetByIdAsync(99);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("WorkoutDay with identifier '99' not found", errorResponse.Error);
    }

    [Fact]
    public async Task GetByIdAsync_WorkoutDayWithoutExercises_ReturnsEmptyExercisesList()
    {
        // Arrange
        var workoutDay = CreateTestWorkoutDay(1, 1, DayOfWeek.Monday);
        workoutDay.WorkoutDayExercises = new List<WorkoutDayExercise>();
        _mockWorkoutDayRepo.Setup(r => r.GetWorkoutDayWithExercisesAsync(1)).ReturnsAsync(workoutDay);

        // Act
        var result = await _controller.GetByIdAsync(1);

        // Assert
        var response = ParseSuccessResponse<WorkoutDayResponse>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Null(response.Data.Exercises);
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new CreateWorkoutDayRequest
        {
            WorkoutPlanId = 1,
            DayOfWeek = DayOfWeek.Monday,
            TargetMuscles = MuscleGroup.Chest,
            DurationMinutes = 75,
            Intensity = Intensity.High,
            Notes = "New workout day"
        };
        var plan = CreateTestWorkoutPlan(1);
        var createdDay = CreateTestWorkoutDay(10, 1, DayOfWeek.Monday);
        createdDay.DurationMinutes = 75;
        createdDay.Intensity = Intensity.High;
        createdDay.Notes = "New workout day";

        _mockWorkoutPlanRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(plan);
        _mockWorkoutDayRepo.Setup(r => r.GetWorkoutDayByWeekdayAndPlanAsync(1, DayOfWeek.Monday))
            .ReturnsAsync((WorkoutDay?)null);
        _mockWorkoutDayRepo.Setup(r => r.AddAsync(It.IsAny<WorkoutDay>())).ReturnsAsync(createdDay);

        // Act
        var result = await _controller.CreateAsync(request);

        // Assert
        var response = ParseCreatedResponse<WorkoutDayResponse>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal("Workout day created successfully", response.Message);
        Assert.Equal(10, response.Data.Id);
        Assert.Equal(DayOfWeek.Monday, response.Data.DayOfWeek);
        Assert.Equal(75, response.Data.DurationMinutes);
        Assert.Equal(Intensity.High, response.Data.Intensity);
    }

    [Fact]
    public async Task CreateAsync_WorkoutPlanNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = new CreateWorkoutDayRequest { WorkoutPlanId = 99, DayOfWeek = DayOfWeek.Monday };
        _mockWorkoutPlanRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((WorkoutPlan?)null);

        // Act
        var result = await _controller.CreateAsync(request);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("WorkoutPlan with identifier '99' not found", errorResponse.Error);
    }

    [Fact]
    public async Task CreateAsync_DuplicateDayOfWeek_ReturnsConflict()
    {
        // Arrange
        var request = new CreateWorkoutDayRequest { WorkoutPlanId = 1, DayOfWeek = DayOfWeek.Monday };
        var plan = CreateTestWorkoutPlan(1);
        var existingDay = CreateTestWorkoutDay(1, 1, DayOfWeek.Monday);

        _mockWorkoutPlanRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(plan);
        _mockWorkoutDayRepo.Setup(r => r.GetWorkoutDayByWeekdayAndPlanAsync(1, DayOfWeek.Monday))
            .ReturnsAsync(existingDay);

        // Act
        var result = await _controller.CreateAsync(request);

        // Assert
        var errorResponse = ParseErrorResponse(result, 409);
        Assert.False(errorResponse.Success);
        Assert.Equal("A workout day for Monday already exists in this plan", errorResponse.Error);
        _mockWorkoutDayRepo.Verify(r => r.AddAsync(It.IsAny<WorkoutDay>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_InvalidModelState_ReturnsValidationError()
    {
        // Arrange
        var request = new CreateWorkoutDayRequest();
        _controller.ModelState.AddModelError("DurationMinutes", "Duration is required");

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
        var existingDay = CreateTestWorkoutDay(1, 1, DayOfWeek.Monday);
        var request = new UpdateWorkoutDayRequest
        {
            DayOfWeek = DayOfWeek.Wednesday,
            TargetMuscles = MuscleGroup.Back,
            DurationMinutes = 90,
            Intensity = Intensity.High,
            Notes = "Updated notes"
        };

        _mockWorkoutDayRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDay);
        _mockWorkoutDayRepo.Setup(r => r.UpdateAsync(It.IsAny<WorkoutDay>()))
            .ReturnsAsync((WorkoutDay d) => d);

        // Act
        var result = await _controller.UpdateAsync(1, request);

        // Assert
        var response = ParseSuccessResponse<WorkoutDayResponse>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal("Workout day updated successfully", response.Message);
        Assert.Equal(DayOfWeek.Wednesday, response.Data.DayOfWeek);
        Assert.Equal(MuscleGroup.Back, response.Data.TargetMuscles);
        Assert.Equal(90, response.Data.DurationMinutes);
        Assert.Equal(Intensity.High, response.Data.Intensity);
        Assert.Equal("Updated notes", response.Data.Notes);
    }

    [Fact]
    public async Task UpdateAsync_WorkoutDayNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = new UpdateWorkoutDayRequest { DurationMinutes = 60 };
        _mockWorkoutDayRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((WorkoutDay?)null);

        // Act
        var result = await _controller.UpdateAsync(99, request);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("WorkoutDay with identifier '99' not found", errorResponse.Error);
    }

    [Fact]
    public async Task UpdateAsync_PartialUpdate_OnlyUpdatesProvidedFields()
    {
        // Arrange
        var existingDay = CreateTestWorkoutDay(1, 1, DayOfWeek.Monday);
        existingDay.DurationMinutes = 60;
        existingDay.Intensity = Intensity.Medium;
        existingDay.Notes = "Original notes";

        var request = new UpdateWorkoutDayRequest { DurationMinutes = 90 }; // فقط duration را آپدیت می‌کند

        _mockWorkoutDayRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDay);
        _mockWorkoutDayRepo.Setup(r => r.UpdateAsync(It.IsAny<WorkoutDay>()))
            .ReturnsAsync((WorkoutDay d) => d);

        // Act
        var result = await _controller.UpdateAsync(1, request);

        // Assert
        var response = ParseSuccessResponse<WorkoutDayResponse>(result);
        Assert.NotNull(response.Data);
        Assert.Equal(90, response.Data.DurationMinutes);
        Assert.Equal(Intensity.Medium, response.Data.Intensity);
        Assert.Equal("Original notes", response.Data.Notes);
    }

    [Fact]
    public async Task UpdateAsync_InvalidModelState_ReturnsValidationError()
    {
        // Arrange
        var request = new UpdateWorkoutDayRequest();
        _controller.ModelState.AddModelError("DurationMinutes", "Invalid value");

        // Act
        var result = await _controller.UpdateAsync(1, request);

        // Assert
        var errorResponse = ParseErrorResponse(result, 400);
        Assert.False(errorResponse.Success);
        Assert.Equal("Validation failed", errorResponse.Error);
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ExistingWorkoutDay_ReturnsSuccess()
    {
        // Arrange
        var workoutDay = CreateTestWorkoutDay(1);
        _mockWorkoutDayRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(workoutDay);
        _mockWorkoutDayRepo.Setup(r => r.SoftDeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteAsync(1);

        // Assert
        var response = ParseSuccessResponse<object>(result);
        Assert.True(response.Success);
        Assert.Equal("Workout day deleted successfully", response.Message);
        _mockWorkoutDayRepo.Verify(r => r.SoftDeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WorkoutDayNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockWorkoutDayRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((WorkoutDay?)null);

        // Act
        var result = await _controller.DeleteAsync(99);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("WorkoutDay with identifier '99' not found", errorResponse.Error);
        _mockWorkoutDayRepo.Verify(r => r.SoftDeleteAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion
}
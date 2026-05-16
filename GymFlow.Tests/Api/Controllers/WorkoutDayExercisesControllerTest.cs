using GymFlow.Api.Controllers;
using GymFlow.Dal.Repositories.Interfaces;
using GymFlow.Models.Entities;
using GymFlow.Models.Enums;
using GymFlow.Tests.Api.Controllers.TestBase;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Text.Json;

namespace GymFlow.Tests.Api.Controllers;

public class WorkoutDayExercisesControllerTest : ControllerTestFixture
{
    private readonly Mock<IWorkoutDayRepository> _mockWorkoutDayRepo;
    private readonly Mock<IExerciseRepository> _mockExerciseRepo;
    private readonly WorkoutDayExercisesController _controller;

    public WorkoutDayExercisesControllerTest()
    {
        _mockWorkoutDayRepo = new Mock<IWorkoutDayRepository>();
        _mockExerciseRepo = new Mock<IExerciseRepository>();
        _controller = CreateController<WorkoutDayExercisesController>(
            _mockWorkoutDayRepo.Object,
            _mockExerciseRepo.Object);
    }

    #region Helper Methods

    private WorkoutDay CreateTestWorkoutDay(int id = 1, int planId = 1)
    {
        return new WorkoutDay
        {
            Id = id,
            WorkoutPlanId = planId,
            DayOfWeek = DayOfWeek.Monday,
            TargetMuscles = MuscleGroup.Chest,
            DurationMinutes = 60,
            Intensity = Intensity.Medium,
            CreatedAt = DateTime.UtcNow
        };
    }

    private Exercise CreateTestExercise(int id = 1, string name = "Bench Press")
    {
        return new Exercise
        {
            Id = id,
            Name = name,
            PrimaryMuscleGroup = MuscleGroup.Chest,
            CreatedAt = DateTime.UtcNow
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
            Notes = "Test note",
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new CreateWorkoutDayExerciseRequest
        {
            WorkoutDayId = 1,
            ExerciseId = 1,
            Sets = 4,
            Reps = "12,10,8,8",
            RestSeconds = 90,
            Notes = "Focus on form"
        };
        var workoutDay = CreateTestWorkoutDay(1);
        var exercise = CreateTestExercise(1);
        var existingExercises = new List<WorkoutDayExercise>();

        _mockWorkoutDayRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(workoutDay);
        _mockExerciseRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(exercise);
        _mockWorkoutDayRepo.Setup(r => r.GetExercisesByDayIdAsync(1)).ReturnsAsync(existingExercises);
        _mockWorkoutDayRepo.Setup(r => r.AddExerciseToDayAsync(It.IsAny<WorkoutDayExercise>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CreateAsync(request);

        // Assert
        var response = ParseCreatedResponse<JsonElement>(result);
        Assert.True(response.Success);
        Assert.Equal("Exercise added successfully", response.Message);
        // به جای Assert.NotNull، بررسی می‌کنیم که property وجود دارد
        Assert.True(response.Data.TryGetProperty("id", out _));
        _mockWorkoutDayRepo.Verify(r => r.AddExerciseToDayAsync(It.Is<WorkoutDayExercise>(
            wde => wde.WorkoutDayId == 1 && wde.ExerciseId == 1 && wde.Sets == 4)), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WorkoutDayNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = new CreateWorkoutDayExerciseRequest { WorkoutDayId = 99, ExerciseId = 1 };
        _mockWorkoutDayRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((WorkoutDay?)null);

        // Act
        var result = await _controller.CreateAsync(request);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("WorkoutDay with identifier '99' not found", errorResponse.Error);
    }

    [Fact]
    public async Task CreateAsync_ExerciseNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = new CreateWorkoutDayExerciseRequest { WorkoutDayId = 1, ExerciseId = 99 };
        var workoutDay = CreateTestWorkoutDay(1);
        _mockWorkoutDayRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(workoutDay);
        _mockExerciseRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Exercise?)null);

        // Act
        var result = await _controller.CreateAsync(request);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("Exercise with identifier '99' not found", errorResponse.Error);
    }

    [Fact]
    public async Task CreateAsync_DuplicateExercise_ReturnsConflict()
    {
        // Arrange
        var request = new CreateWorkoutDayExerciseRequest { WorkoutDayId = 1, ExerciseId = 1 };
        var workoutDay = CreateTestWorkoutDay(1);
        var exercise = CreateTestExercise(1);
        var existingExercises = new List<WorkoutDayExercise>
        {
            new() { ExerciseId = 1, WorkoutDayId = 1 }
        };

        _mockWorkoutDayRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(workoutDay);
        _mockExerciseRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(exercise);
        _mockWorkoutDayRepo.Setup(r => r.GetExercisesByDayIdAsync(1)).ReturnsAsync(existingExercises);

        // Act
        var result = await _controller.CreateAsync(request);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(409, conflictResult.StatusCode);
        _mockWorkoutDayRepo.Verify(r => r.AddExerciseToDayAsync(It.IsAny<WorkoutDayExercise>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_InvalidModelState_ReturnsValidationError()
    {
        // Arrange
        var request = new CreateWorkoutDayExerciseRequest();
        _controller.ModelState.AddModelError("Sets", "Sets is required");

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
        var existingWde = CreateTestWorkoutDayExercise(1, 1, 1);
        var request = new UpdateWorkoutDayExerciseRequest
        {
            Sets = 5,
            Reps = "12,10,8,8,6",
            RestSeconds = 120,
            Notes = "Updated notes"
        };

        _mockWorkoutDayRepo.Setup(r => r.GetExerciseByIdAsync(1)).ReturnsAsync(existingWde);
        _mockWorkoutDayRepo.Setup(r => r.UpdateExerciseAsync(It.IsAny<WorkoutDayExercise>()))
            .ReturnsAsync((WorkoutDayExercise wde) => wde);

        // Act
        var result = await _controller.UpdateAsync(1, request);

        // Assert
        var response = ParseSuccessResponse<object>(result);
        Assert.True(response.Success);
        Assert.Equal("Exercise updated successfully", response.Message);
        _mockWorkoutDayRepo.Verify(r => r.UpdateExerciseAsync(It.Is<WorkoutDayExercise>(
            wde => wde.Sets == 5 && wde.Reps == "12,10,8,8,6" && wde.RestSeconds == 120)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WorkoutDayExerciseNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = new UpdateWorkoutDayExerciseRequest { Sets = 4 };
        _mockWorkoutDayRepo.Setup(r => r.GetExerciseByIdAsync(99)).ReturnsAsync((WorkoutDayExercise?)null);

        // Act
        var result = await _controller.UpdateAsync(99, request);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("WorkoutDayExercise with identifier '99' not found", errorResponse.Error);
    }

    [Fact]
    public async Task UpdateAsync_PartialUpdate_OnlyUpdatesProvidedFields()
    {
        // Arrange
        var existingWde = CreateTestWorkoutDayExercise(1, 1, 1);
        existingWde.Sets = 3;
        existingWde.Reps = "10,10,8";
        existingWde.RestSeconds = 60;
        existingWde.Notes = "Original";

        var request = new UpdateWorkoutDayExerciseRequest { Sets = 5 }; // فقط Sets را آپدیت می‌کند

        _mockWorkoutDayRepo.Setup(r => r.GetExerciseByIdAsync(1)).ReturnsAsync(existingWde);
        _mockWorkoutDayRepo.Setup(r => r.UpdateExerciseAsync(It.IsAny<WorkoutDayExercise>()))
            .ReturnsAsync((WorkoutDayExercise wde) => wde);

        // Act
        var result = await _controller.UpdateAsync(1, request);

        // Assert
        var response = ParseSuccessResponse<object>(result);
        Assert.True(response.Success);
        Assert.Equal("Exercise updated successfully", response.Message);
        _mockWorkoutDayRepo.Verify(r => r.UpdateExerciseAsync(It.Is<WorkoutDayExercise>(
            wde => wde.Sets == 5 && wde.Reps == "10,10,8" && wde.RestSeconds == 60 && wde.Notes == "Original")), Times.Once);
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ExistingExercise_ReturnsSuccess()
    {
        // Arrange
        var wde = CreateTestWorkoutDayExercise(1, 1, 1);
        _mockWorkoutDayRepo.Setup(r => r.GetExerciseByIdAsync(1)).ReturnsAsync(wde);
        _mockWorkoutDayRepo.Setup(r => r.DeleteExerciseAsync(It.IsAny<WorkoutDayExercise>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteAsync(1);

        // Assert
        var response = ParseSuccessResponse<object>(result);
        Assert.True(response.Success);
        Assert.Equal("Exercise deleted successfully", response.Message);
        _mockWorkoutDayRepo.Verify(r => r.DeleteExerciseAsync(It.Is<WorkoutDayExercise>(
            wde => wde.Id == 1)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WorkoutDayExerciseNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockWorkoutDayRepo.Setup(r => r.GetExerciseByIdAsync(99)).ReturnsAsync((WorkoutDayExercise?)null);

        // Act
        var result = await _controller.DeleteAsync(99);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("WorkoutDayExercise with identifier '99' not found", errorResponse.Error);
        _mockWorkoutDayRepo.Verify(r => r.DeleteExerciseAsync(It.IsAny<WorkoutDayExercise>()), Times.Never);
    }

    #endregion
}
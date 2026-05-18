namespace GymFlow.Tests.Api.Controllers;

public class ExercisesControllerTest : ControllerTestFixture
{
    private readonly Mock<IExerciseRepository> _mockRepo;
    private readonly ExercisesController _controller;

    public ExercisesControllerTest()
    {
        _mockRepo = new Mock<IExerciseRepository>();
        _controller = CreateController<ExercisesController>(_mockRepo.Object);
    }

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_WithoutMuscleGroup_ReturnsAllExercises()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new() { Id = 1, Name = "Bench Press", PrimaryMuscleGroup = MuscleGroup.Chest, Description = "Chest exercise" },
            new() { Id = 2, Name = "Squat", PrimaryMuscleGroup = MuscleGroup.Legs, Description = "Leg exercise" }
        };
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(exercises);

        // Act
        var result = await _controller.GetAllAsync(null);

        // Assert
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        var data = response.Data.EnumerateArray().ToList();
        Assert.Equal(2, data.Count);
        Assert.Equal(1, data[0].GetProperty("Id").GetInt32());
        Assert.Equal("Bench Press", data[0].GetProperty("Name").GetString());
    }

    [Fact]
    public async Task GetAllAsync_WithValidMuscleGroup_ReturnsFilteredExercises()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new() { Id = 1, Name = "Bench Press", PrimaryMuscleGroup = MuscleGroup.Chest },
            new() { Id = 2, Name = "Push-up", PrimaryMuscleGroup = MuscleGroup.Chest }
        };
        _mockRepo.Setup(r => r.GetExercisesByMuscleGroupAsync(MuscleGroup.Chest))
            .ReturnsAsync(exercises);

        // Act
        var result = await _controller.GetAllAsync("Chest");

        // Assert
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        var data = response.Data.EnumerateArray().ToList();
        Assert.Equal(2, data.Count);
        Assert.All(data, e => Assert.Equal("Chest", e.GetProperty("MuscleGroup").GetString()));
    }

    [Fact]
    public async Task GetAllAsync_WithInvalidMuscleGroup_ReturnsAllExercises()
    {
        // Arrange
        var exercises = new List<Exercise> { new() { Id = 1, Name = "Bench Press", PrimaryMuscleGroup = MuscleGroup.Chest } };
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(exercises);

        // Act
        var result = await _controller.GetAllAsync("InvalidGroup");

        // Assert
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        var data = response.Data.EnumerateArray().ToList();
        Assert.Single(data);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsExercise()
    {
        // Arrange
        var exercise = new Exercise { Id = 1, Name = "Bench Press", PrimaryMuscleGroup = MuscleGroup.Chest, Description = "Test" };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(exercise);

        // Act
        var result = await _controller.GetByIdAsync(1);

        // Assert
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        Assert.Equal(1, response.Data.GetProperty("Id").GetInt32());
        Assert.Equal("Bench Press", response.Data.GetProperty("Name").GetString());
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Exercise?)null);

        // Act
        var result = await _controller.GetByIdAsync(99);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("Exercise with identifier '99' not found", errorResponse.Error);
    }

    #endregion

    #region GetPopularAsync

    [Fact]
    public async Task GetPopularAsync_ReturnsTopExercises()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new() { Id = 1, Name = "Bench Press", PrimaryMuscleGroup = MuscleGroup.Chest },
            new() { Id = 2, Name = "Squat", PrimaryMuscleGroup = MuscleGroup.Legs }
        };
        _mockRepo.Setup(r => r.GetMostUsedExercisesAsync(5)).ReturnsAsync(exercises);

        // Act
        var result = await _controller.GetPopularAsync(5);

        // Assert
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        var data = response.Data.EnumerateArray().ToList();
        Assert.Equal(2, data.Count);
        Assert.Equal("Bench Press", data[0].GetProperty("Name").GetString());
        Assert.Equal(0, data[0].GetProperty("UsageCount").GetInt32());
    }

    #endregion

    #region GetByMuscleGroupAsync

    [Fact]
    public async Task GetByMuscleGroupAsync_ValidGroup_ReturnsExercises()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new() { Id = 1, Name = "Bench Press", PrimaryMuscleGroup = MuscleGroup.Chest }
        };
        _mockRepo.Setup(r => r.GetExercisesByMuscleGroupAsync(MuscleGroup.Chest)).ReturnsAsync(exercises);

        // Act
        var result = await _controller.GetByMuscleGroupAsync("Chest");

        // Assert
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        var data = response.Data.EnumerateArray().ToList();
        Assert.Single(data);
        Assert.Equal(1, data[0].GetProperty("Id").GetInt32());
    }

    [Fact]
    public async Task GetByMuscleGroupAsync_InvalidGroup_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetByMuscleGroupAsync("InvalidGroup");

        // Assert
        var errorResponse = ParseErrorResponse(result, 400);
        Assert.False(errorResponse.Success);
        Assert.Equal("Invalid muscle group: InvalidGroup", errorResponse.Error);
        _mockRepo.Verify(r => r.GetExercisesByMuscleGroupAsync(It.IsAny<MuscleGroup>()), Times.Never);
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "New Exercise",
            PrimaryMuscleGroup = MuscleGroup.Arms,
            Description = "Test"
        };
        _mockRepo.Setup(r => r.ExerciseExistsAsync(request.Name)).ReturnsAsync(false);
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Exercise>()))
            .ReturnsAsync((Exercise e) => { e.Id = 10; return e; });

        // Act
        var result = await _controller.CreateAsync(request);

        // Assert
        var response = ParseCreatedResponse<JsonElement>(result);
        Assert.True(response.Success);
        Assert.Equal("Exercise created successfully", response.Message);
        Assert.Equal(10, response.Data.GetProperty("Id").GetInt32());
        Assert.Equal("New Exercise", response.Data.GetProperty("Name").GetString());
        _mockRepo.Verify(r => r.AddAsync(It.Is<Exercise>(e => e.Name == request.Name)), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ReturnsConflict()
    {
        // Arrange
        var request = new CreateExerciseRequest { Name = "Existing", PrimaryMuscleGroup = MuscleGroup.Chest };
        _mockRepo.Setup(r => r.ExerciseExistsAsync(request.Name)).ReturnsAsync(true);

        // Act
        var result = await _controller.CreateAsync(request);

        // Assert
        var errorResponse = ParseErrorResponse(result, 409);
        Assert.False(errorResponse.Success);
        Assert.Equal("Exercise 'Existing' already exists", errorResponse.Error);
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Exercise>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_InvalidModelState_ReturnsValidationError()
    {
        // Arrange
        _controller.ModelState.AddModelError("Name", "Required");
        var request = new CreateExerciseRequest();

        // Act
        var result = await _controller.CreateAsync(request);

        // Assert
        var errorResponse = ParseErrorResponse(result, 400);
        Assert.False(errorResponse.Success);
        Assert.Equal("Validation failed", errorResponse.Error);
        Assert.NotNull(errorResponse.Errors);
        Assert.Contains(errorResponse.Errors, e => e == "Required");
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var existing = new Exercise { Id = 1, Name = "Old", PrimaryMuscleGroup = MuscleGroup.Chest, Description = "Old desc" };
        var request = new UpdateExerciseRequest { Name = "Updated", Description = "New desc" };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Exercise>())).ReturnsAsync((Exercise e) => e);

        // Act
        var result = await _controller.UpdateAsync(1, request);

        // Assert
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        Assert.Equal("Exercise updated successfully", response.Message);
        Assert.Equal("Updated", response.Data.GetProperty("Name").GetString());
        Assert.Equal("New desc", response.Data.GetProperty("Description").GetString());
        _mockRepo.Verify(r => r.UpdateAsync(It.Is<Exercise>(e => e.Name == "Updated")), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExerciseNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Exercise?)null);
        var request = new UpdateExerciseRequest { Name = "New" };

        // Act
        var result = await _controller.UpdateAsync(99, request);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("Exercise with identifier '99' not found", errorResponse.Error);
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Exercise>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_InvalidModelState_ReturnsValidationError()
    {
        // Arrange
        _controller.ModelState.AddModelError("Name", "Invalid");
        var request = new UpdateExerciseRequest();

        // Act
        var result = await _controller.UpdateAsync(1, request);

        // Assert
        var errorResponse = ParseErrorResponse(result, 400);
        Assert.False(errorResponse.Success);
        Assert.Equal("Validation failed", errorResponse.Error);
        Assert.NotNull(errorResponse.Errors);
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ExistingUnusedExercise_ReturnsSuccess()
    {
        // Arrange
        var exercise = new Exercise { Id = 1, Name = "Test" };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(exercise);
        _mockRepo.Setup(r => r.GetExerciseUsageCountAsync(1)).ReturnsAsync(0);
        _mockRepo.Setup(r => r.SoftDeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteAsync(1);

        // Assert
        var response = ParseSuccessResponse<object>(result);
        Assert.True(response.Success);
        Assert.Equal("Exercise deleted successfully", response.Message);
        _mockRepo.Verify(r => r.SoftDeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ExerciseUsedInPlans_ReturnsConflict()
    {
        // Arrange
        var exercise = new Exercise { Id = 1, Name = "Used" };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(exercise);
        _mockRepo.Setup(r => r.GetExerciseUsageCountAsync(1)).ReturnsAsync(5);

        // Act
        var result = await _controller.DeleteAsync(1);

        // Assert
        var errorResponse = ParseErrorResponse(result, 409);
        Assert.False(errorResponse.Success);
        Assert.Equal("Cannot delete exercise that is used in 5 workout plans", errorResponse.Error);
        _mockRepo.Verify(r => r.SoftDeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ExerciseNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Exercise?)null);

        // Act
        var result = await _controller.DeleteAsync(99);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("Exercise with identifier '99' not found", errorResponse.Error);
    }

    #endregion
}
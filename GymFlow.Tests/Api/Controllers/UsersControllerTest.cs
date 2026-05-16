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

public class UsersControllerTest : ControllerTestFixture
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IWorkoutPlanRepository> _mockWorkoutPlanRepo;
    private readonly Mock<IWorkoutSessionRepository> _mockWorkoutSessionRepo;
    private readonly Mock<IProgressLogRepository> _mockProgressLogRepo;
    private readonly UsersController _controller;

    public UsersControllerTest()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockWorkoutPlanRepo = new Mock<IWorkoutPlanRepository>();
        _mockWorkoutSessionRepo = new Mock<IWorkoutSessionRepository>();
        _mockProgressLogRepo = new Mock<IProgressLogRepository>();
        _controller = CreateController<UsersController>(
            _mockUserRepo.Object,
            _mockWorkoutPlanRepo.Object,
            _mockWorkoutSessionRepo.Object,
            _mockProgressLogRepo.Object);
    }

    #region Helper Methods

    private User CreateTestUser(int id = 1, string email = "test@example.com")
    {
        var person = new Person
        {
            Id = id,
            FirstName = "Test",
            LastName = $"User{id}",
            Username = $"user{id}",
            Email = email,
            Gender = Gender.Male,
            Age = 30,
            Weight = 80f,
            Height = 180f,
            BodyType = BodyType.Fit,
            CreatedAt = DateTime.UtcNow
        };

        return new User
        {
            Id = id,
            PersonId = person.Id,
            Person = person,
            Goal = Goal.Fitness,
            CreatedAt = DateTime.UtcNow,
            WorkoutPlans = new List<WorkoutPlan>(),
            ProgressLogs = new List<ProgressLog>()
        };
    }

    private ProgressLog CreateTestProgressLog(int id, int userId, DateOnly date, float weight)
    {
        return new ProgressLog
        {
            Id = id,
            UserId = userId,
            LogDate = date,
            Weight = weight,
            CreatedAt = DateTime.UtcNow
        };
    }

    private WorkoutSession CreateTestWorkoutSession(int id, int userId, DateOnly date)
    {
        return new WorkoutSession
        {
            Id = id,
            WorkoutDayId = 1,
            ActualDate = date,
            ActualDurationMinutes = 60,
            CreatedAt = DateTime.UtcNow,
            WorkoutDay = new WorkoutDay { WorkoutPlan = new WorkoutPlan { UserId = userId } }
        };
    }

    #endregion

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            CreateTestUser(1, "user1@test.com"),
            CreateTestUser(2, "user2@test.com")
        };
        _mockUserRepo.Setup(r => r.GetAllUsersWithPersonAsync()).ReturnsAsync(users);

        // Act
        var result = await _controller.GetAllAsync();

        // Assert
        var response = ParseSuccessResponse<IEnumerable<UserResponse>>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data.Count());
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_UserExists_ReturnsUser()
    {
        // Arrange
        var user = CreateTestUser(1);
        _mockUserRepo.Setup(r => r.GetUserWithCompleteHistoryAsync(1)).ReturnsAsync(user);

        // Act
        var result = await _controller.GetByIdAsync(1);

        // Assert
        var response = ParseSuccessResponse<UserResponse>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal(1, response.Data.Id);
        Assert.Equal("Test", response.Data.FirstName);
        Assert.Equal("User1", response.Data.LastName);
    }

    [Fact]
    public async Task GetByIdAsync_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetUserWithCompleteHistoryAsync(99)).ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetByIdAsync(99);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("User with identifier '99' not found", errorResponse.Error);
    }

    #endregion

    #region GetByEmailAsync

    [Fact]
    public async Task GetByEmailAsync_UserExists_ReturnsUser()
    {
        // Arrange
        var user = CreateTestUser(1, "test@example.com");
        _mockUserRepo.Setup(r => r.GetUserByEmailAsync("test@example.com")).ReturnsAsync(user);

        // Act
        var result = await _controller.GetByEmailAsync("test@example.com");

        // Assert
        var response = ParseSuccessResponse<UserResponse>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal("test@example.com", response.Data.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetUserByEmailAsync("notfound@test.com")).ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetByEmailAsync("notfound@test.com");

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("User with identifier 'notfound@test.com' not found", errorResponse.Error);
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            FirstName = "New",
            LastName = "User",
            Email = "new@test.com",
            Gender = Gender.Male,
            Age = 25,
            Weight = 75f,
            Height = 175f,
            Goal = Goal.Fitness
        };
        var createdUser = CreateTestUser(10, "new@test.com");
        createdUser.Person.FirstName = "New";
        createdUser.Person.LastName = "User";

        _mockUserRepo.Setup(r => r.GetUserByEmailAsync("new@test.com")).ReturnsAsync((User?)null);
        _mockUserRepo.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync(createdUser);

        // Act
        var result = await _controller.CreateAsync(request);

        // Assert
        var response = ParseCreatedResponse<UserResponse>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal("User created successfully", response.Message);
        Assert.Equal("New", response.Data.FirstName);
        Assert.Equal("User", response.Data.LastName);
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var existingUser = CreateTestUser(1, "existing@test.com");
        var request = new CreateUserRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = "existing@test.com",
            Gender = Gender.Male,
            Age = 25,
            Goal = Goal.Fitness
        };
        _mockUserRepo.Setup(r => r.GetUserByEmailAsync("existing@test.com")).ReturnsAsync(existingUser);

        // Act
        var result = await _controller.CreateAsync(request);

        // Assert
        var errorResponse = ParseErrorResponse(result, 409);
        Assert.False(errorResponse.Success);
        Assert.Equal("A user with this email already exists", errorResponse.Error);
        _mockUserRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_InvalidModelState_ReturnsValidationError()
    {
        // Arrange
        var request = new CreateUserRequest();
        _controller.ModelState.AddModelError("FirstName", "First name is required");

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
        var existingUser = CreateTestUser(1);
        var request = new UpdateUserRequest
        {
            FirstName = "Updated",
            LastName = "Name",
            Weight = 85f
        };
        _mockUserRepo.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(existingUser);
        _mockUserRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

        // Act
        var result = await _controller.UpdateAsync(1, request);

        // Assert
        var response = ParseSuccessResponse<UserResponse>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal("User updated successfully", response.Message);
        Assert.Equal("Updated", response.Data.FirstName);
        Assert.Equal("Name", response.Data.LastName);
        Assert.Equal(85f, response.Data.Weight);
    }

    [Fact]
    public async Task UpdateAsync_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = new UpdateUserRequest { FirstName = "Updated" };
        _mockUserRepo.Setup(r => r.GetUserWithPersonAsync(99)).ReturnsAsync((User?)null);

        // Act
        var result = await _controller.UpdateAsync(99, request);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("User with identifier '99' not found", errorResponse.Error);
    }

    [Fact]
    public async Task UpdateAsync_InvalidModelState_ReturnsValidationError()
    {
        // Arrange
        var request = new UpdateUserRequest();
        _controller.ModelState.AddModelError("Email", "Invalid email");

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
    public async Task DeleteAsync_UserExists_ReturnsSuccess()
    {
        // Arrange
        var user = CreateTestUser(1);
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockUserRepo.Setup(r => r.SoftDeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteAsync(1);

        // Assert
        var response = ParseSuccessResponse<object>(result);
        Assert.True(response.Success);
        Assert.Equal("User deleted successfully", response.Message);
        _mockUserRepo.Verify(r => r.SoftDeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        // Act
        var result = await _controller.DeleteAsync(99);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("User with identifier '99' not found", errorResponse.Error);
    }

    #endregion

    #region GetUserSummaryAsync

    [Fact]
    public async Task GetUserSummaryAsync_UserExists_ReturnsSummary()
    {
        // Arrange
        var user = CreateTestUser(1);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var logs = new List<ProgressLog>
        {
            CreateTestProgressLog(1, 1, today, 78f),
            CreateTestProgressLog(2, 1, today.AddDays(-7), 80f)
        };
        var sessions = new List<WorkoutSession>
        {
            CreateTestWorkoutSession(1, 1, today),
            CreateTestWorkoutSession(2, 1, today.AddDays(-1))
        };

        _mockUserRepo.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockProgressLogRepo.Setup(r => r.GetUserProgressHistoryAsync(1)).ReturnsAsync(logs);
        _mockWorkoutSessionRepo.Setup(r => r.GetSessionsByUserAsync(1)).ReturnsAsync(sessions);

        // Act
        var result = await _controller.GetUserSummaryAsync(1);

        // Assert
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        Assert.Equal(1, response.Data.GetProperty("userId").GetInt32());
        Assert.Equal("Test User1", response.Data.GetProperty("userName").GetString());
        Assert.Equal(78f, response.Data.GetProperty("currentWeight").GetSingle());
        Assert.Equal(80f, response.Data.GetProperty("startingWeight").GetSingle());
        Assert.Equal(-2f, response.Data.GetProperty("weightChange").GetSingle());
        Assert.Equal(2, response.Data.GetProperty("totalWorkoutSessions").GetInt32());
        Assert.Equal(2, response.Data.GetProperty("totalProgressLogs").GetInt32());
    }

    [Fact]
    public async Task GetUserSummaryAsync_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetUserWithPersonAsync(99)).ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetUserSummaryAsync(99);

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("User with identifier '99' not found", errorResponse.Error);
    }

    [Fact]
    public async Task GetUserSummaryAsync_UserHasNoLogs_ReturnsNullWeights()
    {
        // Arrange
        var user = CreateTestUser(1);
        var logs = new List<ProgressLog>();
        var sessions = new List<WorkoutSession>();

        _mockUserRepo.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockProgressLogRepo.Setup(r => r.GetUserProgressHistoryAsync(1)).ReturnsAsync(logs);
        _mockWorkoutSessionRepo.Setup(r => r.GetSessionsByUserAsync(1)).ReturnsAsync(sessions);

        // Act
        var result = await _controller.GetUserSummaryAsync(1);

        // Assert
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        Assert.Equal(1, response.Data.GetProperty("userId").GetInt32());
        Assert.Equal(80f, response.Data.GetProperty("currentWeight").GetSingle());
        Assert.Equal(JsonValueKind.Null, response.Data.GetProperty("startingWeight").ValueKind);
        Assert.Equal(JsonValueKind.Null, response.Data.GetProperty("weightChange").ValueKind);
    }

    #endregion
}
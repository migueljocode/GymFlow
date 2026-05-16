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

public class ProgressControllerTest : ControllerTestFixture
{
    private readonly Mock<IProgressLogRepository> _mockProgressRepo;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IWorkoutSessionRepository> _mockSessionRepo;
    private readonly ProgressController _controller;

    public ProgressControllerTest()
    {
        _mockProgressRepo = new Mock<IProgressLogRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockSessionRepo = new Mock<IWorkoutSessionRepository>();
        _controller = CreateController<ProgressController>(
            _mockProgressRepo.Object,
            _mockUserRepo.Object,
            _mockSessionRepo.Object);
    }

    #region Helper Methods

    private User CreateTestUser(int id = 1, float weight = 80f)
    {
        return new User
        {
            Id = id,
            Person = new Person
            {
                Id = id,
                FirstName = "Test",
                LastName = "User",
                Weight = weight,
                Username = $"user{id}"
            }
        };
    }

    private ProgressLog CreateTestProgressLog(int id = 1, int userId = 1, DateOnly? logDate = null, float weight = 80f)
    {
        return new ProgressLog
        {
            Id = id,
            UserId = userId,
            LogDate = logDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Weight = weight,
            BodyFatPercentage = 15.5f,
            Notes = "Test note",
            CreatedAt = DateTime.UtcNow
        };
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

    #endregion

    #region GetByUserAsync

    [Fact]
    public async Task GetByUserAsync_UserNotFound_ReturnsNotFound()
    {
        _mockUserRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        var result = await _controller.GetByUserAsync(99);
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("User with identifier '99' not found", errorResponse.Error);
    }

    [Fact]
    public async Task GetByUserAsync_UserHasProgressLogs_ReturnsLogs()
    {
        var user = CreateTestUser(1);
        var logs = new List<ProgressLog>
        {
            CreateTestProgressLog(1, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)), 80f),
            CreateTestProgressLog(2, 1, DateOnly.FromDateTime(DateTime.UtcNow), 78f)
        };
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockProgressRepo.Setup(r => r.GetUserProgressHistoryAsync(1)).ReturnsAsync(logs);

        var result = await _controller.GetByUserAsync(1);
        var response = ParseSuccessResponse<IEnumerable<ProgressLogResponse>>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data.Count());
    }

    #endregion

    #region GetSummaryAsync

    [Fact]
    public async Task GetSummaryAsync_UserNotFound_ReturnsNotFound()
    {
        _mockUserRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        var result = await _controller.GetSummaryAsync(99);
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("User with identifier '99' not found", errorResponse.Error);
    }

    [Fact]
    public async Task GetSummaryAsync_UserHasNoLogs_UsesUserWeight()
    {
        var user = CreateTestUser(1, 82f);
        var logs = new List<ProgressLog>();
        var sessions = new List<WorkoutSession>();

        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockProgressRepo.Setup(r => r.GetUserProgressHistoryAsync(1)).ReturnsAsync(logs);
        _mockSessionRepo.Setup(r => r.GetSessionsByUserAsync(1)).ReturnsAsync(sessions);

        var result = await _controller.GetSummaryAsync(1);
        var response = ParseSuccessResponse<ProgressSummaryResponse>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal(82f, response.Data.CurrentWeight);
        Assert.Equal(0, response.Data.TotalWorkoutSessions);
    }

    [Fact]
    public async Task GetSummaryAsync_WithData_ReturnsFullSummary()
    {
        var user = CreateTestUser(1, 80f);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var logs = new List<ProgressLog>
        {
            CreateTestProgressLog(3, 1, today, 78f),
            CreateTestProgressLog(2, 1, today.AddDays(-7), 79f),
            CreateTestProgressLog(1, 1, today.AddDays(-14), 80f)
        };
        var sessions = new List<WorkoutSession>
        {
            CreateTestWorkoutSession(1, 1, today, 60),
            CreateTestWorkoutSession(2, 1, today.AddDays(-2), 55),
            CreateTestWorkoutSession(3, 1, today.AddDays(-4), 65)
        };

        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockProgressRepo.Setup(r => r.GetUserProgressHistoryAsync(1)).ReturnsAsync(logs);
        _mockSessionRepo.Setup(r => r.GetSessionsByUserAsync(1)).ReturnsAsync(sessions);

        var result = await _controller.GetSummaryAsync(1);
        var response = ParseSuccessResponse<ProgressSummaryResponse>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal(78f, response.Data.CurrentWeight);
        Assert.Equal(3, response.Data.TotalWorkoutSessions);
        Assert.True(response.Data.WeightChangeTotal < 0);
        Assert.Equal(60, response.Data.AverageSessionDuration, 1);
    }

    #endregion

    #region AddProgressLogAsync

    [Fact]
    public async Task AddProgressLogAsync_UserNotFound_ReturnsNotFound()
    {
        var request = new CreateProgressLogRequest { LogDate = DateOnly.FromDateTime(DateTime.UtcNow), Weight = 75f };
        _mockUserRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        var result = await _controller.AddProgressLogAsync(99, request);
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("User with identifier '99' not found", errorResponse.Error);
    }

    [Fact]
    public async Task AddProgressLogAsync_DuplicateDate_ReturnsConflict()
    {
        var user = CreateTestUser(1);
        var request = new CreateProgressLogRequest { LogDate = DateOnly.FromDateTime(DateTime.UtcNow), Weight = 75f };
        var existingLog = CreateTestProgressLog(1, 1, request.LogDate);

        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockProgressRepo.Setup(r => r.GetProgressLogByDateAsync(1, request.LogDate)).ReturnsAsync(existingLog);

        var result = await _controller.AddProgressLogAsync(1, request);
        var errorResponse = ParseErrorResponse(result, 409);
        Assert.False(errorResponse.Success);
        Assert.Contains("already exists", errorResponse.Error);
    }

    [Fact]
    public async Task AddProgressLogAsync_ValidRequest_ReturnsCreated()
    {
        var user = CreateTestUser(1, 80f);
        var request = new CreateProgressLogRequest
        {
            LogDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Weight = 75f,
            BodyFatPercentage = 15f,
            Notes = "New log"
        };
        var createdLog = CreateTestProgressLog(10, 1, request.LogDate, 75f);

        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockProgressRepo.Setup(r => r.GetProgressLogByDateAsync(1, request.LogDate)).ReturnsAsync((ProgressLog?)null);
        _mockProgressRepo.Setup(r => r.AddAsync(It.IsAny<ProgressLog>())).ReturnsAsync(createdLog);
        _mockUserRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(user);

        var result = await _controller.AddProgressLogAsync(1, request);
        var response = ParseCreatedResponse<ProgressLogResponse>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal("Progress log added successfully", response.Message);
        Assert.Equal(75f, response.Data.Weight);
    }

    [Fact]
    public async Task AddProgressLogAsync_InvalidModelState_ReturnsValidationError()
    {
        var request = new CreateProgressLogRequest();
        _controller.ModelState.AddModelError("Weight", "Weight is required");

        var result = await _controller.AddProgressLogAsync(1, request);
        var errorResponse = ParseErrorResponse(result, 400);
        Assert.False(errorResponse.Success);
        Assert.Equal("Validation failed", errorResponse.Error);
        Assert.NotNull(errorResponse.Errors);
    }

    #endregion

    #region UpdateProgressLogAsync

    [Fact]
    public async Task UpdateProgressLogAsync_LogNotFound_ReturnsNotFound()
    {
        var request = new UpdateProgressLogRequest { Weight = 70f };
        _mockProgressRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ProgressLog?)null);

        var result = await _controller.UpdateProgressLogAsync(99, request);
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("ProgressLog with identifier '99' not found", errorResponse.Error);
    }

    [Fact]
    public async Task UpdateProgressLogAsync_ValidRequest_ReturnsSuccess()
    {
        var existingLog = CreateTestProgressLog(1, 1, DateOnly.FromDateTime(DateTime.UtcNow), 80f);
        var request = new UpdateProgressLogRequest { Weight = 75f, BodyFatPercentage = 14f, Notes = "Updated" };
        var updatedLog = CreateTestProgressLog(1, 1, existingLog.LogDate, 75f);
        updatedLog.BodyFatPercentage = 14f;
        updatedLog.Notes = "Updated";

        var user = CreateTestUser(1, 80f);

        _mockProgressRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingLog);
        _mockProgressRepo.Setup(r => r.UpdateAsync(It.IsAny<ProgressLog>())).ReturnsAsync(updatedLog);
        _mockProgressRepo.Setup(r => r.GetLatestProgressLogAsync(1)).ReturnsAsync(updatedLog);
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockUserRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(user);

        var result = await _controller.UpdateProgressLogAsync(1, request);
        var response = ParseSuccessResponse<ProgressLogResponse>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal("Progress log updated successfully", response.Message);
        Assert.Equal(75f, response.Data.Weight);
        Assert.Equal(14f, response.Data.BodyFatPercentage);
        Assert.Equal("Updated", response.Data.Notes);
    }

    [Fact]
    public async Task UpdateProgressLogAsync_InvalidModelState_ReturnsValidationError()
    {
        var request = new UpdateProgressLogRequest();
        _controller.ModelState.AddModelError("Weight", "Invalid");

        var result = await _controller.UpdateProgressLogAsync(1, request);
        var errorResponse = ParseErrorResponse(result, 400);
        Assert.False(errorResponse.Success);
        Assert.Equal("Validation failed", errorResponse.Error);
    }

    #endregion

    #region DeleteProgressLogAsync

    [Fact]
    public async Task DeleteProgressLogAsync_LogNotFound_ReturnsNotFound()
    {
        _mockProgressRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ProgressLog?)null);

        var result = await _controller.DeleteProgressLogAsync(99);
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("ProgressLog with identifier '99' not found", errorResponse.Error);
    }

    [Fact]
    public async Task DeleteProgressLogAsync_ValidRequest_ReturnsSuccess()
    {
        var log = CreateTestProgressLog(1, 1);
        _mockProgressRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(log);
        _mockProgressRepo.Setup(r => r.SoftDeleteAsync(1)).ReturnsAsync(true);

        var result = await _controller.DeleteProgressLogAsync(1);
        var response = ParseSuccessResponse<object>(result);
        Assert.True(response.Success);
        Assert.Equal("Progress log deleted successfully", response.Message);
        _mockProgressRepo.Verify(r => r.SoftDeleteAsync(1), Times.Once);
    }

    #endregion

    #region GetWeightHistoryAsync

    [Fact]
    public async Task GetWeightHistoryAsync_UserNotFound_ReturnsNotFound()
    {
        _mockUserRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        var result = await _controller.GetWeightHistoryAsync(99);
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("User with identifier '99' not found", errorResponse.Error);
    }

    [Fact]
    public async Task GetWeightHistoryAsync_UserHasLogs_ReturnsOrderedHistory()
    {
        var user = CreateTestUser(1);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var logs = new List<ProgressLog>
        {
            CreateTestProgressLog(1, 1, today.AddDays(-14), 80f),
            CreateTestProgressLog(2, 1, today.AddDays(-7), 79f),
            CreateTestProgressLog(3, 1, today, 78f)
        };
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockProgressRepo.Setup(r => r.GetUserProgressHistoryAsync(1)).ReturnsAsync(logs);

        var result = await _controller.GetWeightHistoryAsync(1);
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        
        var history = response.Data.EnumerateArray().ToList();
        Assert.Equal(3, history.Count);
        
        // تاریخ‌ها باید به ترتیب صعودی باشند (قدیمی‌ترین اول)
        var date1 = history[0].GetProperty("Date").GetString();
        var date2 = history[1].GetProperty("Date").GetString();
        var date3 = history[2].GetProperty("Date").GetString();
        
        Assert.True(string.Compare(date1, date2, StringComparison.Ordinal) <= 0);
        Assert.True(string.Compare(date2, date3, StringComparison.Ordinal) <= 0);
    }

    #endregion
}
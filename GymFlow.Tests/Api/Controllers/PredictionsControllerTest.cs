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

public class PredictionsControllerTest : ControllerTestFixture
{
    private readonly Mock<IProgressLogRepository> _mockProgressRepo;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly PredictionsController _controller;

    public PredictionsControllerTest()
    {
        _mockProgressRepo = new Mock<IProgressLogRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _controller = CreateController<PredictionsController>(_mockProgressRepo.Object, _mockUserRepo.Object);
    }

    #region GetPredictionAsync

    [Fact]
    public async Task GetPredictionAsync_UserNotFound_ReturnsNotFound()
    {
        _mockUserRepo.Setup(r => r.GetUserWithPersonAsync(99))
            .ReturnsAsync((User?)null);

        var result = await _controller.GetPredictionAsync(99);
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("User with identifier '99' not found", errorResponse.Error);
    }

    [Fact]
    public async Task GetPredictionAsync_NoWeightLogs_ReturnsBasePredictionUsingUserWeight()
    {
        var user = new User
        {
            Id = 1,
            Person = new Person { Weight = 80f, FirstName = "Test", LastName = "User" }
        };
        _mockUserRepo.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockProgressRepo.Setup(r => r.GetWeightTrendAsync(1, 10))
            .ReturnsAsync(new List<ProgressLog>());

        var result = await _controller.GetPredictionAsync(1);
        var response = ParseSuccessResponse<PredictionResponse>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal(80f, response.Data.CurrentWeight);
        Assert.Equal(0, response.Data.DataPointsUsed);
        Assert.Equal("Low", response.Data.Confidence);
        Assert.Equal("Need at least 3 weight logs for prediction. Currently have 0 log(s).",
            response.Data.Message);
        // AverageWeeklyChange is not set in this case, so we don't assert anything about it
    }

    [Fact]
    public async Task GetPredictionAsync_OneWeightLog_ReturnsBasePrediction()
    {
        var user = new User { Id = 1, Person = new Person { Weight = 80f } };
        var logs = new List<ProgressLog>
        {
            new() { LogDate = DateOnly.FromDateTime(DateTime.UtcNow), Weight = 78f }
        };
        _mockUserRepo.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockProgressRepo.Setup(r => r.GetWeightTrendAsync(1, 10)).ReturnsAsync(logs);

        var result = await _controller.GetPredictionAsync(1);
        var response = ParseSuccessResponse<PredictionResponse>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal(78f, response.Data.CurrentWeight);
        Assert.Equal(1, response.Data.DataPointsUsed);
        Assert.Equal("Low", response.Data.Confidence);
        Assert.Equal("Need at least 3 weight logs for prediction. Currently have 1 log(s).",
            response.Data.Message);
    }

    [Fact]
    public async Task GetPredictionAsync_TwoWeightLogs_ReturnsBasePrediction()
    {
        var user = new User { Id = 1, Person = new Person { Weight = 80f } };
        var logs = new List<ProgressLog>
        {
            new() { LogDate = DateOnly.FromDateTime(DateTime.UtcNow), Weight = 78f },
            new() { LogDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)), Weight = 80f }
        };
        _mockUserRepo.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockProgressRepo.Setup(r => r.GetWeightTrendAsync(1, 10)).ReturnsAsync(logs);

        var result = await _controller.GetPredictionAsync(1);
        var response = ParseSuccessResponse<PredictionResponse>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal(78f, response.Data.CurrentWeight);
        Assert.Equal(2, response.Data.DataPointsUsed);
        Assert.Equal("Low", response.Data.Confidence);
        Assert.Equal("Need at least 3 weight logs for prediction. Currently have 2 log(s).",
            response.Data.Message);
    }

    [Fact]
    public async Task GetPredictionAsync_ThreeWeightLogsWithWeightLoss_ReturnsFullPrediction()
    {
        var user = new User { Id = 1, Person = new Person { Weight = 80f } };
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var logs = new List<ProgressLog>
        {
            new() { LogDate = today, Weight = 78f },
            new() { LogDate = today.AddDays(-7), Weight = 79f },
            new() { LogDate = today.AddDays(-14), Weight = 80f }
        };
        _mockUserRepo.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockProgressRepo.Setup(r => r.GetWeightTrendAsync(1, 10)).ReturnsAsync(logs);

        var result = await _controller.GetPredictionAsync(1);
        var response = ParseSuccessResponse<PredictionResponse>(result);
        Assert.NotNull(response.Data);
        Assert.True(response.Success);
        Assert.Equal(78f, response.Data.CurrentWeight);
        Assert.Equal(3, response.Data.DataPointsUsed);
        Assert.Equal("Low", response.Data.Confidence);
        Assert.Equal("Losing", response.Data.Trend);
        Assert.True(response.Data.AverageWeeklyChange < 0);
        
        // فقط بررسی وجود مقادیر (بدون محدوده)
        Assert.NotNull(response.Data.PredictedWeight7Days);
        Assert.NotNull(response.Data.PredictedWeight30Days);
        Assert.NotNull(response.Data.PredictedWeight90Days);
        
        // بررسی اینکه وزن پیش‌بینی شده کمتر از وزن فعلی است
        Assert.True(response.Data.PredictedWeight7Days < response.Data.CurrentWeight);
        Assert.True(response.Data.PredictedWeight30Days < response.Data.PredictedWeight7Days);
        Assert.True(response.Data.PredictedWeight90Days < response.Data.PredictedWeight30Days);
        
        Assert.Equal("You're on track! Keep up the good work 💪", response.Data.Message);
    }

    [Fact]
    public async Task GetPredictionAsync_EnoughDataForMediumConfidence_ReturnsConfidenceMedium()
    {
        var user = new User { Id = 1, Person = new Person { Weight = 80f } };
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var logs = new List<ProgressLog>();
        for (int i = 0; i < 6; i++)
        {
            logs.Add(new ProgressLog { LogDate = today.AddDays(-i * 7), Weight = 80 - i });
        }
        _mockUserRepo.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockProgressRepo.Setup(r => r.GetWeightTrendAsync(1, 10)).ReturnsAsync(logs);

        var result = await _controller.GetPredictionAsync(1);
        var response = ParseSuccessResponse<PredictionResponse>(result);
        Assert.NotNull(response.Data);
        Assert.Equal(6, response.Data.DataPointsUsed);
        Assert.Equal("Medium", response.Data.Confidence);
    }

    [Fact]
    public async Task GetPredictionAsync_TenOrMoreWeightLogs_ReturnsHighConfidence()
    {
        var user = new User { Id = 1, Person = new Person { Weight = 80f } };
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var logs = new List<ProgressLog>();
        for (int i = 0; i < 10; i++)
        {
            logs.Add(new ProgressLog { LogDate = today.AddDays(-i * 7), Weight = 80 - i });
        }
        _mockUserRepo.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockProgressRepo.Setup(r => r.GetWeightTrendAsync(1, 10)).ReturnsAsync(logs);

        var result = await _controller.GetPredictionAsync(1);
        var response = ParseSuccessResponse<PredictionResponse>(result);
        Assert.NotNull(response.Data);
        Assert.Equal(10, response.Data.DataPointsUsed);
        Assert.Equal("High", response.Data.Confidence);
    }

    #endregion

    #region GetTrendAsync

    [Fact]
    public async Task GetTrendAsync_UserNotFound_ReturnsNotFound()
    {
        _mockUserRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        var result = await _controller.GetTrendAsync(99);
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("User with identifier '99' not found", errorResponse.Error);
    }

    [Fact]
    public async Task GetTrendAsync_LessThanTwoWeightLogs_ReturnsMessageInsufficientData()
    {
        var user = new User { Id = 1 };
        var logs = new List<ProgressLog>
        {
            new() { LogDate = DateOnly.FromDateTime(DateTime.UtcNow), Weight = 75f }
        };
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockProgressRepo.Setup(r => r.GetUserProgressHistoryAsync(1)).ReturnsAsync(logs);

        var result = await _controller.GetTrendAsync(1);
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        Assert.Equal("Not enough data for trend analysis", response.Data.GetProperty("message").GetString());
        Assert.Equal(1, response.Data.GetProperty("dataPoints").GetInt32());
    }

    [Fact]
    public async Task GetTrendAsync_TwoOrMoreWeightLogs_ReturnsTrendData()
    {
        var user = new User { Id = 1 };
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var logs = new List<ProgressLog>
        {
            new() { LogDate = today, Weight = 80f },
            new() { LogDate = today.AddDays(-7), Weight = 82f },
            new() { LogDate = today.AddDays(-14), Weight = 83f }
        };
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockProgressRepo.Setup(r => r.GetUserProgressHistoryAsync(1)).ReturnsAsync(logs);

        var result = await _controller.GetTrendAsync(1);
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        Assert.Equal(80f, response.Data.GetProperty("currentWeight").GetSingle());
        Assert.Equal(83f, response.Data.GetProperty("startingWeight").GetSingle());
        Assert.Equal(-3f, response.Data.GetProperty("totalChange").GetSingle());
        Assert.True(Math.Abs(response.Data.GetProperty("changePercentage").GetSingle() - (-3.614f)) < 0.01);
        Assert.Equal(3, response.Data.GetProperty("dataPoints").GetInt32());
        Assert.Equal("14 days", response.Data.GetProperty("timespan").GetString());
        Assert.Equal(JsonValueKind.Object, response.Data.GetProperty("weeklyAverages").ValueKind);
    }

    #endregion
}
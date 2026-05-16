using Xunit;
using Moq;
using GymFlow.Dal.Repositories.Interfaces;
using GymFlow.Models.Entities;
using GymFlow.Models.Enums;
using GymFlow.Models.DTOs.Responses;
using GymFlow.Services.Implementations;

namespace GymFlow.Tests.Services;

public class WeightPredictionServiceTest
{
    private readonly Mock<IProgressLogRepository> _mockProgressLogRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly WeightPredictionService _weightPredictionService;

    public WeightPredictionServiceTest()
    {
        _mockProgressLogRepository = new Mock<IProgressLogRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _weightPredictionService = new WeightPredictionService(
            _mockProgressLogRepository.Object,
            _mockUserRepository.Object);
    }

    // ========== Helper Methods ==========

    private User CreateTestUser(int id = 1, float weight = 80f, Goal goal = Goal.Fitness)
    {
        var person = new Person
        {
            Id = id,
            FirstName = "Test",
            LastName = "User",
            Username = $"testuser{id}",
            Email = $"test{id}@test.com",
            Gender = Gender.Male,
            Age = 30,
            Weight = weight,
            Height = 180f,
            BodyType = BodyType.Fit,
            CreatedAt = DateTime.UtcNow,
            User = new User { Id = id, Goal = goal }
        };
        return new User
        {
            Id = id,
            PersonId = person.Id,
            Person = person,
            Goal = goal,
            CreatedAt = DateTime.UtcNow
        };
    }

    // تولید لاگ به صورت نزولی (جدیدترین اول) – همانند خروجی واقعی ریپازیتوری
    private List<ProgressLog> CreateWeightLogsDescending(params (DateOnly date, float weight)[] logs)
    {
        return logs
            .Select((log, idx) => new ProgressLog
            {
                Id = idx + 1,
                UserId = 1,
                LogDate = log.date,
                Weight = log.weight,
                CreatedAt = DateTime.UtcNow
            })
            .OrderByDescending(l => l.LogDate)
            .ToList();
    }

    // تولید لاگ‌های هفتگی نزولی با تغییر هفتگی مشخص (مثال: weeklyChange = -0.5 یعنی کاهش 0.5 کیلو در هفته)
    private List<ProgressLog> CreateWeeklyWeightLogsDescending(float startWeight, float weeklyChange, int weeks)
    {
        var logs = new List<ProgressLog>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        // i = 0 -> امروز, i = weeks -> قدیمی‌ترین
        for (int i = 0; i <= weeks; i++)
        {
            // وزن در i هفته قبل = startWeight + (weeklyChange * i)
            float weight = startWeight + (weeklyChange * i);
            logs.Add(new ProgressLog
            {
                Id = i + 1,
                UserId = 1,
                LogDate = today.AddDays(-i * 7),
                Weight = (float)Math.Round(weight, 1),
                CreatedAt = DateTime.UtcNow
            });
        }
        return logs.OrderByDescending(l => l.LogDate).ToList();
    }

    // ========== PredictWeightAsync Tests ==========

    [Fact]
    public async Task PredictWeightAsync_WithEnoughData_ShouldReturnPrediction()
    {
        var logs = CreateWeeklyWeightLogsDescending(80f, -0.5f, 6);
        _mockProgressLogRepository.Setup(r => r.GetWeightTrendAsync(1, 10))
            .ReturnsAsync(logs);
        _mockProgressLogRepository.Setup(r => r.GetAverageWeeklyProgressAsync(1, 4))
            .ReturnsAsync(-0.5f);

        var result = await _weightPredictionService.PredictWeightAsync(1, 7);

        Assert.NotNull(result);
        Assert.Equal(79.5f, result.Value, 1);
    }

    [Fact]
    public async Task PredictWeightAsync_WithInsufficientData_ShouldReturnNull()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var logs = CreateWeightLogsDescending((today, 80f));
        _mockProgressLogRepository.Setup(r => r.GetWeightTrendAsync(1, 10))
            .ReturnsAsync(logs);

        var result = await _weightPredictionService.PredictWeightAsync(1, 7);
        Assert.Null(result);
    }

    [Fact]
    public async Task PredictWeightAsync_WithNoWeeklyChange_ShouldReturnNull()
    {
        var logs = CreateWeeklyWeightLogsDescending(80f, -0.5f, 6);
        _mockProgressLogRepository.Setup(r => r.GetWeightTrendAsync(1, 10))
            .ReturnsAsync(logs);
        _mockProgressLogRepository.Setup(r => r.GetAverageWeeklyProgressAsync(1, 4))
            .ReturnsAsync((float?)null);

        var result = await _weightPredictionService.PredictWeightAsync(1, 30);
        Assert.Null(result);
    }

    // ========== GetPredictionAsync Tests ==========
    [Fact]
    public async Task GetPredictionAsync_WithInsufficientData_ShouldReturnBaseResponse()
    {
        var user = CreateTestUser(1, 80f);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var logs = CreateWeightLogsDescending((today, 80f));

        _mockUserRepository.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockProgressLogRepository.Setup(r => r.GetWeightTrendAsync(1, 15)).ReturnsAsync(logs);

        var result = await _weightPredictionService.GetPredictionAsync(1);

        Assert.NotNull(result);
        Assert.Equal(80f, result.CurrentWeight);
        Assert.Equal(1, result.DataPointsUsed);
        Assert.Equal("Low", result.Confidence);
        Assert.Null(result.PredictedWeight7Days);
    }

    [Fact]
    public async Task GetPredictionAsync_WithNoLogs_ShouldUseUserWeight()
    {
        var user = CreateTestUser(1, 85f);
        var logs = new List<ProgressLog>();

        _mockUserRepository.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockProgressLogRepository.Setup(r => r.GetWeightTrendAsync(1, 15)).ReturnsAsync(logs);

        var result = await _weightPredictionService.GetPredictionAsync(1);
        Assert.Equal(85f, result.CurrentWeight);
    }

    // ========== GetAverageWeeklyChangeAsync Tests ==========

    [Fact]
    public async Task GetAverageWeeklyChangeAsync_ShouldReturnValueFromRepository()
    {
        _mockProgressLogRepository.Setup(r => r.GetAverageWeeklyProgressAsync(1, 4))
            .ReturnsAsync(-0.75f);

        var result = await _weightPredictionService.GetAverageWeeklyChangeAsync(1, 4);
        Assert.NotNull(result);
        Assert.Equal(-0.75f, result.Value);
    }

    // ========== GetWeightTrendAnalysisAsync Tests ==========


    [Fact]
    public async Task GetWeightTrendAnalysisAsync_WithNoLogs_ShouldReturnRecommendation()
    {
        var user = CreateTestUser(1, 80f);
        var logs = new List<ProgressLog>();

        _mockUserRepository.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockProgressLogRepository.Setup(r => r.GetWeightTrendAsync(1, 20)).ReturnsAsync(logs);

        var result = await _weightPredictionService.GetWeightTrendAnalysisAsync(1);
        Assert.Equal("Start logging your weight to see trends and predictions!", result.Recommendation);
        Assert.Empty(result.WeightHistory);
    }

    [Fact]
    public async Task GetWeightTrendAnalysisAsync_WithStableWeight_ShouldGiveAdjustmentRecommendation()
    {
        var user = CreateTestUser(1, 80f, Goal.FatLoss);
        // وزن ثابت
        var logs = CreateWeeklyWeightLogsDescending(80f, 0f, 4);

        _mockUserRepository.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockProgressLogRepository.Setup(r => r.GetWeightTrendAsync(1, 20)).ReturnsAsync(logs);
        _mockProgressLogRepository.Setup(r => r.GetAverageWeeklyProgressAsync(1, 8)).ReturnsAsync(0f);

        var result = await _weightPredictionService.GetWeightTrendAnalysisAsync(1);
        Assert.Contains("Weight is stable", result.Recommendation);
    }

    [Fact]
    public async Task GetWeightTrendAnalysisAsync_ShouldCalculateCorrectPercentages()
    {
        var user = CreateTestUser(1, 80f);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var logs = CreateWeightLogsDescending(
            (today.AddDays(-28), 100f),
            (today, 80f)
        );

        _mockUserRepository.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockProgressLogRepository.Setup(r => r.GetWeightTrendAsync(1, 20)).ReturnsAsync(logs);
        _mockProgressLogRepository.Setup(r => r.GetAverageWeeklyProgressAsync(1, 8)).ReturnsAsync(-5f);

        var result = await _weightPredictionService.GetWeightTrendAnalysisAsync(1);
        Assert.Equal(100f, result.StartingWeight);
        Assert.Equal(80f, result.CurrentWeight);
        Assert.Equal(-20f, result.TotalChange);
        Assert.Equal(-20, result.TotalChangePercentage, 1);
    }

    [Fact]
    public async Task GetWeightTrendAnalysisAsync_ShouldSetCorrectDataPointsCount()
    {
        var user = CreateTestUser(1, 80f);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var logs = CreateWeightLogsDescending(
            (today.AddDays(-28), 85f),
            (today.AddDays(-21), 84f),
            (today.AddDays(-14), 83f),
            (today.AddDays(-7), 82f),
            (today, 81f)
        );

        _mockUserRepository.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockProgressLogRepository.Setup(r => r.GetWeightTrendAsync(1, 20)).ReturnsAsync(logs);
        _mockProgressLogRepository.Setup(r => r.GetAverageWeeklyProgressAsync(1, 8)).ReturnsAsync(-1f);

        var result = await _weightPredictionService.GetWeightTrendAnalysisAsync(1);
        Assert.Equal(5, result.DataPointsCount);
        Assert.Equal(5, result.WeightHistory.Count);
    }


////////////////////////////////////////
    [Fact]
    public async Task GetPredictionAsync_WithEnoughData_ShouldReturnFullPrediction()
    {
        var user = CreateTestUser(1, 80f);
        // ایجاد لاگ‌های دستی با کاهش 0.5 کیلو در هفته (وزن قدیم‌تر بیشتر)
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var logs = new List<ProgressLog>
        {
            new() { Id = 1, UserId = 1, LogDate = today, Weight = 80f, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, UserId = 1, LogDate = today.AddDays(-7), Weight = 80.5f, CreatedAt = DateTime.UtcNow },
            new() { Id = 3, UserId = 1, LogDate = today.AddDays(-14), Weight = 81f, CreatedAt = DateTime.UtcNow },
            new() { Id = 4, UserId = 1, LogDate = today.AddDays(-21), Weight = 81.5f, CreatedAt = DateTime.UtcNow },
            new() { Id = 5, UserId = 1, LogDate = today.AddDays(-28), Weight = 82f, CreatedAt = DateTime.UtcNow },
            new() { Id = 6, UserId = 1, LogDate = today.AddDays(-35), Weight = 82.5f, CreatedAt = DateTime.UtcNow },
            new() { Id = 7, UserId = 1, LogDate = today.AddDays(-42), Weight = 83f, CreatedAt = DateTime.UtcNow },
        }.OrderByDescending(l => l.LogDate).ToList();

        _mockUserRepository.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockProgressLogRepository.Setup(r => r.GetWeightTrendAsync(1, 15)).ReturnsAsync(logs);

        var result = await _weightPredictionService.GetPredictionAsync(1);

        Assert.Equal(80f, result.CurrentWeight);
        Assert.Equal(-0.5f, result.AverageWeeklyChange, 1);
        Assert.Equal("Losing", result.Trend);
        Assert.NotNull(result.PredictedWeight7Days);
        Assert.NotNull(result.PredictedWeight30Days);
        Assert.NotNull(result.PredictedWeight90Days);
    }

    [Fact]
    public async Task GetPredictionAsync_ForWeightLossGoal_ShouldReturnAppropriateMessage()
    {
        var user = CreateTestUser(1, 80f, Goal.FatLoss);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var logs = new List<ProgressLog>
        {
            new() { Id = 1, UserId = 1, LogDate = today, Weight = 80f, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, UserId = 1, LogDate = today.AddDays(-7), Weight = 80.5f, CreatedAt = DateTime.UtcNow },
            new() { Id = 3, UserId = 1, LogDate = today.AddDays(-14), Weight = 81f, CreatedAt = DateTime.UtcNow },
        }.OrderByDescending(l => l.LogDate).ToList();

        _mockUserRepository.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockProgressLogRepository.Setup(r => r.GetWeightTrendAsync(1, 15)).ReturnsAsync(logs);

        var result = await _weightPredictionService.GetPredictionAsync(1);
        Assert.Contains("Great progress", result.Message);
    }

    [Fact]
    public async Task GetPredictionAsync_ForMuscleGainGoal_ShouldReturnAppropriateMessage()
    {
        var user = CreateTestUser(1, 75f, Goal.MuscleGain);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var logs = new List<ProgressLog>
        {
            new() { Id = 1, UserId = 1, LogDate = today, Weight = 75f, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, UserId = 1, LogDate = today.AddDays(-7), Weight = 74.7f, CreatedAt = DateTime.UtcNow },
            new() { Id = 3, UserId = 1, LogDate = today.AddDays(-14), Weight = 74.4f, CreatedAt = DateTime.UtcNow },
        }.OrderByDescending(l => l.LogDate).ToList();

        _mockUserRepository.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockProgressLogRepository.Setup(r => r.GetWeightTrendAsync(1, 15)).ReturnsAsync(logs);

        var result = await _weightPredictionService.GetPredictionAsync(1);
        Assert.Contains("Good progress", result.Message);
    }

    [Fact]
    public async Task GetWeightTrendAnalysisAsync_WithEnoughData_ShouldReturnFullAnalysis()
    {
        var user = CreateTestUser(1, 81f, Goal.FatLoss);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var logs = new List<ProgressLog>
        {
            new() { Id = 1, UserId = 1, LogDate = today, Weight = 81f, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, UserId = 1, LogDate = today.AddDays(-7), Weight = 81.5f, CreatedAt = DateTime.UtcNow },
            new() { Id = 3, UserId = 1, LogDate = today.AddDays(-14), Weight = 82f, CreatedAt = DateTime.UtcNow },
            new() { Id = 4, UserId = 1, LogDate = today.AddDays(-21), Weight = 82.5f, CreatedAt = DateTime.UtcNow },
            new() { Id = 5, UserId = 1, LogDate = today.AddDays(-28), Weight = 83f, CreatedAt = DateTime.UtcNow },
            new() { Id = 6, UserId = 1, LogDate = today.AddDays(-35), Weight = 83.5f, CreatedAt = DateTime.UtcNow },
            new() { Id = 7, UserId = 1, LogDate = today.AddDays(-42), Weight = 84f, CreatedAt = DateTime.UtcNow },
            new() { Id = 8, UserId = 1, LogDate = today.AddDays(-49), Weight = 84.5f, CreatedAt = DateTime.UtcNow },
            new() { Id = 9, UserId = 1, LogDate = today.AddDays(-56), Weight = 85f, CreatedAt = DateTime.UtcNow },
        }.OrderByDescending(l => l.LogDate).ToList();

        _mockUserRepository.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockProgressLogRepository.Setup(r => r.GetWeightTrendAsync(1, 20)).ReturnsAsync(logs);
        _mockProgressLogRepository.Setup(r => r.GetAverageWeeklyProgressAsync(1, 8)).ReturnsAsync(-0.5f);

        var result = await _weightPredictionService.GetWeightTrendAnalysisAsync(1);

        Assert.Equal(85f, result.StartingWeight);
        Assert.Equal(81f, result.CurrentWeight, 1);
        Assert.Equal(-4f, result.TotalChange, 1);
        Assert.Equal(-0.5f, result.AverageWeeklyChange, 1);
        Assert.Equal("Losing", result.TrendDirection);
        Assert.NotEmpty(result.WeightHistory);
    }

    [Fact]
    public async Task GetWeightTrendAnalysisAsync_ForWeightLossWithIncrease_ShouldGiveWarning()
    {
        var user = CreateTestUser(1, 80f, Goal.FatLoss);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        // افزایش وزن (بدتر شدن)
        var logs = new List<ProgressLog>
        {
            new() { Id = 1, UserId = 1, LogDate = today, Weight = 80f, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, UserId = 1, LogDate = today.AddDays(-7), Weight = 79.5f, CreatedAt = DateTime.UtcNow },
            new() { Id = 3, UserId = 1, LogDate = today.AddDays(-14), Weight = 79f, CreatedAt = DateTime.UtcNow },
            new() { Id = 4, UserId = 1, LogDate = today.AddDays(-21), Weight = 78.5f, CreatedAt = DateTime.UtcNow },
            new() { Id = 5, UserId = 1, LogDate = today.AddDays(-28), Weight = 78f, CreatedAt = DateTime.UtcNow },
        }.OrderByDescending(l => l.LogDate).ToList();

        _mockUserRepository.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockProgressLogRepository.Setup(r => r.GetWeightTrendAsync(1, 20)).ReturnsAsync(logs);
        _mockProgressLogRepository.Setup(r => r.GetAverageWeeklyProgressAsync(1, 8)).ReturnsAsync(0.3f);

        var result = await _weightPredictionService.GetWeightTrendAnalysisAsync(1);
        Assert.Contains("weight has increased", result.Recommendation);
    }

    [Fact]
    public async Task GetWeightTrendAnalysisAsync_ForMuscleGainWithDecrease_ShouldGiveWarning()
    {
        var user = CreateTestUser(1, 80f, Goal.MuscleGain);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        // کاهش وزن (بدتر شدن برای عضله‌سازی)
        var logs = new List<ProgressLog>
        {
            new() { Id = 1, UserId = 1, LogDate = today, Weight = 80f, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, UserId = 1, LogDate = today.AddDays(-7), Weight = 80.5f, CreatedAt = DateTime.UtcNow },
            new() { Id = 3, UserId = 1, LogDate = today.AddDays(-14), Weight = 81f, CreatedAt = DateTime.UtcNow },
            new() { Id = 4, UserId = 1, LogDate = today.AddDays(-21), Weight = 81.5f, CreatedAt = DateTime.UtcNow },
            new() { Id = 5, UserId = 1, LogDate = today.AddDays(-28), Weight = 82f, CreatedAt = DateTime.UtcNow },
        }.OrderByDescending(l => l.LogDate).ToList();

        _mockUserRepository.Setup(r => r.GetUserWithPersonAsync(1)).ReturnsAsync(user);
        _mockProgressLogRepository.Setup(r => r.GetWeightTrendAsync(1, 20)).ReturnsAsync(logs);
        _mockProgressLogRepository.Setup(r => r.GetAverageWeeklyProgressAsync(1, 8)).ReturnsAsync(-0.3f);

        var result = await _weightPredictionService.GetWeightTrendAnalysisAsync(1);
        Assert.Contains("losing weight but aiming to gain muscle", result.Recommendation);
    }
}
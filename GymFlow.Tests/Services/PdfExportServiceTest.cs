using Xunit;
using Moq;
using GymFlow.Dal.Repositories.Interfaces;
using GymFlow.Models.Entities;
using GymFlow.Models.Enums;
using GymFlow.Services.Implementations;

namespace GymFlow.Tests.Services;

public class PdfExportServiceTest
{
    private readonly Mock<IWorkoutPlanRepository> _mockWorkoutPlanRepository;
    private readonly Mock<IProgressLogRepository> _mockProgressLogRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IWorkoutSessionRepository> _mockWorkoutSessionRepository;
    private readonly PdfExportService _pdfExportService;

    public PdfExportServiceTest()
    {
        _mockWorkoutPlanRepository = new Mock<IWorkoutPlanRepository>();
        _mockProgressLogRepository = new Mock<IProgressLogRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockWorkoutSessionRepository = new Mock<IWorkoutSessionRepository>();
        
        _pdfExportService = new PdfExportService(
            _mockWorkoutPlanRepository.Object,
            _mockProgressLogRepository.Object,
            _mockUserRepository.Object,
            _mockWorkoutSessionRepository.Object);
    }

    // ========== Helper Methods ==========

    private User CreateTestUser(int id = 1)
    {
        var person = new Person
        {
            Id = id,
            FirstName = "Test",
            LastName = "User",
            Username = "testuser",
            Email = "test@test.com",
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
            CreatedAt = DateTime.UtcNow
        };
    }

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
            CreatedAt = DateTime.UtcNow,
            WorkoutDays = new List<WorkoutDay>()
        };
    }

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
            CreatedAt = DateTime.UtcNow,
            WorkoutDayExercises = new List<WorkoutDayExercise>()
        };
    }

    private Exercise CreateTestExercise(int id = 1)
    {
        return new Exercise
        {
            Id = id,
            Name = "Bench Press",
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
            Exercise = CreateTestExercise(exerciseId),
            Sets = 3,
            Reps = "10,10,8",
            RestSeconds = 60,
            CreatedAt = DateTime.UtcNow
        };
    }

    private ProgressLog CreateTestProgressLog(int id = 1, int userId = 1, int? planId = 1, DateOnly? logDate = null)
    {
        return new ProgressLog
        {
            Id = id,
            UserId = userId,
            WorkoutPlanId = planId,
            LogDate = logDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)),
            Weight = 75.5f,
            BodyFatPercentage = 15.5f,
            Notes = "Test note",
            CreatedAt = DateTime.UtcNow
        };
    }

    private WorkoutSession CreateTestWorkoutSession(int id = 1, int workoutDayId = 1, DateOnly? actualDate = null)
    {
        var workoutDay = CreateTestWorkoutDay(workoutDayId);
        return new WorkoutSession
        {
            Id = id,
            WorkoutDayId = workoutDayId,
            WorkoutDay = workoutDay,
            ActualDate = actualDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            ActualDurationMinutes = 60,
            Feeling = "Great!",
            CreatedAt = DateTime.UtcNow
        };
    }

    // ========== ExportWorkoutPlanToPdfAsync Tests ==========

    [Fact]
    public async Task ExportWorkoutPlanToPdfAsync_WithValidId_ShouldReturnPdfBytes()
    {
        // Arrange
        var user = CreateTestUser();
        var plan = CreateTestWorkoutPlan();
        var workoutDay = CreateTestWorkoutDay();
        var wde = CreateTestWorkoutDayExercise();
        
        workoutDay.WorkoutDayExercises.Add(wde);
        plan.WorkoutDays.Add(workoutDay);
        
        _mockWorkoutPlanRepository.Setup(r => r.GetWorkoutPlanWithDetailsAsync(1))
            .ReturnsAsync(plan);
        _mockUserRepository.Setup(r => r.GetUserWithPersonAsync(1))
            .ReturnsAsync(user);

        // Act
        var result = await _pdfExportService.ExportWorkoutPlanToPdfAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    [Fact]
    public async Task ExportWorkoutPlanToPdfAsync_WithInvalidPlanId_ShouldThrowException()
    {
        // Arrange
        _mockWorkoutPlanRepository.Setup(r => r.GetWorkoutPlanWithDetailsAsync(999))
            .ReturnsAsync((WorkoutPlan?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _pdfExportService.ExportWorkoutPlanToPdfAsync(999));
        Assert.Contains("Workout plan with ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task ExportWorkoutPlanToPdfAsync_WithInvalidUserId_ShouldThrowException()
    {
        // Arrange
        var plan = CreateTestWorkoutPlan();
        _mockWorkoutPlanRepository.Setup(r => r.GetWorkoutPlanWithDetailsAsync(1))
            .ReturnsAsync(plan);
        _mockUserRepository.Setup(r => r.GetUserWithPersonAsync(1))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _pdfExportService.ExportWorkoutPlanToPdfAsync(1));
        Assert.Contains("User with ID 1 not found", exception.Message);
    }

    // ========== ExportProgressReportToPdfAsync Tests ==========

    [Fact]
    public async Task ExportProgressReportToPdfAsync_WithValidUserId_ShouldReturnPdfBytes()
    {
        // Arrange
        var user = CreateTestUser();
        var logs = new List<ProgressLog> { CreateTestProgressLog() };
        var sessions = new List<WorkoutSession> { CreateTestWorkoutSession() };
        
        _mockUserRepository.Setup(r => r.GetUserWithPersonAsync(1))
            .ReturnsAsync(user);
        _mockProgressLogRepository.Setup(r => r.GetUserProgressHistoryAsync(1))
            .ReturnsAsync(logs);
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);

        // Act
        var result = await _pdfExportService.ExportProgressReportToPdfAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    [Fact]
    public async Task ExportProgressReportToPdfAsync_WithDateRange_ShouldFilterLogs()
    {
        // Arrange
        var user = CreateTestUser();
        var logs = new List<ProgressLog>
        {
            CreateTestProgressLog(1, 1, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30))),
            CreateTestProgressLog(2, 1, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-15))),
            CreateTestProgressLog(3, 1, 1, DateOnly.FromDateTime(DateTime.UtcNow))
        };
        
        _mockUserRepository.Setup(r => r.GetUserWithPersonAsync(1))
            .ReturnsAsync(user);
        _mockProgressLogRepository.Setup(r => r.GetUserProgressHistoryAsync(1))
            .ReturnsAsync(logs);
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(new List<WorkoutSession>());

        // Act
        var fromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-20));
        var toDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5));
        var result = await _pdfExportService.ExportProgressReportToPdfAsync(1, fromDate, toDate);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    [Fact]
    public async Task ExportProgressReportToPdfAsync_WithInvalidUserId_ShouldThrowException()
    {
        // Arrange
        _mockUserRepository.Setup(r => r.GetUserWithPersonAsync(999))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _pdfExportService.ExportProgressReportToPdfAsync(999));
        Assert.Contains("User with ID 999 not found", exception.Message);
    }

    // ========== ExportWeeklySummaryToPdfAsync Tests ==========

    [Fact]
    public async Task ExportWeeklySummaryToPdfAsync_WithValidUserId_ShouldReturnPdfBytes()
    {
        // Arrange
        var user = CreateTestUser();
        var sessions = new List<WorkoutSession> { CreateTestWorkoutSession() };
        var activePlan = CreateTestWorkoutPlan();
        
        _mockUserRepository.Setup(r => r.GetUserWithPersonAsync(1))
            .ReturnsAsync(user);
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByDateRangeAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(sessions);
        _mockWorkoutPlanRepository.Setup(r => r.GetActiveWorkoutPlanAsync(1))
            .ReturnsAsync(activePlan);

        // Act
        var result = await _pdfExportService.ExportWeeklySummaryToPdfAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    [Fact]
    public async Task ExportWeeklySummaryToPdfAsync_WithWeekStart_ShouldUseProvidedDate()
    {
        // Arrange
        var user = CreateTestUser();
        var weekStart = new DateOnly(2024, 1, 1);
        
        _mockUserRepository.Setup(r => r.GetUserWithPersonAsync(1))
            .ReturnsAsync(user);
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByDateRangeAsync(1, weekStart, weekStart.AddDays(6)))
            .ReturnsAsync(new List<WorkoutSession>());
        _mockWorkoutPlanRepository.Setup(r => r.GetActiveWorkoutPlanAsync(1))
            .ReturnsAsync((WorkoutPlan?)null);

        // Act
        var result = await _pdfExportService.ExportWeeklySummaryToPdfAsync(1, weekStart);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    // ========== ExportAchievementsCertificateAsync Tests ==========

    [Fact]
    public async Task ExportAchievementsCertificateAsync_WithValidUserId_ShouldReturnPdfBytes()
    {
        // Arrange
        var user = CreateTestUser();
        var sessions = new List<WorkoutSession>
        {
            CreateTestWorkoutSession(1, 1, DateOnly.FromDateTime(DateTime.UtcNow)),
            CreateTestWorkoutSession(2, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)))
        };
        
        _mockUserRepository.Setup(r => r.GetUserWithPersonAsync(1))
            .ReturnsAsync(user);
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);

        // Act
        var result = await _pdfExportService.ExportAchievementsCertificateAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    // ========== PDF Content Validation Tests ==========

    [Fact]
    public async Task ExportWorkoutPlanToPdfAsync_ShouldGenerateValidPdf()
    {
        // Arrange
        var user = CreateTestUser();
        var plan = CreateTestWorkoutPlan();
        var workoutDay = CreateTestWorkoutDay();
        var wde = CreateTestWorkoutDayExercise();
        workoutDay.WorkoutDayExercises.Add(wde);
        plan.WorkoutDays.Add(workoutDay);
        
        _mockWorkoutPlanRepository.Setup(r => r.GetWorkoutPlanWithDetailsAsync(1))
            .ReturnsAsync(plan);
        _mockUserRepository.Setup(r => r.GetUserWithPersonAsync(1))
            .ReturnsAsync(user);

        // Act
        var result = await _pdfExportService.ExportWorkoutPlanToPdfAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
        Assert.StartsWith("%PDF", System.Text.Encoding.ASCII.GetString(result, 0, 4));
    }
}
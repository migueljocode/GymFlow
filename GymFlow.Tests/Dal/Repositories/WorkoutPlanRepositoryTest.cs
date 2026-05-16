using Xunit;
using Microsoft.EntityFrameworkCore;
using GymFlow.Dal.Repositories.Implementations;
using GymFlow.Models.Entities;
using GymFlow.Models.Enums;

namespace GymFlow.Tests.Dal.Repositories;

public class WorkoutPlanRepositoryTest : IClassFixture<DbContextFixture>
{
    private readonly DbContextFixture _fixture;
    private static int _uniqueCounter = 0;

    public WorkoutPlanRepositoryTest(DbContextFixture fixture)
    {
        _fixture = fixture;
    }

    // ========== Helper Methods ==========

    private string GetUniqueSuffix()
    {
        _uniqueCounter++;
        return $"{_uniqueCounter}_{Guid.NewGuid():N}";
    }

    private async Task<User> CreateTestUserAsync()
    {
        var personRepo = new PersonRepository(_fixture.DbContextFactory);
        
        var uniqueSuffix = GetUniqueSuffix();
        var person = new Person
        {
            FirstName = "Test",
            LastName = $"User{uniqueSuffix}",
            Username = $"testuser_{uniqueSuffix}",
            Password = "pass123",
            Email = $"testuser_{uniqueSuffix}@test.com",
            Gender = Gender.Male,
            Age = 25,
            Weight = 75f,
            Height = 175f,
            BodyType = BodyType.Fit,
            CreatedAt = DateTime.UtcNow
        };
        
        var addedPerson = await personRepo.AddAsync(person);
        await personRepo.SaveChangesAsync();
        
        var userRepo = new UserRepository(_fixture.DbContextFactory);
        var user = new User
        {
            PersonId = addedPerson.Id,
            Goal = Goal.Fitness,
            EstimatedCaloriesIntake = 2500,
            CreatedAt = DateTime.UtcNow
        };
        
        return await userRepo.AddAsync(user);
    }

    private async Task<WorkoutPlan> CreateTestWorkoutPlanAsync(int userId, int phase = 1, bool isActive = true)
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        var plan = new WorkoutPlan
        {
            UserId = userId,
            Phase = phase,
            SessionsPerWeek = 3,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = null,
            IsActive = isActive,
            Notes = "Test workout plan",
            CreatedAt = DateTime.UtcNow
        };
        return await repo.AddAsync(plan);
    }

    private async Task<WorkoutDay> CreateTestWorkoutDayAsync(int workoutPlanId, DayOfWeek dayOfWeek = DayOfWeek.Monday)
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        var workoutDay = new WorkoutDay
        {
            WorkoutPlanId = workoutPlanId,
            DayOfWeek = dayOfWeek,
            TargetMuscles = MuscleGroup.Chest,
            DurationMinutes = 60,
            Intensity = Intensity.Medium,
            CreatedAt = DateTime.UtcNow
        };
        return await repo.AddAsync(workoutDay);
    }

    private async Task<ProgressLog> CreateTestProgressLogAsync(int userId, int? planId = null, DateOnly? logDate = null)
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        var date = logDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        
        var log = new ProgressLog
        {
            UserId = userId,
            WorkoutPlanId = planId,
            LogDate = date,
            Weight = 75f,
            CreatedAt = DateTime.UtcNow
        };
        return await repo.AddAsync(log);
    }

    // ========== Generic Repository Query Tests ==========

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectWorkoutPlan()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);

        var fetched = await repo.GetByIdAsync(plan.Id);

        Assert.NotNull(fetched);
        Assert.Equal(plan.Id, fetched.Id);
        Assert.Equal(plan.Phase, fetched.Phase);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();

        var fetched = await repo.GetByIdAsync(99999);

        Assert.Null(fetched);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ShouldReturnFirstMatchingWorkoutPlan()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        await CreateTestWorkoutPlanAsync(user.Id, 1);
        await CreateTestWorkoutPlanAsync(user.Id, 2);

        var fetched = await repo.FirstOrDefaultAsync(wp => wp.Phase == 1);

        Assert.NotNull(fetched);
        Assert.Equal(1, fetched.Phase);
    }

    [Fact]
    public async Task SingleOrDefaultAsync_WithSingleMatch_ShouldReturnWorkoutPlan()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);

        var fetched = await repo.SingleOrDefaultAsync(wp => wp.Id == plan.Id);

        Assert.NotNull(fetched);
        Assert.Equal(plan.Id, fetched.Id);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllWorkoutPlans()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        await CreateTestWorkoutPlanAsync(user.Id, 1);
        await CreateTestWorkoutPlanAsync(user.Id, 2);

        var all = await repo.GetAllAsync();

        Assert.NotNull(all);
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task FindAsync_ShouldReturnFilteredWorkoutPlans()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        await CreateTestWorkoutPlanAsync(user.Id, 1);
        await CreateTestWorkoutPlanAsync(user.Id, 2);

        var found = await repo.FindAsync(wp => wp.Phase == 1);

        Assert.Single(found);
        Assert.Equal(1, found.First().Phase);
    }

    [Fact]
    public async Task FindAsync_ById_ShouldReturnWorkoutPlan()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);

        var fetched = await repo.FindAsync(plan.Id);

        Assert.NotNull(fetched);
        Assert.Equal(plan.Id, fetched.Id);
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnTrueWhenExists()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        await CreateTestWorkoutPlanAsync(user.Id);

        var exists = await repo.AnyAsync(wp => wp.UserId == user.Id);

        Assert.True(exists);
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnFalseWhenNotExists()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();

        var exists = await repo.AnyAsync(wp => wp.Id == 99999);

        Assert.False(exists);
    }

    [Fact]
    public async Task AllAsync_ShouldReturnTrueWhenAllMatch()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        await CreateTestWorkoutPlanAsync(user.Id, 1);
        await CreateTestWorkoutPlanAsync(user.Id, 2);

        var allMatch = await repo.AllAsync(wp => wp.UserId == user.Id);

        Assert.True(allMatch);
    }

    [Fact]
    public async Task AllAsync_ShouldReturnFalseWhenNotAllMatch()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user1 = await CreateTestUserAsync();
        var user2 = await CreateTestUserAsync();
        await CreateTestWorkoutPlanAsync(user1.Id);
        await CreateTestWorkoutPlanAsync(user2.Id);

        var allMatch = await repo.AllAsync(wp => wp.UserId == user1.Id);

        Assert.False(allMatch);
    }

    [Fact]
    public async Task CountAsync_ShouldReturnTotalCount()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        await CreateTestWorkoutPlanAsync(user.Id, 1);
        await CreateTestWorkoutPlanAsync(user.Id, 2);

        var count = await repo.CountAsync();

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ShouldReturnFilteredCount()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        await CreateTestWorkoutPlanAsync(user.Id, 1);
        await CreateTestWorkoutPlanAsync(user.Id, 2);

        var count = await repo.CountAsync(wp => wp.Phase == 1);

        Assert.Equal(1, count);
    }

    // ========== Generic Repository Command Tests ==========

    [Fact]
    public async Task AddAsync_ShouldSaveWorkoutPlanToDatabase()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = new WorkoutPlan
        {
            UserId = user.Id,
            Phase = 5,
            SessionsPerWeek = 4,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var added = await repo.AddAsync(plan);

        var fetched = await repo.GetByIdAsync(added.Id);
        Assert.NotNull(fetched);
        Assert.Equal(5, fetched.Phase);
        Assert.Equal(4, fetched.SessionsPerWeek);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingWorkoutPlan()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id, 1);
        
        plan.Phase = 2;
        plan.SessionsPerWeek = 5;

        var updated = await repo.UpdateAsync(plan);

        var fetched = await repo.GetByIdAsync(updated.Id);
        Assert.Equal(2, fetched?.Phase);
        Assert.Equal(5, fetched?.SessionsPerWeek);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteWorkoutPlan()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);

        var deleted = await repo.DeleteAsync(plan);

        Assert.True(deleted);
        
        var fetched = await repo.GetByIdAsync(plan.Id);
        Assert.Null(fetched);
        
        await using var context = _fixture.CreateContext();
        var allPlans = await context.WorkoutPlans.IgnoreQueryFilters().ToListAsync();
        var deletedPlan = allPlans.FirstOrDefault(wp => wp.Id == plan.Id);
        Assert.NotNull(deletedPlan);
        Assert.True(deletedPlan.IsDeleted);
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldSoftDeleteWorkoutPlanById()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);

        var deleted = await repo.DeleteByIdAsync(plan.Id);

        Assert.True(deleted);
        var fetched = await repo.GetByIdAsync(plan.Id);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task DeleteByIdAsync_WithInvalidId_ShouldReturnFalse()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();

        var deleted = await repo.DeleteByIdAsync(99999);

        Assert.False(deleted);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleWorkoutPlans()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plans = new List<WorkoutPlan>
        {
            new() { UserId = user.Id, Phase = 1, SessionsPerWeek = 3, StartDate = DateOnly.FromDateTime(DateTime.UtcNow), CreatedAt = DateTime.UtcNow },
            new() { UserId = user.Id, Phase = 2, SessionsPerWeek = 4, StartDate = DateOnly.FromDateTime(DateTime.UtcNow), CreatedAt = DateTime.UtcNow }
        };

        var added = await repo.AddRangeAsync(plans);

        Assert.Equal(2, added.Count());
        var all = await repo.GetAllAsync();
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task DeleteRangeAsync_ShouldSoftDeleteMultipleWorkoutPlans()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plans = new List<WorkoutPlan>
        {
            new() { UserId = user.Id, Phase = 1, SessionsPerWeek = 3, StartDate = DateOnly.FromDateTime(DateTime.UtcNow), CreatedAt = DateTime.UtcNow },
            new() { UserId = user.Id, Phase = 2, SessionsPerWeek = 4, StartDate = DateOnly.FromDateTime(DateTime.UtcNow), CreatedAt = DateTime.UtcNow }
        };
        var added = await repo.AddRangeAsync(plans);

        var deleted = await repo.DeleteRangeAsync(added);

        Assert.True(deleted);
        var all = await repo.GetAllAsync();
        Assert.Empty(all);
    }

    [Fact]
    public async Task DeleteAllAsync_ShouldSoftDeleteAllWorkoutPlans()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        await CreateTestWorkoutPlanAsync(user.Id, 1);
        await CreateTestWorkoutPlanAsync(user.Id, 2);

        var deleted = await repo.DeleteAllAsync();

        Assert.True(deleted);
        var all = await repo.GetAllAsync();
        Assert.Empty(all);
    }

    // ========== Specific Interface Tests ==========

    [Fact]
    public async Task GetWorkoutPlanWithDetailsAsync_ShouldIncludeDaysAndExercises()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        
        var exerciseRepo = new ExerciseRepository(_fixture.DbContextFactory);
        var exercise = new Exercise
        {
            Name = $"TestExercise_{GetUniqueSuffix()}",
            PrimaryMuscleGroup = MuscleGroup.Chest,
            CreatedAt = DateTime.UtcNow
        };
        var addedExercise = await exerciseRepo.AddAsync(exercise);
        
        var workoutDayRepo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await workoutDayRepo.AddExerciseToDayAsync(new WorkoutDayExercise
        {
            WorkoutDayId = workoutDay.Id,
            ExerciseId = addedExercise.Id,
            Sets = 3,
            Reps = "10,10,8",
            RestSeconds = 60,
            CreatedAt = DateTime.UtcNow
        });

        var fetched = await repo.GetWorkoutPlanWithDetailsAsync(plan.Id);

        Assert.NotNull(fetched);
        Assert.NotNull(fetched.WorkoutDays);
        Assert.NotEmpty(fetched.WorkoutDays);
        
        var firstDay = fetched.WorkoutDays.First();
        Assert.NotNull(firstDay.WorkoutDayExercises);
        Assert.NotEmpty(firstDay.WorkoutDayExercises);
    }

    [Fact]
    public async Task GetWorkoutPlanWithDaysAsync_ShouldIncludeDays()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        await CreateTestWorkoutDayAsync(plan.Id, DayOfWeek.Monday);
        await CreateTestWorkoutDayAsync(plan.Id, DayOfWeek.Tuesday);

        var fetched = await repo.GetWorkoutPlanWithDaysAsync(plan.Id);

        Assert.NotNull(fetched);
        Assert.NotNull(fetched.WorkoutDays);
        Assert.Equal(2, fetched.WorkoutDays.Count);
    }

    [Fact]
    public async Task GetWorkoutPlanWithProgressAsync_ShouldIncludeProgressLogsـSimple()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        await CreateTestProgressLogAsync(user.Id, plan.Id, today.AddDays(-2));
        await CreateTestProgressLogAsync(user.Id, plan.Id, today.AddDays(-1));

        var fetched = await repo.GetWorkoutPlanWithProgressAsync(plan.Id);

        Assert.NotNull(fetched);
        Assert.NotNull(fetched.ProgressLogs);
        Assert.Equal(2, fetched.ProgressLogs.Count);
    }

    [Fact]
    public async Task GetWorkoutPlanWithProgressAsync_ShouldIncludeProgressLogs()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        
        // استفاده از تاریخ‌های متفاوت برای جلوگیری از Unique Constraint
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var yesterday = today.AddDays(-1);
        var twoDaysAgo = today.AddDays(-2);
        
        var progressRepo = new ProgressLogRepository(_fixture.DbContextFactory);
        
        var log1 = new ProgressLog
        {
            UserId = user.Id,
            WorkoutPlanId = plan.Id,
            LogDate = twoDaysAgo,
            Weight = 80f,
            CreatedAt = DateTime.UtcNow
        };
        await progressRepo.AddAsync(log1);
        
        var log2 = new ProgressLog
        {
            UserId = user.Id,
            WorkoutPlanId = plan.Id,
            LogDate = yesterday,
            Weight = 79f,
            CreatedAt = DateTime.UtcNow
        };
        await progressRepo.AddAsync(log2);

        var fetched = await repo.GetWorkoutPlanWithProgressAsync(plan.Id);

        Assert.NotNull(fetched);
        Assert.NotNull(fetched.ProgressLogs);
        Assert.Equal(2, fetched.ProgressLogs.Count);
    }

    [Fact]
    public async Task GetUserWorkoutPlansAsync_ShouldReturnAllPlansForUser()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user1 = await CreateTestUserAsync();
        var user2 = await CreateTestUserAsync();
        
        await CreateTestWorkoutPlanAsync(user1.Id, 1);
        await CreateTestWorkoutPlanAsync(user1.Id, 2);
        await CreateTestWorkoutPlanAsync(user2.Id, 1);

        var plansForUser1 = await repo.GetUserWorkoutPlansAsync(user1.Id);
        var plansForUser2 = await repo.GetUserWorkoutPlansAsync(user2.Id);

        Assert.Equal(2, plansForUser1.Count());
        Assert.Single(plansForUser2);
    }

    [Fact]
    public async Task GetActiveWorkoutPlanAsync_ShouldReturnActivePlan()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        await CreateTestWorkoutPlanAsync(user.Id, 1, false);
        await CreateTestWorkoutPlanAsync(user.Id, 2, true);
        await CreateTestWorkoutPlanAsync(user.Id, 3, false);

        var activePlan = await repo.GetActiveWorkoutPlanAsync(user.Id);

        Assert.NotNull(activePlan);
        Assert.True(activePlan.IsActive);
        Assert.Equal(2, activePlan.Phase);
    }

    [Fact]
    public async Task GetCurrentWorkoutPlanAsync_ShouldReturnPlanBasedOnDate()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        var pastPlan = new WorkoutPlan
        {
            UserId = user.Id,
            Phase = 1,
            SessionsPerWeek = 3,
            StartDate = today.AddDays(-30),
            EndDate = today.AddDays(-15),
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };
        var currentPlan = new WorkoutPlan
        {
            UserId = user.Id,
            Phase = 2,
            SessionsPerWeek = 4,
            StartDate = today.AddDays(-7),
            EndDate = today.AddDays(7),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var futurePlan = new WorkoutPlan
        {
            UserId = user.Id,
            Phase = 3,
            SessionsPerWeek = 5,
            StartDate = today.AddDays(14),
            EndDate = today.AddDays(28),
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };
        
        await repo.AddRangeAsync(new[] { pastPlan, currentPlan, futurePlan });

        var current = await repo.GetCurrentWorkoutPlanAsync(user.Id);

        Assert.NotNull(current);
        Assert.Equal(2, current.Phase);
    }

    [Fact]
    public async Task GetWorkoutPlansByPhaseAsync_ShouldReturnPlansWithSpecificPhase()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        await CreateTestWorkoutPlanAsync(user.Id, 1);
        await CreateTestWorkoutPlanAsync(user.Id, 1);
        await CreateTestWorkoutPlanAsync(user.Id, 2);

        var phase1Plans = await repo.GetWorkoutPlansByPhaseAsync(user.Id, 1);

        Assert.Equal(2, phase1Plans.Count());
        Assert.All(phase1Plans, p => Assert.Equal(1, p.Phase));
    }

    [Fact]
    public async Task DeactivateAllUserPlansAsync_ShouldDeactivateAllActivePlans()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        await CreateTestWorkoutPlanAsync(user.Id, 1, true);
        await CreateTestWorkoutPlanAsync(user.Id, 2, true);
        await CreateTestWorkoutPlanAsync(user.Id, 3, false);

        var result = await repo.DeactivateAllUserPlansAsync(user.Id);

        Assert.True(result);
        
        var plans = await repo.GetUserWorkoutPlansAsync(user.Id);
        Assert.All(plans, p => Assert.False(p.IsActive));
    }

    [Fact]
    public async Task DeactivateAllUserPlansAsync_WithNoActivePlans_ShouldReturnFalse()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        await CreateTestWorkoutPlanAsync(user.Id, 1, false);
        await CreateTestWorkoutPlanAsync(user.Id, 2, false);

        var result = await repo.DeactivateAllUserPlansAsync(user.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task ActivateWorkoutPlanAsync_ShouldActivateSpecificPlan()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id, 1, false);

        var result = await repo.ActivateWorkoutPlanAsync(plan.Id);

        Assert.True(result);
        
        var activatedPlan = await repo.GetByIdAsync(plan.Id);
        Assert.NotNull(activatedPlan);
        Assert.True(activatedPlan.IsActive);
    }

    [Fact]
    public async Task ActivateWorkoutPlanAsync_WithInvalidId_ShouldReturnFalse()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();

        var result = await repo.ActivateWorkoutPlanAsync(99999);

        Assert.False(result);
    }

    [Fact]
    public async Task DeactivateAllAndAddAsync_ShouldDeactivateAllAndAddNewPlan()
    {
        var repo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        
        // ایجاد چند پلن فعال
        await CreateTestWorkoutPlanAsync(user.Id, 1, true);
        await CreateTestWorkoutPlanAsync(user.Id, 2, true);
        
        var newPlan = new WorkoutPlan
        {
            UserId = user.Id,
            Phase = 3,
            SessionsPerWeek = 5,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await repo.DeactivateAllAndAddAsync(newPlan);

        Assert.NotNull(result);
        Assert.Equal(3, result.Phase);
        
        // بررسی اینکه پلن‌های قبلی غیرفعال شده‌اند
        var plans = await repo.GetUserWorkoutPlansAsync(user.Id);
        var activePlans = plans.Where(p => p.IsActive).ToList();
        Assert.Single(activePlans);
        Assert.Equal(3, activePlans.First().Phase);
    }
}
using Xunit;
using Microsoft.EntityFrameworkCore;
using GymFlow.Dal.Repositories.Implementations;
using GymFlow.Models.Entities;
using GymFlow.Models.Enums;

namespace GymFlow.Tests.Dal.Repositories;

public class WorkoutDayRepositoryTest : IClassFixture<DbContextFixture>
{
    private readonly DbContextFixture _fixture;
    private static int _uniqueCounter = 0;

    public WorkoutDayRepositoryTest(DbContextFixture fixture)
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

    private async Task<WorkoutPlan> CreateTestWorkoutPlanAsync(int userId)
    {
        var planRepo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        var plan = new WorkoutPlan
        {
            UserId = userId,
            Phase = 1,
            SessionsPerWeek = 3,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedAt = DateTime.UtcNow
        };
        return await planRepo.AddAsync(plan);
    }

    private async Task<WorkoutDay> CreateTestWorkoutDayAsync(int workoutPlanId, DayOfWeek dayOfWeek = DayOfWeek.Monday)
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        
        // بررسی اینکه آیا برای این پلن و این روز قبلاً workout day وجود دارد
        var existing = await repo.GetWorkoutDayByWeekdayAndPlanAsync(workoutPlanId, dayOfWeek);
        if (existing != null)
        {
            // اگر وجود داشت، همان را برگردان (یا می‌توانید حذف کنید و دوباره بسازید)
            return existing;
        }
        
        var workoutDay = new WorkoutDay
        {
            WorkoutPlanId = workoutPlanId,
            DayOfWeek = dayOfWeek,
            TargetMuscles = MuscleGroup.Chest,
            DurationMinutes = 60,
            Intensity = Intensity.Medium,
            Notes = "Test workout day",
            CreatedAt = DateTime.UtcNow
        };
        return await repo.AddAsync(workoutDay);
    }

    private async Task<Exercise> CreateTestExerciseAsync()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        var exercise = new Exercise
        {
            Name = $"TestExercise_{GetUniqueSuffix()}",
            PrimaryMuscleGroup = MuscleGroup.Chest,
            Description = "Test description",
            CreatedAt = DateTime.UtcNow
        };
        return await repo.AddAsync(exercise);
    }

    // ========== Generic Repository Query Tests ==========

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectWorkoutDay()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);

        var fetched = await repo.GetByIdAsync(workoutDay.Id);

        Assert.NotNull(fetched);
        Assert.Equal(workoutDay.Id, fetched.Id);
        Assert.Equal(workoutDay.DayOfWeek, fetched.DayOfWeek);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();

        var fetched = await repo.GetByIdAsync(99999);

        Assert.Null(fetched);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ShouldReturnFirstMatchingWorkoutDay()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        await CreateTestWorkoutDayAsync(plan.Id, DayOfWeek.Monday);
        await CreateTestWorkoutDayAsync(plan.Id, DayOfWeek.Tuesday);

        var fetched = await repo.FirstOrDefaultAsync(wd => wd.DayOfWeek == DayOfWeek.Monday);

        Assert.NotNull(fetched);
        Assert.Equal(DayOfWeek.Monday, fetched.DayOfWeek);
    }

    [Fact]
    public async Task SingleOrDefaultAsync_WithSingleMatch_ShouldReturnWorkoutDay()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);

        var fetched = await repo.SingleOrDefaultAsync(wd => wd.Id == workoutDay.Id);

        Assert.NotNull(fetched);
        Assert.Equal(workoutDay.Id, fetched.Id);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllWorkoutDays()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        await CreateTestWorkoutDayAsync(plan.Id, DayOfWeek.Monday);
        await CreateTestWorkoutDayAsync(plan.Id, DayOfWeek.Tuesday);

        var all = await repo.GetAllAsync();

        Assert.NotNull(all);
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task FindAsync_ShouldReturnFilteredWorkoutDays()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        await CreateTestWorkoutDayAsync(plan.Id, DayOfWeek.Monday);
        await CreateTestWorkoutDayAsync(plan.Id, DayOfWeek.Tuesday);

        var found = await repo.FindAsync(wd => wd.DayOfWeek == DayOfWeek.Monday);

        Assert.Single(found);
        Assert.Equal(DayOfWeek.Monday, found.First().DayOfWeek);
    }

    [Fact]
    public async Task FindAsync_ById_ShouldReturnWorkoutDay()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);

        var fetched = await repo.FindAsync(workoutDay.Id);

        Assert.NotNull(fetched);
        Assert.Equal(workoutDay.Id, fetched.Id);
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnTrueWhenExists()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        await CreateTestWorkoutDayAsync(plan.Id);

        var exists = await repo.AnyAsync(wd => wd.WorkoutPlanId == plan.Id);

        Assert.True(exists);
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnFalseWhenNotExists()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();

        var exists = await repo.AnyAsync(wd => wd.Id == 99999);

        Assert.False(exists);
    }

    [Fact]
    public async Task AllAsync_ShouldReturnTrueWhenAllMatch()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        await CreateTestWorkoutDayAsync(plan.Id, DayOfWeek.Monday);
        await CreateTestWorkoutDayAsync(plan.Id, DayOfWeek.Tuesday);

        var allMatch = await repo.AllAsync(wd => wd.WorkoutPlanId == plan.Id);

        Assert.True(allMatch);
    }

    [Fact]
    public async Task AllAsync_ShouldReturnFalseWhenNotAllMatch()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user1 = await CreateTestUserAsync();
        var user2 = await CreateTestUserAsync();
        var plan1 = await CreateTestWorkoutPlanAsync(user1.Id);
        var plan2 = await CreateTestWorkoutPlanAsync(user2.Id);
        await CreateTestWorkoutDayAsync(plan1.Id);
        await CreateTestWorkoutDayAsync(plan2.Id);

        var allMatch = await repo.AllAsync(wd => wd.WorkoutPlanId == plan1.Id);

        Assert.False(allMatch);
    }

    [Fact]
    public async Task CountAsync_ShouldReturnTotalCount()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        await CreateTestWorkoutDayAsync(plan.Id, DayOfWeek.Monday);
        await CreateTestWorkoutDayAsync(plan.Id, DayOfWeek.Tuesday);

        var count = await repo.CountAsync();

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ShouldReturnFilteredCount()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        await CreateTestWorkoutDayAsync(plan.Id, DayOfWeek.Monday);
        await CreateTestWorkoutDayAsync(plan.Id, DayOfWeek.Tuesday);

        var count = await repo.CountAsync(wd => wd.DayOfWeek == DayOfWeek.Monday);

        Assert.Equal(1, count);
    }

    // ========== Generic Repository Command Tests ==========

    [Fact]
    public async Task AddAsync_ShouldSaveWorkoutDayToDatabase()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        
        var workoutDay = new WorkoutDay
        {
            WorkoutPlanId = plan.Id,
            DayOfWeek = DayOfWeek.Wednesday,
            TargetMuscles = MuscleGroup.Back,
            DurationMinutes = 75,
            Intensity = Intensity.High,
            Notes = "New workout day",
            CreatedAt = DateTime.UtcNow
        };

        var added = await repo.AddAsync(workoutDay);

        var fetched = await repo.GetByIdAsync(added.Id);
        Assert.NotNull(fetched);
        Assert.Equal(DayOfWeek.Wednesday, fetched.DayOfWeek);
        Assert.Equal(75, fetched.DurationMinutes);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingWorkoutDay()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        
        workoutDay.DurationMinutes = 90;
        workoutDay.Intensity = Intensity.High;

        var updated = await repo.UpdateAsync(workoutDay);

        var fetched = await repo.GetByIdAsync(updated.Id);
        Assert.Equal(90, fetched?.DurationMinutes);
        Assert.Equal(Intensity.High, fetched?.Intensity);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteWorkoutDay()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);

        var deleted = await repo.DeleteAsync(workoutDay);

        Assert.True(deleted);
        
        var fetched = await repo.GetByIdAsync(workoutDay.Id);
        Assert.Null(fetched);
        
        await using var context = _fixture.CreateContext();
        var allWorkoutDays = await context.WorkoutDays.IgnoreQueryFilters().ToListAsync();
        var deletedWorkoutDay = allWorkoutDays.FirstOrDefault(wd => wd.Id == workoutDay.Id);
        Assert.NotNull(deletedWorkoutDay);
        Assert.True(deletedWorkoutDay.IsDeleted);
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldSoftDeleteWorkoutDayById()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);

        var deleted = await repo.DeleteByIdAsync(workoutDay.Id);

        Assert.True(deleted);
        var fetched = await repo.GetByIdAsync(workoutDay.Id);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task DeleteByIdAsync_WithInvalidId_ShouldReturnFalse()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();

        var deleted = await repo.DeleteByIdAsync(99999);

        Assert.False(deleted);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleWorkoutDays()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        
        var workoutDays = new List<WorkoutDay>
        {
            new() { WorkoutPlanId = plan.Id, DayOfWeek = DayOfWeek.Monday, TargetMuscles = MuscleGroup.Chest, DurationMinutes = 60, Intensity = Intensity.Medium, CreatedAt = DateTime.UtcNow },
            new() { WorkoutPlanId = plan.Id, DayOfWeek = DayOfWeek.Tuesday, TargetMuscles = MuscleGroup.Back, DurationMinutes = 60, Intensity = Intensity.Medium, CreatedAt = DateTime.UtcNow }
        };

        var added = await repo.AddRangeAsync(workoutDays);

        Assert.Equal(2, added.Count());
        var all = await repo.GetAllAsync();
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task DeleteRangeAsync_ShouldSoftDeleteMultipleWorkoutDays()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        
        var workoutDays = new List<WorkoutDay>
        {
            new() { WorkoutPlanId = plan.Id, DayOfWeek = DayOfWeek.Monday, TargetMuscles = MuscleGroup.Chest, DurationMinutes = 60, Intensity = Intensity.Medium, CreatedAt = DateTime.UtcNow },
            new() { WorkoutPlanId = plan.Id, DayOfWeek = DayOfWeek.Tuesday, TargetMuscles = MuscleGroup.Back, DurationMinutes = 60, Intensity = Intensity.Medium, CreatedAt = DateTime.UtcNow }
        };
        var added = await repo.AddRangeAsync(workoutDays);

        var deleted = await repo.DeleteRangeAsync(added);

        Assert.True(deleted);
        var all = await repo.GetAllAsync();
        Assert.Empty(all);
    }

    [Fact]
    public async Task DeleteAllAsync_ShouldSoftDeleteAllWorkoutDays()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        await CreateTestWorkoutDayAsync(plan.Id, DayOfWeek.Monday);
        await CreateTestWorkoutDayAsync(plan.Id, DayOfWeek.Tuesday);

        var deleted = await repo.DeleteAllAsync();

        Assert.True(deleted);
        var all = await repo.GetAllAsync();
        Assert.Empty(all);
    }

    // ========== Specific Interface Tests ==========

    [Fact]
    public async Task GetWorkoutDaysByPlanAsync_ShouldReturnDaysForSpecificPlan()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan1 = await CreateTestWorkoutPlanAsync(user.Id);
        var plan2 = await CreateTestWorkoutPlanAsync(user.Id);
        
        await CreateTestWorkoutDayAsync(plan1.Id, DayOfWeek.Monday);
        await CreateTestWorkoutDayAsync(plan1.Id, DayOfWeek.Tuesday);
        await CreateTestWorkoutDayAsync(plan2.Id, DayOfWeek.Wednesday);

        var daysForPlan1 = await repo.GetWorkoutDaysByPlanAsync(plan1.Id);
        var daysForPlan2 = await repo.GetWorkoutDaysByPlanAsync(plan2.Id);

        Assert.Equal(2, daysForPlan1.Count());
        Assert.Single(daysForPlan2);
    }

    [Fact]
    public async Task GetWorkoutDayWithExercisesAsync_ShouldIncludeExercises()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        
        var exercise = await CreateTestExerciseAsync();
        await repo.AddExerciseToDayAsync(new WorkoutDayExercise
        {
            WorkoutDayId = workoutDay.Id,
            ExerciseId = exercise.Id,
            Sets = 3,
            Reps = "10,10,8",
            RestSeconds = 60,
            CreatedAt = DateTime.UtcNow
        });

        var fetched = await repo.GetWorkoutDayWithExercisesAsync(workoutDay.Id);

        Assert.NotNull(fetched);
        Assert.NotNull(fetched.WorkoutDayExercises);
        Assert.NotEmpty(fetched.WorkoutDayExercises);
    }

    [Fact]
    public async Task GetWorkoutDayWithSessionsAsync_ShouldIncludeSessions()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        
        var sessionRepo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        var session = new WorkoutSession
        {
            WorkoutDayId = workoutDay.Id,
            ActualDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ActualDurationMinutes = 60,
            CreatedAt = DateTime.UtcNow
        };
        await sessionRepo.AddAsync(session);

        var fetched = await repo.GetWorkoutDayWithSessionsAsync(workoutDay.Id);

        Assert.NotNull(fetched);
        Assert.NotNull(fetched.WorkoutSessions);
        Assert.NotEmpty(fetched.WorkoutSessions);
    }

    [Fact]
    public async Task GetWorkoutDaysByWeekdayAsync_ShouldReturnDaysForSpecificWeekday()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        
        // ایجاد دو پلن متفاوت
        var plan1 = await CreateTestWorkoutPlanAsync(user.Id);
        var plan2 = await CreateTestWorkoutPlanAsync(user.Id);
        
        // اضافه کردن مستقیم به repository
        var monday1 = new WorkoutDay
        {
            WorkoutPlanId = plan1.Id,
            DayOfWeek = DayOfWeek.Monday,
            TargetMuscles = MuscleGroup.Chest,
            DurationMinutes = 60,
            Intensity = Intensity.Medium,
            CreatedAt = DateTime.UtcNow
        };
        await repo.AddAsync(monday1);
        
        var monday2 = new WorkoutDay
        {
            WorkoutPlanId = plan2.Id,
            DayOfWeek = DayOfWeek.Monday,
            TargetMuscles = MuscleGroup.Chest,
            DurationMinutes = 60,
            Intensity = Intensity.Medium,
            CreatedAt = DateTime.UtcNow
        };
        await repo.AddAsync(monday2);
        
        var tuesday = new WorkoutDay
        {
            WorkoutPlanId = plan1.Id,
            DayOfWeek = DayOfWeek.Tuesday,
            TargetMuscles = MuscleGroup.Back,
            DurationMinutes = 60,
            Intensity = Intensity.Medium,
            CreatedAt = DateTime.UtcNow
        };
        await repo.AddAsync(tuesday);

        var mondayWorkouts = await repo.GetWorkoutDaysByWeekdayAsync(DayOfWeek.Monday);

        Assert.Equal(2, mondayWorkouts.Count());
        Assert.All(mondayWorkouts, wd => Assert.Equal(DayOfWeek.Monday, wd.DayOfWeek));
    }

    [Fact]
    public async Task GetWorkoutDayByWeekdayAndPlanAsync_ShouldReturnCorrectDay()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        
        await CreateTestWorkoutDayAsync(plan.Id, DayOfWeek.Monday);
        await CreateTestWorkoutDayAsync(plan.Id, DayOfWeek.Wednesday);

        var mondayWorkout = await repo.GetWorkoutDayByWeekdayAndPlanAsync(plan.Id, DayOfWeek.Monday);
        var tuesdayWorkout = await repo.GetWorkoutDayByWeekdayAndPlanAsync(plan.Id, DayOfWeek.Tuesday);

        Assert.NotNull(mondayWorkout);
        Assert.Equal(DayOfWeek.Monday, mondayWorkout.DayOfWeek);
        Assert.Null(tuesdayWorkout);
    }

    [Fact]
    public async Task GetTotalExercisesCountAsync_ShouldReturnCorrectCount()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        
        var exercise1 = await CreateTestExerciseAsync();
        var exercise2 = await CreateTestExerciseAsync();
        
        await repo.AddExerciseToDayAsync(new WorkoutDayExercise { WorkoutDayId = workoutDay.Id, ExerciseId = exercise1.Id, Sets = 3, Reps = "10", RestSeconds = 60, CreatedAt = DateTime.UtcNow });
        await repo.AddExerciseToDayAsync(new WorkoutDayExercise { WorkoutDayId = workoutDay.Id, ExerciseId = exercise2.Id, Sets = 3, Reps = "10", RestSeconds = 60, CreatedAt = DateTime.UtcNow });

        var count = await repo.GetTotalExercisesCountAsync(workoutDay.Id);

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task AddExerciseToDayAsync_ShouldAddExerciseToWorkoutDay()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        var exercise = await CreateTestExerciseAsync();

        var workoutDayExercise = new WorkoutDayExercise
        {
            WorkoutDayId = workoutDay.Id,
            ExerciseId = exercise.Id,
            Sets = 4,
            Reps = "12,10,8,8",
            RestSeconds = 90,
            Notes = "Test exercise",
            CreatedAt = DateTime.UtcNow
        };

        var result = await repo.AddExerciseToDayAsync(workoutDayExercise);

        Assert.True(result);
        
        var exercises = await repo.GetExercisesByDayIdAsync(workoutDay.Id);
        Assert.Single(exercises);
        Assert.Equal(exercise.Id, exercises.First().ExerciseId);
    }

    [Fact]
    public async Task GetExercisesByDayIdAsync_ShouldReturnAllExercisesForDay()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        
        var exercise1 = await CreateTestExerciseAsync();
        var exercise2 = await CreateTestExerciseAsync();
        
        await repo.AddExerciseToDayAsync(new WorkoutDayExercise { WorkoutDayId = workoutDay.Id, ExerciseId = exercise1.Id, Sets = 3, Reps = "10", RestSeconds = 60, CreatedAt = DateTime.UtcNow });
        await repo.AddExerciseToDayAsync(new WorkoutDayExercise { WorkoutDayId = workoutDay.Id, ExerciseId = exercise2.Id, Sets = 3, Reps = "10", RestSeconds = 60, CreatedAt = DateTime.UtcNow });

        var exercises = await repo.GetExercisesByDayIdAsync(workoutDay.Id);

        Assert.Equal(2, exercises.Count);
        Assert.Contains(exercises, e => e.ExerciseId == exercise1.Id);
        Assert.Contains(exercises, e => e.ExerciseId == exercise2.Id);
    }

    [Fact]
    public async Task GetExerciseByIdAsync_ShouldReturnCorrectExercise()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        var exercise = await CreateTestExerciseAsync();

        var workoutDayExercise = new WorkoutDayExercise
        {
            WorkoutDayId = workoutDay.Id,
            ExerciseId = exercise.Id,
            Sets = 3,
            Reps = "10,10,8",
            RestSeconds = 60,
            CreatedAt = DateTime.UtcNow
        };
        await repo.AddExerciseToDayAsync(workoutDayExercise);
        
        var exercises = await repo.GetExercisesByDayIdAsync(workoutDay.Id);
        var savedExercise = exercises.First();

        var fetched = await repo.GetExerciseByIdAsync(savedExercise.Id);

        Assert.NotNull(fetched);
        Assert.Equal(savedExercise.Id, fetched.Id);
        Assert.Equal(3, fetched.Sets);
    }

    [Fact]
    public async Task UpdateExerciseAsync_ShouldModifyExistingExercise()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        var exercise = await CreateTestExerciseAsync();

        var workoutDayExercise = new WorkoutDayExercise
        {
            WorkoutDayId = workoutDay.Id,
            ExerciseId = exercise.Id,
            Sets = 3,
            Reps = "10,10,8",
            RestSeconds = 60,
            CreatedAt = DateTime.UtcNow
        };
        await repo.AddExerciseToDayAsync(workoutDayExercise);
        
        var exercises = await repo.GetExercisesByDayIdAsync(workoutDay.Id);
        var savedExercise = exercises.First();
        
        savedExercise.Sets = 5;
        savedExercise.Reps = "12,12,10,10,8";
        savedExercise.RestSeconds = 120;

        var updated = await repo.UpdateExerciseAsync(savedExercise);

        Assert.Equal(5, updated.Sets);
        Assert.Equal("12,12,10,10,8", updated.Reps);
        Assert.Equal(120, updated.RestSeconds);
    }

    [Fact]
    public async Task DeleteExerciseAsync_ShouldRemoveExerciseFromWorkoutDay()
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        var exercise = await CreateTestExerciseAsync();

        var workoutDayExercise = new WorkoutDayExercise
        {
            WorkoutDayId = workoutDay.Id,
            ExerciseId = exercise.Id,
            Sets = 3,
            Reps = "10,10,8",
            RestSeconds = 60,
            CreatedAt = DateTime.UtcNow
        };
        await repo.AddExerciseToDayAsync(workoutDayExercise);
        
        var exercises = await repo.GetExercisesByDayIdAsync(workoutDay.Id);
        var savedExercise = exercises.First();

        var deleted = await repo.DeleteExerciseAsync(savedExercise);

        Assert.True(deleted);
        
        var remainingExercises = await repo.GetExercisesByDayIdAsync(workoutDay.Id);
        Assert.Empty(remainingExercises);
    }
}
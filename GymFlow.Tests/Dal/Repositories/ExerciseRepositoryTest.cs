using Xunit;
using Microsoft.EntityFrameworkCore;
using GymFlow.Dal.Repositories.Implementations;
using GymFlow.Models.Entities;
using GymFlow.Models.Enums;

namespace GymFlow.Tests.Dal.Repositories;

public class ExerciseRepositoryTest : IClassFixture<DbContextFixture>
{
    private readonly DbContextFixture _fixture;
    private static int _uniqueCounter = 0;

    public ExerciseRepositoryTest(DbContextFixture fixture)
    {
        _fixture = fixture;
    }

    // ========== Helper Methods ==========
    
    private string GetUniqueName(string baseName)
    {
        _uniqueCounter++;
        return $"{baseName}_{_uniqueCounter}_{Guid.NewGuid():N}";
    }

    private async Task<Exercise> CreateTestExerciseAsync(string name, MuscleGroup muscleGroup = MuscleGroup.Chest)
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        var uniqueName = GetUniqueName(name);
        var exercise = new Exercise
        {
            Name = uniqueName,
            PrimaryMuscleGroup = muscleGroup,
            Description = $"Description for {uniqueName}",
            CreatedAt = DateTime.UtcNow
        };
        return await repo.AddAsync(exercise);
    }

    private async Task<User> CreateTestUserAsync(string uniqueSuffix)
    {
        var personRepo = new PersonRepository(_fixture.DbContextFactory);
        
        var uniqueUsername = $"testuser_{uniqueSuffix}_{Guid.NewGuid():N}";
        var person = new Person
        {
            FirstName = "Test",
            LastName = $"User{uniqueSuffix}",
            Username = uniqueUsername,
            Password = "pass123",
            Email = $"{uniqueUsername}@test.com",
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

    private async Task<WorkoutDay> CreateTestWorkoutDayAsync()
    {
        var user = await CreateTestUserAsync(Guid.NewGuid().ToString());
        
        await using var context = _fixture.CreateContext();
        
        var workoutPlan = new WorkoutPlan
        {
            UserId = user.Id,
            Phase = 1,
            SessionsPerWeek = 3,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedAt = DateTime.UtcNow
        };
        context.WorkoutPlans.Add(workoutPlan);
        await context.SaveChangesAsync();
        
        var workoutDay = new WorkoutDay
        {
            WorkoutPlanId = workoutPlan.Id,
            DayOfWeek = DayOfWeek.Monday,
            TargetMuscles = MuscleGroup.Chest,
            DurationMinutes = 60,
            Intensity = Intensity.Medium,
            CreatedAt = DateTime.UtcNow
        };
        context.WorkoutDays.Add(workoutDay);
        await context.SaveChangesAsync();
        
        return workoutDay;
    }

    private async Task<WorkoutDayExercise?> CreateWorkoutDayExerciseAsync(int workoutDayId, int exerciseId)
    {
        await using var context = _fixture.CreateContext();
        
        // بررسی وجود رکورد تکراری
        var exists = await context.WorkoutDayExercises
            .AnyAsync(wde => wde.WorkoutDayId == workoutDayId && wde.ExerciseId == exerciseId);
        
        if (exists)
            return null;
        
        var wde = new WorkoutDayExercise
        {
            WorkoutDayId = workoutDayId,
            ExerciseId = exerciseId,
            Sets = 3,
            Reps = "10,10,8",
            RestSeconds = 60,
            CreatedAt = DateTime.UtcNow
        };
        context.WorkoutDayExercises.Add(wde);
        await context.SaveChangesAsync();
        return wde;
    }

    // ========== Query Tests ==========

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectExercise()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        var exercise = await CreateTestExerciseAsync("Bench Press");

        var fetched = await repo.GetByIdAsync(exercise.Id);

        Assert.NotNull(fetched);
        Assert.Equal(exercise.Name, fetched.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();

        var fetched = await repo.GetByIdAsync(99999);

        Assert.Null(fetched);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ShouldReturnFirstMatchingExercise()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        await CreateTestExerciseAsync("Squat", MuscleGroup.Legs);
        await CreateTestExerciseAsync("Deadlift", MuscleGroup.Back);

        var fetched = await repo.FirstOrDefaultAsync(e => e.PrimaryMuscleGroup == MuscleGroup.Legs);

        Assert.NotNull(fetched);
        Assert.Contains("Squat", fetched.Name);
    }

    [Fact]
    public async Task SingleOrDefaultAsync_WithSingleMatch_ShouldReturnExercise()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        var exercise = await CreateTestExerciseAsync("UniqueExercise");

        var fetched = await repo.SingleOrDefaultAsync(e => e.Id == exercise.Id);

        Assert.NotNull(fetched);
        Assert.Equal(exercise.Id, fetched.Id);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllExercises()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        await CreateTestExerciseAsync("Exercise1");
        await CreateTestExerciseAsync("Exercise2");

        var all = await repo.GetAllAsync();

        Assert.NotNull(all);
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task FindAsync_ShouldReturnFilteredExercises()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        await CreateTestExerciseAsync("Chest Press", MuscleGroup.Chest);
        await CreateTestExerciseAsync("Leg Press", MuscleGroup.Legs);

        var found = await repo.FindAsync(e => e.PrimaryMuscleGroup == MuscleGroup.Chest);

        Assert.Single(found);
        Assert.Contains("Chest Press", found.First().Name);
    }

    [Fact]
    public async Task FindAsync_ById_ShouldReturnExercise()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        var exercise = await CreateTestExerciseAsync("FindById");

        var fetched = await repo.FindAsync(exercise.Id);

        Assert.NotNull(fetched);
        Assert.Equal(exercise.Id, fetched.Id);
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnTrueWhenExists()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        var exercise = await CreateTestExerciseAsync("AnyExercise");

        var exists = await repo.AnyAsync(e => e.Id == exercise.Id);

        Assert.True(exists);
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnFalseWhenNotExists()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();

        var exists = await repo.AnyAsync(e => e.Id == 99999);

        Assert.False(exists);
    }

    [Fact]
    public async Task AllAsync_ShouldReturnTrueWhenAllMatch()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        await CreateTestExerciseAsync("Push1", MuscleGroup.Chest);
        await CreateTestExerciseAsync("Push2", MuscleGroup.Chest);

        var allMatch = await repo.AllAsync(e => e.PrimaryMuscleGroup == MuscleGroup.Chest);

        Assert.True(allMatch);
    }

    [Fact]
    public async Task AllAsync_ShouldReturnFalseWhenNotAllMatch()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        await CreateTestExerciseAsync("Chest Exercise", MuscleGroup.Chest);
        await CreateTestExerciseAsync("Back Exercise", MuscleGroup.Back);

        var allMatch = await repo.AllAsync(e => e.PrimaryMuscleGroup == MuscleGroup.Chest);

        Assert.False(allMatch);
    }

    [Fact]
    public async Task CountAsync_ShouldReturnTotalCount()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        await CreateTestExerciseAsync("Count1");
        await CreateTestExerciseAsync("Count2");

        var count = await repo.CountAsync();

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ShouldReturnFilteredCount()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        await CreateTestExerciseAsync("Leg1", MuscleGroup.Legs);
        await CreateTestExerciseAsync("Leg2", MuscleGroup.Legs);
        await CreateTestExerciseAsync("Arm1", MuscleGroup.Arms);

        var count = await repo.CountAsync(e => e.PrimaryMuscleGroup == MuscleGroup.Legs);

        Assert.Equal(2, count);
    }

    // ========== Command Tests ==========

    [Fact]
    public async Task AddAsync_ShouldSaveExerciseToDatabase()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        var exercise = new Exercise
        {
            Name = GetUniqueName("New Exercise"),
            PrimaryMuscleGroup = MuscleGroup.Shoulders,
            Description = "Shoulder exercise",
            CreatedAt = DateTime.UtcNow
        };

        var added = await repo.AddAsync(exercise);

        var fetched = await repo.GetByIdAsync(added.Id);
        Assert.NotNull(fetched);
        Assert.Equal(exercise.Name, fetched.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingExercise()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        var exercise = await CreateTestExerciseAsync("Original Name");
        var newName = GetUniqueName("Updated Name");
        exercise.Name = newName;

        var updated = await repo.UpdateAsync(exercise);

        var fetched = await repo.GetByIdAsync(updated.Id);
        Assert.Equal(newName, fetched?.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteExercise()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        var exercise = await CreateTestExerciseAsync("ToDelete");

        var deleted = await repo.DeleteAsync(exercise);

        Assert.True(deleted);
        
        var fetched = await repo.GetByIdAsync(exercise.Id);
        Assert.Null(fetched);
        
        await using var context = _fixture.CreateContext();
        var allExercises = await context.Exercises.IgnoreQueryFilters().ToListAsync();
        var deletedExercise = allExercises.FirstOrDefault(e => e.Id == exercise.Id);
        Assert.NotNull(deletedExercise);
        Assert.True(deletedExercise.IsDeleted);
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldSoftDeleteExerciseById()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        var exercise = await CreateTestExerciseAsync("DeleteById");

        var deleted = await repo.DeleteByIdAsync(exercise.Id);

        Assert.True(deleted);
        var fetched = await repo.GetByIdAsync(exercise.Id);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task DeleteByIdAsync_WithInvalidId_ShouldReturnFalse()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();

        var deleted = await repo.DeleteByIdAsync(99999);

        Assert.False(deleted);
    }

    [Fact]
    public async Task SoftDeleteAsync_ShouldSetIsDeletedFlag()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        var exercise = await CreateTestExerciseAsync("SoftDelete");

        var deleted = await repo.DeleteByIdAsync(exercise.Id);

        Assert.True(deleted);
        
        var fetched = await repo.GetByIdAsync(exercise.Id);
        Assert.Null(fetched);
        
        await using var context = _fixture.CreateContext();
        var allExercises = await context.Exercises.IgnoreQueryFilters().ToListAsync();
        var softDeletedExercise = allExercises.FirstOrDefault(e => e.Id == exercise.Id);
        Assert.NotNull(softDeletedExercise);
        Assert.True(softDeletedExercise.IsDeleted);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleExercises()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        var exercises = new List<Exercise>
        {
            new() { Name = GetUniqueName("Exercise A"), PrimaryMuscleGroup = MuscleGroup.Chest, CreatedAt = DateTime.UtcNow },
            new() { Name = GetUniqueName("Exercise B"), PrimaryMuscleGroup = MuscleGroup.Back, CreatedAt = DateTime.UtcNow }
        };

        var added = await repo.AddRangeAsync(exercises);

        Assert.Equal(2, added.Count());
        var all = await repo.GetAllAsync();
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task DeleteRangeAsync_ShouldSoftDeleteMultipleExercises()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        var exercises = new List<Exercise>
        {
            new() { Name = GetUniqueName("Del1"), PrimaryMuscleGroup = MuscleGroup.Chest, CreatedAt = DateTime.UtcNow },
            new() { Name = GetUniqueName("Del2"), PrimaryMuscleGroup = MuscleGroup.Back, CreatedAt = DateTime.UtcNow }
        };
        var added = await repo.AddRangeAsync(exercises);

        var deleted = await repo.DeleteRangeAsync(added);

        Assert.True(deleted);
        var all = await repo.GetAllAsync();
        Assert.Empty(all);
    }

    [Fact]
    public async Task DeleteAllAsync_ShouldSoftDeleteAllExercises()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        await CreateTestExerciseAsync("All1");
        await CreateTestExerciseAsync("All2");

        var deleted = await repo.DeleteAllAsync();

        Assert.True(deleted);
        var all = await repo.GetAllAsync();
        Assert.Empty(all);
    }

    // ========== حذف شد: SaveChangesAsync_ShouldPersistChanges ==========
    // چون SaveChangesAsync نباید مستقیماً در Repositoryهای مجزا صدا زده بشه

    // ========== Specific Interface Tests ==========

    [Fact]
    public async Task GetExercisesByMuscleGroupAsync_ShouldReturnCorrectExercises()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        // استفاده از نام‌های یکتا
        await CreateTestExerciseAsync("Chest Fly", MuscleGroup.Chest);
        await CreateTestExerciseAsync("Bench Press", MuscleGroup.Chest);
        await CreateTestExerciseAsync("Squat", MuscleGroup.Legs);

        var chestExercises = await repo.GetExercisesByMuscleGroupAsync(MuscleGroup.Chest);

        Assert.Equal(2, chestExercises.Count());
        Assert.All(chestExercises, e => Assert.Equal(MuscleGroup.Chest, e.PrimaryMuscleGroup));
    }

    [Fact]
    public async Task GetMostUsedExercisesAsync_ShouldReturnOrderedByUsage()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        // ایجاد تمرین‌های مجزا با نام‌های یکتا
        var exercise1 = await CreateTestExerciseAsync("Most Used Exercise", MuscleGroup.Chest);
        var exercise2 = await CreateTestExerciseAsync("Medium Used Exercise", MuscleGroup.Back);
        var exercise3 = await CreateTestExerciseAsync("Least Used Exercise", MuscleGroup.Legs);
        
        // ایجاد روزهای تمرینی مجزا برای جلوگیری از Unique Constraint
        var workoutDay1 = await CreateTestWorkoutDayAsync();
        var workoutDay2 = await CreateTestWorkoutDayAsync();
        var workoutDay3 = await CreateTestWorkoutDayAsync();
        
        // exercise1: 3 بار در روزهای مختلف
        await CreateWorkoutDayExerciseAsync(workoutDay1.Id, exercise1.Id);
        await CreateWorkoutDayExerciseAsync(workoutDay2.Id, exercise1.Id);
        await CreateWorkoutDayExerciseAsync(workoutDay3.Id, exercise1.Id);
        
        // exercise2: 2 بار
        var workoutDay4 = await CreateTestWorkoutDayAsync();
        var workoutDay5 = await CreateTestWorkoutDayAsync();
        await CreateWorkoutDayExerciseAsync(workoutDay4.Id, exercise2.Id);
        await CreateWorkoutDayExerciseAsync(workoutDay5.Id, exercise2.Id);
        
        // exercise3: 1 بار
        var workoutDay6 = await CreateTestWorkoutDayAsync();
        await CreateWorkoutDayExerciseAsync(workoutDay6.Id, exercise3.Id);

        var mostUsed = await repo.GetMostUsedExercisesAsync(2);

        Assert.Equal(2, mostUsed.Count());
        var mostUsedList = mostUsed.ToList();
        Assert.Equal(exercise1.Name, mostUsedList[0].Name);
        Assert.Equal(exercise2.Name, mostUsedList[1].Name);
    }

    [Fact]
    public async Task GetExercisesByWorkoutDayAsync_ShouldReturnExercisesForDay()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        var exercise = await CreateTestExerciseAsync("Day Exercise", MuscleGroup.Chest);
        
        var workoutDay = await CreateTestWorkoutDayAsync();
        await CreateWorkoutDayExerciseAsync(workoutDay.Id, exercise.Id);

        var exercises = await repo.GetExercisesByWorkoutDayAsync(workoutDay.Id);

        Assert.Single(exercises);
        Assert.Equal(exercise.Name, exercises.First().Name);
    }

    [Fact]
    public async Task GetExerciseWithWorkoutDaysAsync_ShouldIncludeWorkoutDays()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        var exercise = await CreateTestExerciseAsync("MultiDay Exercise", MuscleGroup.Chest);
        
        var workoutDay1 = await CreateTestWorkoutDayAsync();
        var workoutDay2 = await CreateTestWorkoutDayAsync();
        
        await CreateWorkoutDayExerciseAsync(workoutDay1.Id, exercise.Id);
        await CreateWorkoutDayExerciseAsync(workoutDay2.Id, exercise.Id);

        var fetched = await repo.GetExerciseWithWorkoutDaysAsync(exercise.Id);

        Assert.NotNull(fetched);
        Assert.NotNull(fetched.WorkoutDayExercises);
        Assert.Equal(2, fetched.WorkoutDayExercises.Count);
    }

    [Fact]
    public async Task ExerciseExistsAsync_ShouldReturnTrueWhenExists()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        var exercise = await CreateTestExerciseAsync("UniqueName123", MuscleGroup.Chest);

        var exists = await repo.ExerciseExistsAsync(exercise.Name);

        Assert.True(exists);
    }

    [Fact]
    public async Task ExerciseExistsAsync_ShouldReturnFalseWhenNotExists()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();

        var exists = await repo.ExerciseExistsAsync("NonexistentExerciseName");

        Assert.False(exists);
    }

    [Fact]
    public async Task GetExerciseUsageCountAsync_ShouldReturnCorrectCount()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        var exercise = await CreateTestExerciseAsync("Usage Exercise", MuscleGroup.Chest);
        
        // ایجاد روزهای تمرینی مجزا برای جلوگیری از Unique Constraint
        var workoutDay1 = await CreateTestWorkoutDayAsync();
        var workoutDay2 = await CreateTestWorkoutDayAsync();
        
        await CreateWorkoutDayExerciseAsync(workoutDay1.Id, exercise.Id);
        await CreateWorkoutDayExerciseAsync(workoutDay2.Id, exercise.Id);

        var usageCount = await repo.GetExerciseUsageCountAsync(exercise.Id);

        Assert.Equal(2, usageCount);
    }

    [Fact]
    public async Task GetExerciseUsageCountAsync_ForUnusedExercise_ShouldReturnZero()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        var exercise = await CreateTestExerciseAsync("Unused Exercise", MuscleGroup.Chest);

        var usageCount = await repo.GetExerciseUsageCountAsync(exercise.Id);

        Assert.Equal(0, usageCount);
    }
}
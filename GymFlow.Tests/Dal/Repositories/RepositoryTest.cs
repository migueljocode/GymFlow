using Xunit;
using Microsoft.EntityFrameworkCore;
using GymFlow.Dal.Repositories.Implementations;
using GymFlow.Models.Entities;
using GymFlow.Models.Enums;

namespace GymFlow.Tests.Dal.Repositories;

public class RepositoryTest : IClassFixture<DbContextFixture>
{
    private readonly DbContextFixture _fixture;
    private static int _uniqueCounter = 0;

    public RepositoryTest(DbContextFixture fixture)
    {
        _fixture = fixture;
    }

    // ========== Helper Methods ==========

    private string GetUniqueName(string baseName)
    {
        _uniqueCounter++;
        return $"{baseName}_{_uniqueCounter}_{Guid.NewGuid():N}";
    }

    private async Task<Exercise> CreateTestExerciseAsync(MuscleGroup muscleGroup = MuscleGroup.Chest)
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        var exercise = new Exercise
        {
            Name = GetUniqueName("TestExercise"),
            PrimaryMuscleGroup = muscleGroup,
            Description = "Test description",
            CreatedAt = DateTime.UtcNow
        };
        return await repo.AddAsync(exercise);
    }

    // ========== Query Tests ==========

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectEntity()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var exercise = await CreateTestExerciseAsync();

        var fetched = await repo.GetByIdAsync(exercise.Id);

        Assert.NotNull(fetched);
        Assert.Equal(exercise.Id, fetched.Id);
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
    public async Task FirstOrDefaultAsync_ShouldReturnFirstMatchingEntity()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        // ایجاد دو تمرین با عضلات هدف متفاوت
        var chestExercise = await CreateTestExerciseAsync(MuscleGroup.Chest);
        var backExercise = await CreateTestExerciseAsync(MuscleGroup.Back);

        // جستجوی اولین تمرین با عضله هدف Chest
        var fetched = await repo.FirstOrDefaultAsync(e => e.PrimaryMuscleGroup == MuscleGroup.Chest);

        Assert.NotNull(fetched);
        Assert.Equal(MuscleGroup.Chest, fetched.PrimaryMuscleGroup);
        Assert.Equal(chestExercise.Id, fetched.Id);
    }

    [Fact]
    public async Task SingleOrDefaultAsync_WithSingleMatch_ShouldReturnEntity()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var exercise = await CreateTestExerciseAsync();

        var fetched = await repo.SingleOrDefaultAsync(e => e.Id == exercise.Id);

        Assert.NotNull(fetched);
        Assert.Equal(exercise.Id, fetched.Id);
    }

    [Fact]
    public async Task SingleOrDefaultAsync_WithMultipleMatches_ShouldThrowException()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        // ایجاد دو تمرین
        await CreateTestExerciseAsync(MuscleGroup.Chest);
        await CreateTestExerciseAsync(MuscleGroup.Chest);

        // SingleOrDefaultAsync با شرطی که دو رکورد برمی‌گرداند باید خطا بدهد
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await repo.SingleOrDefaultAsync(e => e.PrimaryMuscleGroup == MuscleGroup.Chest));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        await CreateTestExerciseAsync();
        await CreateTestExerciseAsync();

        var all = await repo.GetAllAsync();

        Assert.NotNull(all);
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task FindAsync_ShouldReturnFilteredEntities()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        await CreateTestExerciseAsync(MuscleGroup.Chest);
        await CreateTestExerciseAsync(MuscleGroup.Legs);

        var found = await repo.FindAsync(e => e.PrimaryMuscleGroup == MuscleGroup.Chest);

        Assert.Single(found);
        Assert.Equal(MuscleGroup.Chest, found.First().PrimaryMuscleGroup);
    }

    [Fact]
    public async Task FindAsync_ById_ShouldReturnEntity()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var exercise = await CreateTestExerciseAsync();

        var fetched = await repo.FindAsync(exercise.Id);

        Assert.NotNull(fetched);
        Assert.Equal(exercise.Id, fetched.Id);
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnTrueWhenExists()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        await CreateTestExerciseAsync();

        var exists = await repo.AnyAsync(e => e.PrimaryMuscleGroup == MuscleGroup.Chest);

        Assert.True(exists);
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnFalseWhenNotExists()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();

        var exists = await repo.AnyAsync(e => e.PrimaryMuscleGroup == (MuscleGroup)999);

        Assert.False(exists);
    }

    [Fact]
    public async Task AllAsync_ShouldReturnTrueWhenAllMatch()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        await CreateTestExerciseAsync(MuscleGroup.Chest);
        await CreateTestExerciseAsync(MuscleGroup.Chest);

        var allMatch = await repo.AllAsync(e => e.PrimaryMuscleGroup == MuscleGroup.Chest);

        Assert.True(allMatch);
    }

    [Fact]
    public async Task AllAsync_ShouldReturnFalseWhenNotAllMatch()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        await CreateTestExerciseAsync(MuscleGroup.Chest);
        await CreateTestExerciseAsync(MuscleGroup.Legs);

        var allMatch = await repo.AllAsync(e => e.PrimaryMuscleGroup == MuscleGroup.Chest);

        Assert.False(allMatch);
    }

    [Fact]
    public async Task CountAsync_ShouldReturnTotalCount()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        await CreateTestExerciseAsync();
        await CreateTestExerciseAsync();

        var count = await repo.CountAsync();

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ShouldReturnFilteredCount()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        await CreateTestExerciseAsync(MuscleGroup.Chest);
        await CreateTestExerciseAsync(MuscleGroup.Chest);
        await CreateTestExerciseAsync(MuscleGroup.Legs);

        var count = await repo.CountAsync(e => e.PrimaryMuscleGroup == MuscleGroup.Chest);

        Assert.Equal(2, count);
    }

    // ========== Command Tests ==========

    [Fact]
    public async Task AddAsync_ShouldSaveEntityToDatabase()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var exercise = new Exercise
        {
            Name = GetUniqueName("NewExercise"),
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
    public async Task UpdateAsync_ShouldModifyExistingEntity()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var exercise = await CreateTestExerciseAsync();
        var newName = GetUniqueName("UpdatedName");
        exercise.Name = newName;

        var updated = await repo.UpdateAsync(exercise);

        var fetched = await repo.GetByIdAsync(updated.Id);
        Assert.Equal(newName, fetched?.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteEntity()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var exercise = await CreateTestExerciseAsync();

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
    public async Task DeleteByIdAsync_ShouldSoftDeleteEntityById()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var exercise = await CreateTestExerciseAsync();

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
    public async Task AddRangeAsync_ShouldAddMultipleEntities()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var exercises = new List<Exercise>
        {
            new() { Name = GetUniqueName("ExerciseA"), PrimaryMuscleGroup = MuscleGroup.Chest, CreatedAt = DateTime.UtcNow },
            new() { Name = GetUniqueName("ExerciseB"), PrimaryMuscleGroup = MuscleGroup.Back, CreatedAt = DateTime.UtcNow }
        };

        var added = await repo.AddRangeAsync(exercises);

        Assert.Equal(2, added.Count());
        var all = await repo.GetAllAsync();
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task DeleteRangeAsync_ShouldSoftDeleteMultipleEntities()
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
    public async Task DeleteAllAsync_ShouldSoftDeleteAllEntities()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        await CreateTestExerciseAsync();
        await CreateTestExerciseAsync();

        var deleted = await repo.DeleteAllAsync();

        Assert.True(deleted);
        var all = await repo.GetAllAsync();
        Assert.Empty(all);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistPendingChanges()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        // ایجاد یک موجودیت جدید از طریق DbContext مستقیم (نه از طریق Repository)
        await using var context = _fixture.CreateContext();
        var exercise = new Exercise
        {
            Name = GetUniqueName("SaveChangesTest"),
            PrimaryMuscleGroup = MuscleGroup.Chest,
            CreatedAt = DateTime.UtcNow
        };
        context.Exercises.Add(exercise);
        
        var savedCount = await context.SaveChangesAsync();

        Assert.True(savedCount > 0);
        
        // بررسی اینکه موجودیت ذخیره شده
        var fetched = await repo.GetByIdAsync(exercise.Id);
        Assert.NotNull(fetched);
    }

    // ========== Soft Delete Filter Tests ==========

    [Fact]
    public async Task SoftDeletedEntities_ShouldBeExcludedFromQueries()
    {
        var repo = new ExerciseRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var exercise = await CreateTestExerciseAsync();
        
        // Soft delete کردن با DeleteByIdAsync
        await repo.DeleteByIdAsync(exercise.Id);
        
        // GetAll نباید موجودیت soft deleted شده را برگرداند
        var all = await repo.GetAllAsync();
        Assert.DoesNotContain(all, e => e.Id == exercise.Id);
        
        // FindAsync (با predicate) نباید موجودیت soft deleted شده را برگرداند
        var found = await repo.FindAsync(e => e.Id == exercise.Id);
        Assert.Empty(found);
        
        // CountAsync نباید موجودیت soft deleted شده را بشمارد
        var count = await repo.CountAsync();
        Assert.Equal(0, count);
        
        // با IgnoreQueryFilters می‌توانیم موجودیت soft deleted شده را ببینیم
        await using var context = _fixture.CreateContext();
        var allWithDeleted = await context.Exercises.IgnoreQueryFilters().ToListAsync();
        var deletedExercise = allWithDeleted.FirstOrDefault(e => e.Id == exercise.Id);
        Assert.NotNull(deletedExercise);
        Assert.True(deletedExercise.IsDeleted);
    }
}
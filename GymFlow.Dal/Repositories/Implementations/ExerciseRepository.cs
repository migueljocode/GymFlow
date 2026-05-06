namespace GymFlow.Dal.Repositories.Implementations;

/// <summary>
/// Repository implementation for Exercise entity with specialized query methods.
/// </summary>
public class ExerciseRepository : Repository<Exercise>, IExerciseRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExerciseRepository"/> class.
    /// </summary>
    /// <param name="dbContextFactory">Factory for creating DbContext instances.</param>
    public ExerciseRepository(IDbContextFactory<AppDbContext> dbContextFactory) 
        : base(dbContextFactory) { }

    /// <inheritdoc />
    public async Task<IEnumerable<Exercise>> GetExercisesByMuscleGroupAsync(MuscleGroup muscleGroup)
    {
        await using var context = await CreateContextAsync();
        return await context.Exercises
            .Where(e => e.PrimaryMuscleGroup == muscleGroup)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Exercise>> GetMostUsedExercisesAsync(int topCount = 10)
    {
        await using var context = await CreateContextAsync();
        return await context.Exercises
            .Select(e => new
            {
                Exercise = e,
                UsageCount = e.WorkoutDayExercises.Count()
            })
            .OrderByDescending(x => x.UsageCount)
            .Take(topCount)
            .Select(x => x.Exercise)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Exercise>> GetExercisesByWorkoutDayAsync(int workoutDayId)
    {
        await using var context = await CreateContextAsync();
        return await context.WorkoutDayExercises
            .Where(wde => wde.WorkoutDayId == workoutDayId)
            .Include(wde => wde.Exercise)
            .Select(wde => wde.Exercise)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Exercise?> GetExerciseWithWorkoutDaysAsync(int exerciseId)
    {
        await using var context = await CreateContextAsync();
        return await context.Exercises
            .Include(e => e.WorkoutDayExercises)
                .ThenInclude(wde => wde.WorkoutDay)
            .FirstOrDefaultAsync(e => e.Id == exerciseId);
    }

    /// <inheritdoc />
    public async Task<bool> ExerciseExistsAsync(string exerciseName)
    {
        await using var context = await CreateContextAsync();
        return await context.Exercises
            .AnyAsync(e => e.Name.ToLower() == exerciseName.ToLower());
    }

    /// <inheritdoc />
    public async Task<int> GetExerciseUsageCountAsync(int exerciseId)
    {
        await using var context = await CreateContextAsync();
        return await context.WorkoutDayExercises
            .Where(wde => wde.ExerciseId == exerciseId)
            .CountAsync();
    }
}
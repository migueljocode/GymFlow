namespace GymFlow.Dal.Repositories.Interfaces;

/// <summary>
/// Repository interface for Exercise-specific operations extending the generic repository.
/// </summary>
/// <remarks>
/// Provides specialized query methods for managing exercise library,
/// including muscle group filtering, usage statistics, and duplicate prevention.
/// </remarks>
public interface IExerciseRepository : IRepository<Exercise>
{
    /// <summary>
    /// Retrieves all exercises targeting a specific muscle group.
    /// </summary>
    /// <param name="muscleGroup">The muscle group to filter by (e.g., Chest, Legs, Arms).</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains a collection of exercises for that muscle group, ordered alphabetically by name.
    /// </returns>
    /// <remarks>
    /// Useful for suggesting exercises when building workout plans for specific muscle groups.
    /// </remarks>
    Task<IEnumerable<Exercise>> GetExercisesByMuscleGroupAsync(MuscleGroup muscleGroup);
    
    /// <summary>
    /// Retrieves the most frequently used exercises across all workout plans.
    /// </summary>
    /// <param name="topCount">The number of top exercises to retrieve (default 10).</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the most popular exercises, ordered by usage count descending.
    /// </returns>
    /// <remarks>
    /// Helps identify core exercises that are commonly used by many users.
    /// Can be used for "recommended" or "trending" exercise features.
    /// </remarks>
    Task<IEnumerable<Exercise>> GetMostUsedExercisesAsync(int topCount = 10);
    
    /// <summary>
    /// Retrieves all exercises used in a specific workout day.
    /// </summary>
    /// <param name="workoutDayId">The unique identifier of the workout day.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains a collection of exercises used in that day.
    /// </returns>
    /// <remarks>
    /// Uses the junction table (WorkoutDayExercise) to find exercises without loading
    /// the entire day details. Useful for quick exercise lookups.
    /// </remarks>
    Task<IEnumerable<Exercise>> GetExercisesByWorkoutDayAsync(int workoutDayId);
    
    /// <summary>
    /// Retrieves an exercise with all its associated workout days (where this exercise is used).
    /// </summary>
    /// <param name="exerciseId">The unique identifier of the exercise.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the exercise with WorkoutDayExercises populated, or <c>null</c> if not found.
    /// </returns>
    /// <remarks>
    /// Useful for analyzing how frequently an exercise is programmed and in which contexts (e.g., is it used mainly in beginner or advanced plans?).
    /// </remarks>
    Task<Exercise?> GetExerciseWithWorkoutDaysAsync(int exerciseId);
    
    /// <summary>
    /// Checks if an exercise with the given name already exists (case-insensitive).
    /// </summary>
    /// <param name="exerciseName">The name of the exercise to check.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains <c>true</c> if the exercise exists; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Prevents duplicate exercises being added to the library (e.g., "Bench Press" vs "bench press").
    /// Called before adding a new exercise to enforce uniqueness.
    /// </remarks>
    Task<bool> ExerciseExistsAsync(string exerciseName);
    
    /// <summary>
    /// Gets the total number of times an exercise has been used across all workout plans.
    /// </summary>
    /// <param name="exerciseId">The unique identifier of the exercise.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the usage count (number of WorkoutDayExercise entries referencing this exercise).
    /// </returns>
    /// <remarks>
    /// Useful for analytics and determining exercise popularity.
    /// Zero usage indicates an exercise that's been added to library but never programmed.
    /// </remarks>
    Task<int> GetExerciseUsageCountAsync(int exerciseId);
}
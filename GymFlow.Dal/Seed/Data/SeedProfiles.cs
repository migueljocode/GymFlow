namespace GymFlow.Dal.Seed.Data;

/// <summary>
/// Pre-defined seed profiles for different scenarios.
/// Each profile represents a specific use case with appropriate data volumes and characteristics.
/// </summary>
public static class SeedProfiles
{
    /// <summary>
    /// Default development profile - Rich, realistic data for development and testing.
    /// </summary>
    public static SeedOptions Development => new()
    {
        RefreshOnStartup = true,
        ClearExistingData = true,
        SeedOnlyIfEmpty = false,
        UserCount = 15,
        IncludeDemoUser = true,
        MinWorkoutPlansPerUser = 2,
        MaxWorkoutPlansPerUser = 4,
        MinWorkoutDaysPerPlan = 3,
        MaxWorkoutDaysPerPlan = 6,
        MinExercisesPerDay = 4,
        MaxExercisesPerDay = 8,
        MinProgressLogsPerUser = 15,
        MaxProgressLogsPerUser = 45,
        WorkoutSessionCompletionRate = 0.65,
        BodyFatPercentageInclusionRate = 0.7,
        FeelingNoteProbability = 0.8,
        CompoundWorkoutProbability = 0.3,
        RandomSeed = 1337
    };

    /// <summary>
    /// Quick demo profile - Minimal but realistic data for quick demonstrations.
    /// </summary>
    public static SeedOptions QuickDemo => new()
    {
        RefreshOnStartup = true,
        ClearExistingData = true,
        SeedOnlyIfEmpty = false,
        UserCount = 3,
        IncludeDemoUser = true,
        MinWorkoutPlansPerUser = 1,
        MaxWorkoutPlansPerUser = 1,
        MinWorkoutDaysPerPlan = 3,
        MaxWorkoutDaysPerPlan = 3,
        MinExercisesPerDay = 4,
        MaxExercisesPerDay = 6,
        MinProgressLogsPerUser = 5,
        MaxProgressLogsPerUser = 10,
        WorkoutSessionCompletionRate = 0.8,
        BodyFatPercentageInclusionRate = 0.7,
        FeelingNoteProbability = 0.8,
        CompoundWorkoutProbability = 0.2,
        RandomSeed = 42
    };

    /// <summary>
    /// Lightweight profile - Very minimal data for fast seeding and basic testing.
    /// </summary>
    public static SeedOptions Lightweight => new()
    {
        RefreshOnStartup = true,
        ClearExistingData = true,
        SeedOnlyIfEmpty = false,
        UserCount = 5,
        IncludeDemoUser = true,
        MinWorkoutPlansPerUser = 1,
        MaxWorkoutPlansPerUser = 2,
        MinWorkoutDaysPerPlan = 2,
        MaxWorkoutDaysPerPlan = 3,
        MinExercisesPerDay = 3,
        MaxExercisesPerDay = 5,
        MinProgressLogsPerUser = 5,
        MaxProgressLogsPerUser = 15,
        WorkoutSessionCompletionRate = 0.7,
        BodyFatPercentageInclusionRate = 0.5,
        FeelingNoteProbability = 0.6,
        CompoundWorkoutProbability = 0.1,
        RandomSeed = 123
    };

    /// <summary>
    /// Stress test profile - Large data volume for performance testing.
    /// </summary>
    public static SeedOptions StressTest => new()
    {
        RefreshOnStartup = true,
        ClearExistingData = true,
        SeedOnlyIfEmpty = false,
        UserCount = 50,
        IncludeDemoUser = true,
        MinWorkoutPlansPerUser = 3,
        MaxWorkoutPlansPerUser = 5,
        MinWorkoutDaysPerPlan = 4,
        MaxWorkoutDaysPerPlan = 6,
        MinExercisesPerDay = 5,
        MaxExercisesPerDay = 10,
        MinProgressLogsPerUser = 30,
        MaxProgressLogsPerUser = 60,
        WorkoutSessionCompletionRate = 0.6,
        BodyFatPercentageInclusionRate = 0.8,
        FeelingNoteProbability = 0.9,
        CompoundWorkoutProbability = 0.4,
        RandomSeed = null
    };

    /// <summary>
    /// Production profile - No automatic seed, only if empty, minimal data.
    /// </summary>
    public static SeedOptions Production => new()
    {
        RefreshOnStartup = false,
        ClearExistingData = false,
        SeedOnlyIfEmpty = true,
        UserCount = 0,
        IncludeDemoUser = false,
        MinWorkoutPlansPerUser = 0,
        MaxWorkoutPlansPerUser = 0,
        MinWorkoutDaysPerPlan = 0,
        MaxWorkoutDaysPerPlan = 0,
        MinExercisesPerDay = 0,
        MaxExercisesPerDay = 0,
        MinProgressLogsPerUser = 0,
        MaxProgressLogsPerUser = 0,
        WorkoutSessionCompletionRate = 0,
        BodyFatPercentageInclusionRate = 0,
        FeelingNoteProbability = 0,
        CompoundWorkoutProbability = 0,
        RandomSeed = null
    };

    /// <summary>
    /// Empty profile - No data at all (useful for clean starts).
    /// </summary>
    public static SeedOptions Empty => new()
    {
        RefreshOnStartup = false,
        ClearExistingData = true,
        SeedOnlyIfEmpty = false,
        UserCount = 0,
        IncludeDemoUser = false,
        MinWorkoutPlansPerUser = 0,
        MaxWorkoutPlansPerUser = 0,
        MinWorkoutDaysPerPlan = 0,
        MaxWorkoutDaysPerPlan = 0,
        MinExercisesPerDay = 0,
        MaxExercisesPerDay = 0,
        MinProgressLogsPerUser = 0,
        MaxProgressLogsPerUser = 0,
        WorkoutSessionCompletionRate = 0,
        BodyFatPercentageInclusionRate = 0,
        FeelingNoteProbability = 0,
        CompoundWorkoutProbability = 0,
        RandomSeed = null
    };

    /// <summary>
    /// Gets a profile by name (case-insensitive).
    /// </summary>
    public static SeedOptions GetByName(string profileName)
    {
        return profileName?.ToLowerInvariant() switch
        {
            "development" or "dev" => Development,
            "quickdemo" or "demo" => QuickDemo,
            "lightweight" or "light" => Lightweight,
            "stresstest" or "stress" or "performance" => StressTest,
            "production" or "prod" => Production,
            "empty" or "clean" or "none" => Empty,
            _ => Development
        };
    }

    /// <summary>
    /// Returns list of all available profile names.
    /// </summary>
    public static IReadOnlyList<string> AvailableProfiles => new[]
    {
        "Development",
        "QuickDemo",
        "Lightweight",
        "StressTest",
        "Production",
        "Empty"
    };
}
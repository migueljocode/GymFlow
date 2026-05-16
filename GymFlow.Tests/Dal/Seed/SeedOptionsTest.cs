using Xunit;
using GymFlow.Dal.Seed.Data;

namespace GymFlow.Tests.Dal.Seed.Data;

public class SeedOptionsTest
{
    [Fact]
    public void DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var options = new SeedOptions();

        // Assert
        Assert.True(options.RefreshOnStartup);
        Assert.True(options.ClearExistingData);
        Assert.False(options.SeedOnlyIfEmpty);
        
        // User Generation
        Assert.Equal(15, options.UserCount);
        Assert.True(options.IncludeDemoUser);
        Assert.Equal("demo@gymflow.com", options.DemoUserEmail);
        Assert.Equal("Demo", options.DemoUserFirstName);
        Assert.Equal("User", options.DemoUserLastName);
        
        // Workout Plan Generation
        Assert.Equal(2, options.MinWorkoutPlansPerUser);
        Assert.Equal(4, options.MaxWorkoutPlansPerUser);
        Assert.Equal(3, options.MinWorkoutDaysPerPlan);
        Assert.Equal(6, options.MaxWorkoutDaysPerPlan);
        Assert.Equal(4, options.MinExercisesPerDay);
        Assert.Equal(8, options.MaxExercisesPerDay);
        Assert.Equal(0.3, options.CompoundWorkoutProbability);
        
        // Progress Log Generation
        Assert.Equal(15, options.MinProgressLogsPerUser);
        Assert.Equal(45, options.MaxProgressLogsPerUser);
        Assert.Equal(0.7, options.BodyFatPercentageInclusionRate);
        
        // Workout Session Generation
        Assert.Equal(0.65, options.WorkoutSessionCompletionRate);
        Assert.Equal(0.8, options.FeelingNoteProbability);
        
        // Realism & Variety
        Assert.Equal(1337, options.RandomSeed);
        Assert.Equal((-0.7f, -0.3f), options.ActiveWeightChangeRange);
        Assert.Equal((0.2f, 0.5f), options.BreakWeightChangeRange);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        // Arrange
        var options = new SeedOptions();

        // Act
        options.RefreshOnStartup = false;
        options.ClearExistingData = false;
        options.SeedOnlyIfEmpty = true;
        options.UserCount = 10;
        options.IncludeDemoUser = false;
        options.DemoUserEmail = "custom@example.com";
        options.DemoUserFirstName = "Custom";
        options.DemoUserLastName = "Name";
        options.MinWorkoutPlansPerUser = 1;
        options.MaxWorkoutPlansPerUser = 3;
        options.MinWorkoutDaysPerPlan = 2;
        options.MaxWorkoutDaysPerPlan = 4;
        options.MinExercisesPerDay = 3;
        options.MaxExercisesPerDay = 6;
        options.CompoundWorkoutProbability = 0.5;
        options.MinProgressLogsPerUser = 10;
        options.MaxProgressLogsPerUser = 30;
        options.BodyFatPercentageInclusionRate = 0.6;
        options.WorkoutSessionCompletionRate = 0.7;
        options.FeelingNoteProbability = 0.9;
        options.RandomSeed = 42;
        options.ActiveWeightChangeRange = (-0.5f, -0.2f);
        options.BreakWeightChangeRange = (0.3f, 0.6f);

        // Assert
        Assert.False(options.RefreshOnStartup);
        Assert.False(options.ClearExistingData);
        Assert.True(options.SeedOnlyIfEmpty);
        Assert.Equal(10, options.UserCount);
        Assert.False(options.IncludeDemoUser);
        Assert.Equal("custom@example.com", options.DemoUserEmail);
        Assert.Equal("Custom", options.DemoUserFirstName);
        Assert.Equal("Name", options.DemoUserLastName);
        Assert.Equal(1, options.MinWorkoutPlansPerUser);
        Assert.Equal(3, options.MaxWorkoutPlansPerUser);
        Assert.Equal(2, options.MinWorkoutDaysPerPlan);
        Assert.Equal(4, options.MaxWorkoutDaysPerPlan);
        Assert.Equal(3, options.MinExercisesPerDay);
        Assert.Equal(6, options.MaxExercisesPerDay);
        Assert.Equal(0.5, options.CompoundWorkoutProbability);
        Assert.Equal(10, options.MinProgressLogsPerUser);
        Assert.Equal(30, options.MaxProgressLogsPerUser);
        Assert.Equal(0.6, options.BodyFatPercentageInclusionRate);
        Assert.Equal(0.7, options.WorkoutSessionCompletionRate);
        Assert.Equal(0.9, options.FeelingNoteProbability);
        Assert.Equal(42, options.RandomSeed);
        Assert.Equal((-0.5f, -0.2f), options.ActiveWeightChangeRange);
        Assert.Equal((0.3f, 0.6f), options.BreakWeightChangeRange);
    }

    [Fact]
    public void FromProfile_ShouldCreateCopyWithSameValues()
    {
        // Arrange
        var original = new SeedOptions
        {
            RefreshOnStartup = false,
            ClearExistingData = false,
            SeedOnlyIfEmpty = true,
            UserCount = 5,
            IncludeDemoUser = false,
            DemoUserEmail = "copy@example.com",
            DemoUserFirstName = "Copy",
            DemoUserLastName = "Test",
            MinWorkoutPlansPerUser = 1,
            MaxWorkoutPlansPerUser = 2,
            MinWorkoutDaysPerPlan = 2,
            MaxWorkoutDaysPerPlan = 3,
            MinExercisesPerDay = 3,
            MaxExercisesPerDay = 5,
            CompoundWorkoutProbability = 0.4,
            MinProgressLogsPerUser = 8,
            MaxProgressLogsPerUser = 20,
            BodyFatPercentageInclusionRate = 0.5,
            WorkoutSessionCompletionRate = 0.6,
            FeelingNoteProbability = 0.7,
            RandomSeed = 99,
            ActiveWeightChangeRange = (-0.6f, -0.3f),
            BreakWeightChangeRange = (0.1f, 0.4f)
        };

        // Act
        var copy = SeedOptions.FromProfile(original);

        // Assert
        Assert.Equal(original.RefreshOnStartup, copy.RefreshOnStartup);
        Assert.Equal(original.ClearExistingData, copy.ClearExistingData);
        Assert.Equal(original.SeedOnlyIfEmpty, copy.SeedOnlyIfEmpty);
        Assert.Equal(original.UserCount, copy.UserCount);
        Assert.Equal(original.IncludeDemoUser, copy.IncludeDemoUser);
        Assert.Equal(original.DemoUserEmail, copy.DemoUserEmail);
        Assert.Equal(original.DemoUserFirstName, copy.DemoUserFirstName);
        Assert.Equal(original.DemoUserLastName, copy.DemoUserLastName);
        Assert.Equal(original.MinWorkoutPlansPerUser, copy.MinWorkoutPlansPerUser);
        Assert.Equal(original.MaxWorkoutPlansPerUser, copy.MaxWorkoutPlansPerUser);
        Assert.Equal(original.MinWorkoutDaysPerPlan, copy.MinWorkoutDaysPerPlan);
        Assert.Equal(original.MaxWorkoutDaysPerPlan, copy.MaxWorkoutDaysPerPlan);
        Assert.Equal(original.MinExercisesPerDay, copy.MinExercisesPerDay);
        Assert.Equal(original.MaxExercisesPerDay, copy.MaxExercisesPerDay);
        Assert.Equal(original.CompoundWorkoutProbability, copy.CompoundWorkoutProbability);
        Assert.Equal(original.MinProgressLogsPerUser, copy.MinProgressLogsPerUser);
        Assert.Equal(original.MaxProgressLogsPerUser, copy.MaxProgressLogsPerUser);
        Assert.Equal(original.BodyFatPercentageInclusionRate, copy.BodyFatPercentageInclusionRate);
        Assert.Equal(original.WorkoutSessionCompletionRate, copy.WorkoutSessionCompletionRate);
        Assert.Equal(original.FeelingNoteProbability, copy.FeelingNoteProbability);
        Assert.Equal(original.RandomSeed, copy.RandomSeed);
        Assert.Equal(original.ActiveWeightChangeRange, copy.ActiveWeightChangeRange);
        Assert.Equal(original.BreakWeightChangeRange, copy.BreakWeightChangeRange);
        
        // Ensure it's a different instance
        Assert.NotSame(original, copy);
    }

    [Fact]
    public void FromProfile_ShouldNotAffectOriginalWhenCopyIsModified()
    {
        // Arrange
        var original = new SeedOptions();
        var copy = SeedOptions.FromProfile(original);

        // Act
        copy.UserCount = 100;
        copy.IncludeDemoUser = false;
        copy.RandomSeed = 999;

        // Assert
        Assert.Equal(15, original.UserCount);
        Assert.True(original.IncludeDemoUser);
        Assert.Equal(1337, original.RandomSeed);
        
        Assert.Equal(100, copy.UserCount);
        Assert.False(copy.IncludeDemoUser);
        Assert.Equal(999, copy.RandomSeed);
    }

    [Fact]
    public void RandomSeed_CanBeNull()
    {
        // Arrange
        var options = new SeedOptions();

        // Act
        options.RandomSeed = null;

        // Assert
        Assert.Null(options.RandomSeed);
    }

    [Fact]
    public void ActiveWeightChangeRange_CanBeSetToDifferentValues()
    {
        // Arrange
        var options = new SeedOptions();

        // Act
        options.ActiveWeightChangeRange = (-1.0f, -0.1f);

        // Assert
        Assert.Equal(-1.0f, options.ActiveWeightChangeRange.Min);
        Assert.Equal(-0.1f, options.ActiveWeightChangeRange.Max);
    }

    [Fact]
    public void BreakWeightChangeRange_CanBeSetToDifferentValues()
    {
        // Arrange
        var options = new SeedOptions();

        // Act
        options.BreakWeightChangeRange = (0.5f, 1.0f);

        // Assert
        Assert.Equal(0.5f, options.BreakWeightChangeRange.Min);
        Assert.Equal(1.0f, options.BreakWeightChangeRange.Max);
    }

    [Fact]
    public void MinMaxValues_ShouldBeValid()
    {
        // Arrange
        var options = new SeedOptions();

        // Assert - Min should be less than or equal to Max
        Assert.True(options.MinWorkoutPlansPerUser <= options.MaxWorkoutPlansPerUser);
        Assert.True(options.MinWorkoutDaysPerPlan <= options.MaxWorkoutDaysPerPlan);
        Assert.True(options.MinExercisesPerDay <= options.MaxExercisesPerDay);
        Assert.True(options.MinProgressLogsPerUser <= options.MaxProgressLogsPerUser);
    }

    [Fact]
    public void ProbabilityValues_ShouldBeBetweenZeroAndOne()
    {
        // Arrange
        var options = new SeedOptions();

        // Assert
        Assert.InRange(options.CompoundWorkoutProbability, 0, 1);
        Assert.InRange(options.BodyFatPercentageInclusionRate, 0, 1);
        Assert.InRange(options.WorkoutSessionCompletionRate, 0, 1);
        Assert.InRange(options.FeelingNoteProbability, 0, 1);
    }

    [Fact]
    public void WeightChangeRanges_ShouldHaveMinLessThanMax()
    {
        // Arrange
        var options = new SeedOptions();

        // Assert
        Assert.True(options.ActiveWeightChangeRange.Min <= options.ActiveWeightChangeRange.Max);
        Assert.True(options.BreakWeightChangeRange.Min <= options.BreakWeightChangeRange.Max);
    }

    [Fact]
    public void DemoUserProperties_ShouldBeStringProperties()
    {
        // Arrange
        var options = new SeedOptions();

        // Act & Assert
        options.DemoUserEmail = "newemail@test.com";
        options.DemoUserFirstName = "NewFirstName";
        options.DemoUserLastName = "NewLastName";
        
        Assert.Equal("newemail@test.com", options.DemoUserEmail);
        Assert.Equal("NewFirstName", options.DemoUserFirstName);
        Assert.Equal("NewLastName", options.DemoUserLastName);
        
        Assert.IsType<string>(options.DemoUserEmail);
        Assert.IsType<string>(options.DemoUserFirstName);
        Assert.IsType<string>(options.DemoUserLastName);
    }
}
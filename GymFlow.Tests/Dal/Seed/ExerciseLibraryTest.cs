namespace GymFlow.Tests.Dal.Seed.Constants;

public class ExerciseLibraryTest
{
    // ========== Tests for GetAllExercises ==========

    [Fact]
    public void GetAllExercises_ShouldReturnAllExercises()
    {
        // Act
        var exercises = ExerciseLibrary.GetAllExercises();

        // Assert
        Assert.NotNull(exercises);
        Assert.Equal(52, exercises.Count); // 8 Chest + 9 Back + 10 Legs + 8 Shoulders + 9 Arms + 8 Core = 52? 
        // بیا دقیق بشماریم:
        // Chest: 8
        // Back: 9  
        // Legs: 10
        // Shoulders: 8
        // Arms: 9
        // Core: 8
        // Total: 8+9+10+8+9+8 = 52
    }

    [Fact]
    public void GetAllExercises_ShouldHaveUniqueNames()
    {
        // Act
        var exercises = ExerciseLibrary.GetAllExercises();
        var names = exercises.Select(e => e.Name).ToList();
        var distinctNames = names.Distinct().ToList();

        // Assert
        Assert.Equal(names.Count, distinctNames.Count);
    }

    [Fact]
    public void GetAllExercises_ShouldHaveValidDifficultyRange()
    {
        // Act
        var exercises = ExerciseLibrary.GetAllExercises();

        // Assert
        foreach (var exercise in exercises)
        {
            Assert.InRange(exercise.Difficulty, 1, 5);
        }
    }

    [Fact]
    public void GetAllExercises_ShouldHaveNonEmptyDescription()
    {
        // Act
        var exercises = ExerciseLibrary.GetAllExercises();

        // Assert
        foreach (var exercise in exercises)
        {
            Assert.NotNull(exercise.Description);
            Assert.NotEmpty(exercise.Description);
        }
    }

    [Fact]
    public void GetAllExercises_ShouldHaveNonEmptyEquipment()
    {
        // Act
        var exercises = ExerciseLibrary.GetAllExercises();

        // Assert
        foreach (var exercise in exercises)
        {
            Assert.NotNull(exercise.Equipment);
            Assert.NotEmpty(exercise.Equipment);
        }
    }

    [Fact]
    public void GetAllExercises_ShouldHaveValidMuscleGroups()
    {
        // Act
        var exercises = ExerciseLibrary.GetAllExercises();
        var validMuscleGroups = new[] 
        { 
            MuscleGroup.Chest, MuscleGroup.Back, MuscleGroup.Legs, 
            MuscleGroup.Shoulders, MuscleGroup.Arms, MuscleGroup.Core 
        };

        // Assert
        foreach (var exercise in exercises)
        {
            Assert.Contains(exercise.MuscleGroup, validMuscleGroups);
        }
    }

    // ========== Tests for GetByMuscleGroup ==========

    [Fact]
    public void GetByMuscleGroup_ShouldReturnCorrectCountForChest()
    {
        // Act
        var chestExercises = ExerciseLibrary.GetByMuscleGroup(MuscleGroup.Chest);

        // Assert
        Assert.NotNull(chestExercises);
        Assert.Equal(8, chestExercises.Count);
        Assert.All(chestExercises, e => Assert.Equal(MuscleGroup.Chest, e.MuscleGroup));
    }

    [Fact]
    public void GetByMuscleGroup_ShouldReturnCorrectCountForBack()
    {
        // Act
        var backExercises = ExerciseLibrary.GetByMuscleGroup(MuscleGroup.Back);

        // Assert
        Assert.NotNull(backExercises);
        Assert.Equal(9, backExercises.Count);
        Assert.All(backExercises, e => Assert.Equal(MuscleGroup.Back, e.MuscleGroup));
    }

    [Fact]
    public void GetByMuscleGroup_ShouldReturnCorrectCountForLegs()
    {
        // Act
        var legExercises = ExerciseLibrary.GetByMuscleGroup(MuscleGroup.Legs);

        // Assert
        Assert.NotNull(legExercises);
        Assert.Equal(10, legExercises.Count);
        Assert.All(legExercises, e => Assert.Equal(MuscleGroup.Legs, e.MuscleGroup));
    }

    [Fact]
    public void GetByMuscleGroup_ShouldReturnCorrectCountForShoulders()
    {
        // Act
        var shoulderExercises = ExerciseLibrary.GetByMuscleGroup(MuscleGroup.Shoulders);

        // Assert
        Assert.NotNull(shoulderExercises);
        Assert.Equal(8, shoulderExercises.Count);
        Assert.All(shoulderExercises, e => Assert.Equal(MuscleGroup.Shoulders, e.MuscleGroup));
    }

    [Fact]
    public void GetByMuscleGroup_ShouldReturnCorrectCountForArms()
    {
        // Act
        var armExercises = ExerciseLibrary.GetByMuscleGroup(MuscleGroup.Arms);

        // Assert
        Assert.NotNull(armExercises);
        Assert.Equal(9, armExercises.Count);
        Assert.All(armExercises, e => Assert.Equal(MuscleGroup.Arms, e.MuscleGroup));
    }

    [Fact]
    public void GetByMuscleGroup_ShouldReturnCorrectCountForCore()
    {
        // Act
        var coreExercises = ExerciseLibrary.GetByMuscleGroup(MuscleGroup.Core);

        // Assert
        Assert.NotNull(coreExercises);
        Assert.Equal(8, coreExercises.Count);
        Assert.All(coreExercises, e => Assert.Equal(MuscleGroup.Core, e.MuscleGroup));
    }

    [Fact]
    public void GetByMuscleGroup_WithInvalidMuscleGroup_ShouldReturnEmpty()
    {
        // Act
        var exercises = ExerciseLibrary.GetByMuscleGroup((MuscleGroup)999);

        // Assert
        Assert.NotNull(exercises);
        Assert.Empty(exercises);
    }

    // ========== Tests for GetRandomByMuscleGroup ==========

    [Fact]
    public void GetRandomByMuscleGroup_ShouldReturnCorrectCount()
    {
        // Arrange
        var random = new Random(42);
        var count = 3;

        // Act
        var randomExercises = ExerciseLibrary.GetRandomByMuscleGroup(MuscleGroup.Chest, count, random);

        // Assert
        Assert.NotNull(randomExercises);
        Assert.Equal(count, randomExercises.Count);
        Assert.All(randomExercises, e => Assert.Equal(MuscleGroup.Chest, e.MuscleGroup));
    }

    [Fact]
    public void GetRandomByMuscleGroup_ShouldNotExceedAvailableCount()
    {
        // Arrange
        var random = new Random(42);
        var count = 20; // بیشتر از تعداد موجود

        // Act
        var randomExercises = ExerciseLibrary.GetRandomByMuscleGroup(MuscleGroup.Core, count, random);

        // Assert
        Assert.NotNull(randomExercises);
        Assert.Equal(8, randomExercises.Count); // فقط 8 تمرین Core وجود دارد
    }

    [Fact]
    public void GetRandomByMuscleGroup_WithSameSeed_ShouldReturnSameResults()
    {
        // Arrange
        var random1 = new Random(123);
        var random2 = new Random(123);
        var count = 3;

        // Act
        var result1 = ExerciseLibrary.GetRandomByMuscleGroup(MuscleGroup.Legs, count, random1);
        var result2 = ExerciseLibrary.GetRandomByMuscleGroup(MuscleGroup.Legs, count, random2);

        // Assert
        Assert.Equal(result1.Count, result2.Count);
        for (int i = 0; i < result1.Count; i++)
        {
            Assert.Equal(result1[i].Name, result2[i].Name);
        }
    }

    [Fact]
    public void GetRandomByMuscleGroup_WithDifferentSeeds_ShouldReturnDifferentResults()
    {
        // Arrange
        var random1 = new Random(123);
        var random2 = new Random(456);
        var count = 3;

        // Act
        var result1 = ExerciseLibrary.GetRandomByMuscleGroup(MuscleGroup.Chest, count, random1);
        var result2 = ExerciseLibrary.GetRandomByMuscleGroup(MuscleGroup.Chest, count, random2);

        // Assert - احتمال اینکه نتایج یکسان باشد بسیار کم است
        bool isDifferent = false;
        for (int i = 0; i < Math.Min(result1.Count, result2.Count); i++)
        {
            if (result1[i].Name != result2[i].Name)
            {
                isDifferent = true;
                break;
            }
        }
        Assert.True(isDifferent, "Expected different results with different seeds");
    }

    [Fact]
    public void GetRandomByMuscleGroup_WithCountZero_ShouldReturnEmpty()
    {
        // Arrange
        var random = new Random(42);

        // Act
        var result = ExerciseLibrary.GetRandomByMuscleGroup(MuscleGroup.Chest, 0, random);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    // ========== Tests for Specific Exercises ==========

    [Fact]
    public void ChestExercises_ShouldHaveCorrectProperties()
    {
        // Arrange
        var chestExercises = ExerciseLibrary.GetByMuscleGroup(MuscleGroup.Chest);
        var benchPress = chestExercises.FirstOrDefault(e => e.Name == "Barbell Bench Press");

        // Assert
        Assert.NotNull(benchPress);
        Assert.Equal(3, benchPress.Difficulty);
        Assert.Equal("Flat bench barbell press", benchPress.Description);
        Assert.Equal("Barbell, bench", benchPress.Equipment);
    }

    [Fact]
    public void Deadlift_ShouldHaveHighestDifficulty()
    {
        // Arrange
        var allExercises = ExerciseLibrary.GetAllExercises();
        var deadlift = allExercises.FirstOrDefault(e => e.Name == "Deadlift");

        // Assert
        Assert.NotNull(deadlift);
        Assert.Equal(5, deadlift.Difficulty);
    }

    [Fact]
    public void PushUps_ShouldHaveLowestDifficulty()
    {
        // Arrange
        var allExercises = ExerciseLibrary.GetAllExercises();
        var pushUps = allExercises.FirstOrDefault(e => e.Name == "Push-ups");

        // Assert
        Assert.NotNull(pushUps);
        Assert.Equal(1, pushUps.Difficulty);
        Assert.Equal("None", pushUps.Equipment);
    }

    [Fact]
    public void LegExercises_ShouldIncludeCompoundAndIsolation()
    {
        // Arrange
        var legExercises = ExerciseLibrary.GetByMuscleGroup(MuscleGroup.Legs);
        
        // Act
        var hasCompound = legExercises.Any(e => e.Name == "Barbell Squat");
        var hasIsolation = legExercises.Any(e => e.Name == "Leg Extension");

        // Assert
        Assert.True(hasCompound, "Should have compound exercise like Squat");
        Assert.True(hasIsolation, "Should have isolation exercise like Leg Extension");
    }

    // ========== Integration Tests ==========

    [Fact]
    public void GetAllExercises_ShouldCoverAllMuscleGroups()
    {
        // Act
        var exercises = ExerciseLibrary.GetAllExercises();
        var muscleGroups = exercises.Select(e => e.MuscleGroup).Distinct().ToList();

        // Assert
        Assert.Contains(MuscleGroup.Chest, muscleGroups);
        Assert.Contains(MuscleGroup.Back, muscleGroups);
        Assert.Contains(MuscleGroup.Legs, muscleGroups);
        Assert.Contains(MuscleGroup.Shoulders, muscleGroups);
        Assert.Contains(MuscleGroup.Arms, muscleGroups);
        Assert.Contains(MuscleGroup.Core, muscleGroups);
    }

    [Fact]
    public void GetByMuscleGroup_ShouldNotModifyOriginalList()
    {
        // Arrange
        var original = ExerciseLibrary.GetAllExercises();
        var originalCount = original.Count;

        // Act
        var chestExercises = ExerciseLibrary.GetByMuscleGroup(MuscleGroup.Chest);
        var afterCount = ExerciseLibrary.GetAllExercises().Count;

        // Assert
        Assert.Equal(originalCount, afterCount);
        Assert.True(chestExercises.Count < originalCount);
    }
}
namespace GymFlow.Tests.Dal.Seed.Generators;

public class UserGeneratorTest
{
    private readonly SeedOptions _options;

    public UserGeneratorTest()
    {
        _options = SeedProfiles.Lightweight;
        _options.RandomSeed = 42;
    }

    // ========== Helper Methods ==========

    private Person CreateTestPerson(int id = 1, string username = "testuser")
    {
        return new Person
        {
            Id = id,
            FirstName = "Test",
            LastName = "User",
            Username = username,
            Password = "password123",
            Email = $"{username}@test.com",
            Gender = Gender.Male,
            Age = 30,
            Weight = 80f,
            Height = 180f,
            BodyType = BodyType.Fit,
            CreatedAt = DateTime.UtcNow
        };
    }

    private UserGenerator CreateGenerator(int startId = 1)
    {
        return new UserGenerator(_options, startId);
    }

    // ========== Tests for CreateFromPerson - Demo Mode ==========

    [Fact]
    public void CreateFromPerson_WithDemoMode_ShouldCreateUserWithCorrectProperties()
    {
        // Arrange
        var generator = CreateGenerator(1);
        var person = CreateTestPerson(10, "demouser");

        // Act
        var user = generator.CreateFromPerson(person, isDemo: true);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(1, user.Id);
        Assert.Equal(person.Id, user.PersonId);
        Assert.Equal(Goal.MuscleGain, user.Goal);
        Assert.Equal(2500, user.EstimatedCaloriesIntake);
        Assert.Equal(person.CreatedAt, user.CreatedAt);
    }

    [Fact]
    public void CreateFromPerson_WithDemoMode_ShouldIncrementCurrentId()
    {
        // Arrange
        var generator = CreateGenerator(10);
        var person = CreateTestPerson(1, "demouser1");

        // Act
        var user1 = generator.CreateFromPerson(person, isDemo: true);
        var currentIdAfterFirst = generator.CurrentId;
        var user2 = generator.CreateFromPerson(person, isDemo: true);
        var currentIdAfterSecond = generator.CurrentId;

        // Assert
        Assert.Equal(10, user1.Id);
        Assert.Equal(11, currentIdAfterFirst);
        Assert.Equal(11, user2.Id);
        Assert.Equal(12, currentIdAfterSecond);
    }

    [Fact]
    public void CreateFromPerson_WithDemoMode_ShouldUsePersonCreatedAt()
    {
        // Arrange
        var generator = CreateGenerator(1);
        var customDate = new DateTime(2023, 5, 15);
        var person = CreateTestPerson(1, "demouser");
        person.CreatedAt = customDate;

        // Act
        var user = generator.CreateFromPerson(person, isDemo: true);

        // Assert
        Assert.Equal(customDate, user.CreatedAt);
    }

    // ========== Tests for CreateFromPerson - Random Mode ==========

    [Fact]
    public void CreateFromPerson_WithRandomMode_ShouldCreateUserWithValidProperties()
    {
        // Arrange
        var generator = CreateGenerator(1);
        var person = CreateTestPerson(1, "randomuser");

        // Act
        var user = generator.CreateFromPerson(person, isDemo: false);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(1, user.Id);
        Assert.Equal(person.Id, user.PersonId);
        Assert.IsType<Goal>(user.Goal);
        Assert.Equal(person.CreatedAt, user.CreatedAt);
    }

    [Fact]
    public void CreateFromPerson_WithRandomMode_ShouldGenerateValidGoals()
    {
        // Arrange
        var generator = CreateGenerator(1);
        var person = CreateTestPerson(1, "goaluser");
        var validGoals = new[] { Goal.FatLoss, Goal.MuscleGain, Goal.Fitness };

        // Act
        var goals = new List<Goal>();
        for (int i = 0; i < 50; i++)
        {
            var user = generator.CreateFromPerson(person, isDemo: false);
            goals.Add(user.Goal);
        }

        // Assert
        Assert.All(goals, g => Assert.Contains(g, validGoals));
    }

    [Fact]
    public void CreateFromPerson_WithRandomMode_ShouldGenerateAllPossibleGoals()
    {
        // Arrange
        var generator = CreateGenerator(1);
        var person = CreateTestPerson(1, "allgoalsuser");
        var goalsFound = new HashSet<Goal>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            var user = generator.CreateFromPerson(person, isDemo: false);
            goalsFound.Add(user.Goal);
        }

        // Assert
        Assert.Equal(3, goalsFound.Count); // FatLoss, MuscleGain, Fitness
    }

    [Fact]
    public void CreateFromPerson_WithRandomMode_ShouldSometimesHaveCaloriesIntake()
    {
        // Arrange
        var generator = CreateGenerator(1);
        var person = CreateTestPerson(1, "caloriesuser");
        var hasCaloriesCount = 0;
        var totalAttempts = 100;

        // Act
        for (int i = 0; i < totalAttempts; i++)
        {
            var user = generator.CreateFromPerson(person, isDemo: false);
            if (user.EstimatedCaloriesIntake.HasValue)
            {
                hasCaloriesCount++;
                Assert.InRange(user.EstimatedCaloriesIntake.Value, 1800, 3200);
            }
        }

        // Assert - 70% احتمال، پس باید بین 50 تا 90 مورد داشته باشد
        Assert.InRange(hasCaloriesCount, 50, 90);
    }

    [Fact]
    public void CreateFromPerson_WithRandomMode_ShouldIncrementCurrentId()
    {
        // Arrange
        var generator = CreateGenerator(20);
        var person = CreateTestPerson(1, "incrementuser1");
        var person2 = CreateTestPerson(2, "incrementuser2");

        // Act
        var user1 = generator.CreateFromPerson(person, isDemo: false);
        var currentIdAfterFirst = generator.CurrentId;
        var user2 = generator.CreateFromPerson(person2, isDemo: false);
        var currentIdAfterSecond = generator.CurrentId;

        // Assert
        Assert.Equal(20, user1.Id);
        Assert.Equal(21, currentIdAfterFirst);
        Assert.Equal(21, user2.Id);
        Assert.Equal(22, currentIdAfterSecond);
    }

    [Fact]
    public void CreateFromPerson_WithRandomMode_ShouldUsePersonCreatedAt()
    {
        // Arrange
        var generator = CreateGenerator(1);
        var customDate = new DateTime(2024, 1, 10);
        var person = CreateTestPerson(1, "randomuser");
        person.CreatedAt = customDate;

        // Act
        var user = generator.CreateFromPerson(person, isDemo: false);

        // Assert
        Assert.Equal(customDate, user.CreatedAt);
    }

    [Fact]
    public void CreateFromPerson_WithRandomMode_ShouldGenerateDifferentGoalsForDifferentCalls()
    {
        // Arrange
        var generator = CreateGenerator(1);
        var person = CreateTestPerson(1, "varietyuser");

        // Act
        var user1 = generator.CreateFromPerson(person, isDemo: false);
        var user2 = generator.CreateFromPerson(person, isDemo: false);
        var user3 = generator.CreateFromPerson(person, isDemo: false);
        var user4 = generator.CreateFromPerson(person, isDemo: false);
        var user5 = generator.CreateFromPerson(person, isDemo: false);

        var goals = new[] { user1.Goal, user2.Goal, user3.Goal, user4.Goal, user5.Goal };

        // Assert - حداقل دو نوع goal مختلف وجود داشته باشد
        Assert.True(goals.Distinct().Count() >= 2, 
            "Expected at least 2 different goals among 5 random users");
    }

    // ========== Tests for CreateFromPerson - Edge Cases ==========

    [Fact]
    public void CreateFromPerson_WithNullPerson_ShouldThrowNullReferenceException()
    {
        // Arrange
        var generator = CreateGenerator(1);

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => generator.CreateFromPerson(null!, false));
    }

    [Fact]
    public void CreateFromPerson_WithMultipleCalls_ShouldGenerateSequentialIds()
    {
        // Arrange
        var generator = CreateGenerator(5);
        var person1 = CreateTestPerson(1, "sequser1");
        var person2 = CreateTestPerson(2, "sequser2");
        var person3 = CreateTestPerson(3, "sequser3");

        // Act
        var user1 = generator.CreateFromPerson(person1, false);
        var user2 = generator.CreateFromPerson(person2, true);
        var user3 = generator.CreateFromPerson(person3, false);

        // Assert
        Assert.Equal(5, user1.Id);
        Assert.Equal(6, user2.Id);
        Assert.Equal(7, user3.Id);
    }

    // ========== Tests for CurrentId Property ==========

    [Fact]
    public void CurrentId_ShouldStartWithCorrectValue()
    {
        // Arrange & Act
        var generator1 = CreateGenerator(1);
        var generator2 = CreateGenerator(50);
        var generator3 = CreateGenerator(100);

        // Assert
        Assert.Equal(1, generator1.CurrentId);
        Assert.Equal(50, generator2.CurrentId);
        Assert.Equal(100, generator3.CurrentId);
    }

    [Fact]
    public void CurrentId_ShouldNotChangeWithoutCreatingUser()
    {
        // Arrange
        var generator = CreateGenerator(10);

        // Act
        var idBefore = generator.CurrentId;
        var idAfter = generator.CurrentId;

        // Assert
        Assert.Equal(10, idBefore);
        Assert.Equal(10, idAfter);
    }

    [Fact]
    public void CurrentId_ShouldUpdateAfterCreatingUsers()
    {
        // Arrange
        var generator = CreateGenerator(1);
        var person = CreateTestPerson(1, "currentiduser");

        // Act
        var initialId = generator.CurrentId;
        var user1 = generator.CreateFromPerson(person, false);
        var afterFirstId = generator.CurrentId;
        var user2 = generator.CreateFromPerson(person, true);
        var afterSecondId = generator.CurrentId;

        // Assert
        Assert.Equal(1, initialId);
        Assert.Equal(1, user1.Id);
        Assert.Equal(2, afterFirstId);
        Assert.Equal(2, user2.Id);
        Assert.Equal(3, afterSecondId);
    }

    // ========== Integration Tests ==========

    [Fact]
    public void CreateMultipleUsers_ShouldGenerateUniqueIds()
    {
        // Arrange
        var generator = CreateGenerator(1);
        var persons = new List<Person>();
        for (int i = 1; i <= 20; i++)
        {
            persons.Add(CreateTestPerson(i, $"user{i}"));
        }

        // Act
        var users = new List<User>();
        foreach (var person in persons)
        {
            var isDemo = person.Id % 3 == 0;
            users.Add(generator.CreateFromPerson(person, isDemo));
        }

        // Assert
        var ids = users.Select(u => u.Id).ToList();
        var distinctIds = ids.Distinct().ToList();
        Assert.Equal(ids.Count, distinctIds.Count);
        Assert.Equal(Enumerable.Range(1, 20).ToList(), ids.OrderBy(x => x).ToList());
    }

    [Fact]
    public void CreateFromPerson_ShouldPreservePersonId()
    {
        // Arrange
        var generator = CreateGenerator(1);
        var person1 = CreateTestPerson(5, "preserve1");
        var person2 = CreateTestPerson(10, "preserve2");
        var person3 = CreateTestPerson(15, "preserve3");

        // Act
        var user1 = generator.CreateFromPerson(person1, false);
        var user2 = generator.CreateFromPerson(person2, true);
        var user3 = generator.CreateFromPerson(person3, false);

        // Assert
        Assert.Equal(5, user1.PersonId);
        Assert.Equal(10, user2.PersonId);
        Assert.Equal(15, user3.PersonId);
    }

    [Fact]
    public void DemoMode_ShouldAlwaysCreateMuscleGainGoal()
    {
        // Arrange
        var generator = CreateGenerator(1);
        var person = CreateTestPerson(1, "demogalusere");

        // Act
        var user1 = generator.CreateFromPerson(person, true);
        var user2 = generator.CreateFromPerson(person, true);
        var user3 = generator.CreateFromPerson(person, true);

        // Assert
        Assert.Equal(Goal.MuscleGain, user1.Goal);
        Assert.Equal(Goal.MuscleGain, user2.Goal);
        Assert.Equal(Goal.MuscleGain, user3.Goal);
    }

    [Fact]
    public void DemoMode_ShouldAlwaysHaveCaloriesIntake()
    {
        // Arrange
        var generator = CreateGenerator(1);
        var person = CreateTestPerson(1, "democaloriesuser");

        // Act
        var user1 = generator.CreateFromPerson(person, true);
        var user2 = generator.CreateFromPerson(person, true);
        var user3 = generator.CreateFromPerson(person, true);

        // Assert
        Assert.NotNull(user1.EstimatedCaloriesIntake);
        Assert.NotNull(user2.EstimatedCaloriesIntake);
        Assert.NotNull(user3.EstimatedCaloriesIntake);
        Assert.Equal(2500, user1.EstimatedCaloriesIntake);
        Assert.Equal(2500, user2.EstimatedCaloriesIntake);
        Assert.Equal(2500, user3.EstimatedCaloriesIntake);
    }

    [Fact]
    public void RandomMode_ShouldGenerateDifferentCaloriesIntake()
    {
        // Arrange
        var generator = CreateGenerator(1);
        var person = CreateTestPerson(1, "randomcaloriesuser");
        var caloriesValues = new List<int>();

        // Act
        for (int i = 0; i < 50; i++)
        {
            var user = generator.CreateFromPerson(person, false);
            if (user.EstimatedCaloriesIntake.HasValue)
            {
                caloriesValues.Add(user.EstimatedCaloriesIntake.Value);
            }
        }

        // Assert - حداقل چند مقدار مختلف داشته باشد
        var distinctNonNull = caloriesValues.Distinct().ToList();
        Assert.True(distinctNonNull.Count >= 3, 
            $"Expected at least 3 different calorie values, got {distinctNonNull.Count}");
    }
}
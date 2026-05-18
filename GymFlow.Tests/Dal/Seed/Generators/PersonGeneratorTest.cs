namespace GymFlow.Tests.Dal.Seed.Generators;

public class PersonGeneratorTest
{
    private readonly SeedOptions _options;

    public PersonGeneratorTest()
    {
        _options = SeedProfiles.Lightweight;
        // تنظیم RandomSeed ثابت برای تست‌های قابل پیش‌بینی
        _options.RandomSeed = 42;
    }

    // ========== Helper Methods ==========

    private PersonGenerator CreateGenerator(int startId = 1)
    {
        return new PersonGenerator(_options, startId);
    }

    // ========== Tests for CreateDemoCoach ==========

    [Fact]
    public void CreateDemoCoach_ShouldCreateCoachWithCorrectProperties()
    {
        // Arrange
        var generator = CreateGenerator(1);

        // Act
        var coach = generator.CreateDemoCoach();

        // Assert
        Assert.NotNull(coach);
        Assert.Equal(1, coach.Id);
        Assert.Equal("Master", coach.FirstName);
        Assert.Equal("Coach", coach.LastName);
        Assert.Equal("coach", coach.Username);
        Assert.Equal("coach123", coach.Password);
        Assert.Equal("coach@gymflow.com", coach.Email);
        Assert.Equal("+1 (555) 000-0001", coach.Phone);
        Assert.Equal(Gender.Male, coach.Gender);
        Assert.Equal(35, coach.Age);
        Assert.Equal(85.5f, coach.Weight);
        Assert.Equal(182f, coach.Height);
        Assert.Equal(BodyType.Fit, coach.BodyType);
        Assert.True(coach.CreatedAt <= DateTime.UtcNow);
        Assert.True(coach.CreatedAt >= DateTime.UtcNow.AddMonths(-6).AddDays(-1));
    }

    [Fact]
    public void CreateDemoCoach_ShouldIncrementCurrentId()
    {
        // Arrange
        var generator = CreateGenerator(5);

        // Act
        var coach = generator.CreateDemoCoach();
        var currentIdAfter = generator.CurrentId;

        // Assert
        Assert.Equal(5, coach.Id);
        Assert.Equal(6, currentIdAfter);
    }

    // ========== Tests for CreateDemoMember ==========

    [Fact]
    public void CreateDemoMember_ShouldCreateMemberWithCorrectProperties()
    {
        // Arrange
        var generator = CreateGenerator(1);

        // Act
        var member = generator.CreateDemoMember();

        // Assert
        Assert.NotNull(member);
        Assert.Equal(1, member.Id);
        Assert.Equal("John", member.FirstName);
        Assert.Equal("Doe", member.LastName);
        Assert.Equal("member", member.Username);
        Assert.Equal("member123", member.Password);
        Assert.Equal("member@gymflow.com", member.Email);
        Assert.Equal("+1 (555) 000-0002", member.Phone);
        Assert.Equal(Gender.Male, member.Gender);
        Assert.Equal(25, member.Age);
        Assert.Equal(75.0f, member.Weight);
        Assert.Equal(175f, member.Height);
        Assert.Equal(BodyType.Fit, member.BodyType);
        Assert.True(member.CreatedAt <= DateTime.UtcNow);
        Assert.True(member.CreatedAt >= DateTime.UtcNow.AddMonths(-3).AddDays(-1));
    }

    [Fact]
    public void CreateDemoMember_ShouldIncrementCurrentId()
    {
        // Arrange
        var generator = CreateGenerator(10);

        // Act
        var member = generator.CreateDemoMember();
        var currentIdAfter = generator.CurrentId;

        // Assert
        Assert.Equal(10, member.Id);
        Assert.Equal(11, currentIdAfter);
    }

    // ========== Tests for CreateRandom ==========

    [Fact]
    public void CreateRandom_ShouldCreatePersonWithValidProperties()
    {
        // Arrange
        var generator = CreateGenerator(1);

        // Act
        var person = generator.CreateRandom();

        // Assert
        Assert.NotNull(person);
        Assert.Equal(1, person.Id);
        Assert.NotEmpty(person.FirstName);
        Assert.NotEmpty(person.LastName);
        Assert.NotEmpty(person.Username);
        Assert.Equal("password123", person.Password);
        Assert.NotNull(person.Email);
        Assert.NotNull(person.Phone);
        Assert.InRange(person.Age, 18, 55);
        Assert.True(person.Weight.HasValue);
        Assert.True(person.Height.HasValue);
        Assert.NotNull(person.BodyType);
        Assert.True(person.CreatedAt <= DateTime.UtcNow);
        Assert.True(person.CreatedAt >= DateTime.UtcNow.AddYears(-1));
    }

    [Fact]
    public void CreateRandom_ShouldGenerateCorrectGenderBasedWeight()
    {
        // Arrange
        var generator = CreateGenerator(1);
        var maleWeights = new List<float>();
        var femaleWeights = new List<float>();

        // Act - ایجاد چندین نفر برای نمونه‌گیری
        for (int i = 0; i < 50; i++)
        {
            var person = generator.CreateRandom();
            Assert.True(person.Weight.HasValue);
            
            if (person.Gender == Gender.Male)
                maleWeights.Add(person.Weight.Value);
            else if (person.Gender == Gender.Female)
                femaleWeights.Add(person.Weight.Value);
        }

        // Assert
        // وزن آقایان باید بین 65 تا 110 باشد
        if (maleWeights.Any())
        {
            Assert.All(maleWeights, w => Assert.InRange(w, 65f, 110f));
        }
        // وزن خانم‌ها باید بین 50 تا 85 باشد
        if (femaleWeights.Any())
        {
            Assert.All(femaleWeights, w => Assert.InRange(w, 50f, 85f));
        }
    }

    [Fact]
    public void CreateRandom_ShouldGenerateCorrectGenderBasedHeight()
    {
        // Arrange
        var generator = CreateGenerator(1);
        var maleHeights = new List<float>();
        var femaleHeights = new List<float>();

        // Act
        for (int i = 0; i < 50; i++)
        {
            var person = generator.CreateRandom();
            Assert.True(person.Height.HasValue);
            
            if (person.Gender == Gender.Male)
                maleHeights.Add(person.Height.Value);
            else if (person.Gender == Gender.Female)
                femaleHeights.Add(person.Height.Value);
        }

        // Assert
        // قد آقایان باید بین 170 تا 190 باشد
        if (maleHeights.Any())
        {
            Assert.All(maleHeights, h => Assert.InRange(h, 170f, 190f));
        }
        // قد خانم‌ها باید بین 155 تا 175 باشد
        if (femaleHeights.Any())
        {
            Assert.All(femaleHeights, h => Assert.InRange(h, 155f, 175f));
        }
    }

    [Fact]
    public void CreateRandom_ShouldGenerateUniqueUsernames()
    {
        // Arrange
        var generator = CreateGenerator(1);
        var usernames = new HashSet<string>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            var person = generator.CreateRandom();
            usernames.Add(person.Username);
        }

        // Assert
        Assert.Equal(100, usernames.Count);
    }

    [Fact]
    public void CreateRandom_ShouldGenerateValidEmailFormat()
    {
        // Arrange
        var generator = CreateGenerator(1);

        // Act
        for (int i = 0; i < 50; i++)
        {
            var person = generator.CreateRandom();

            // Assert
            Assert.NotNull(person.Email);
            Assert.Contains("@", person.Email);
            Assert.Contains(".", person.Email);
        }
    }

    [Fact]
    public void CreateRandom_ShouldGenerateValidPhoneNumber()
    {
        // Arrange
        var generator = CreateGenerator(1);

        // Act
        for (int i = 0; i < 50; i++)
        {
            var person = generator.CreateRandom();

            // Assert
            Assert.NotNull(person.Phone);
            Assert.NotEmpty(person.Phone);
        }
    }

    [Fact]
    public void CreateRandom_ShouldIncrementCurrentId()
    {
        // Arrange
        var generator = CreateGenerator(20);

        // Act
        var initialId = generator.CurrentId;
        var person1 = generator.CreateRandom();
        var afterFirstId = generator.CurrentId;
        var person2 = generator.CreateRandom();
        var afterSecondId = generator.CurrentId;

        // Assert
        Assert.Equal(20, initialId);
        Assert.Equal(20, person1.Id);
        Assert.Equal(21, afterFirstId);
        Assert.Equal(21, person2.Id);
        Assert.Equal(22, afterSecondId);
    }

    [Fact]
    public void CreateRandom_MayHaveUpdatedAt()
    {
        // Arrange
        var options = SeedProfiles.Lightweight;
        options.RandomSeed = 42;
        var generator = new PersonGenerator(options, 1);
        var hasUpdatedAt = false;
        var attempts = 0;
        var maxAttempts = 200;

        // Act - تا 200 بار تلاش می‌کنیم تا یک نمونه با UpdatedAt پیدا کنیم
        while (!hasUpdatedAt && attempts < maxAttempts)
        {
            var person = generator.CreateRandom();
            if (person.UpdatedAt.HasValue)
            {
                hasUpdatedAt = true;
                Assert.True(person.UpdatedAt.Value <= DateTime.UtcNow);
                Assert.True(person.UpdatedAt.Value >= DateTime.UtcNow.AddDays(-30));
            }
            attempts++;
        }

        // Assert - در 200 بار تلاش، احتمال اینکه هیچ UpdatedAt نداشته باشیم بسیار کم است
        Assert.True(hasUpdatedAt, 
            $"Expected at least one person to have UpdatedAt after {maxAttempts} attempts. " +
            "This suggests the probability might be lower than expected.");
    }

    // ========== Tests for DetermineBodyType (Private - Test via Reflection) ==========

    [Fact]
    public void DetermineBodyType_ShouldReturnCorrectBodyTypeBasedOnBMI()
    {
        // Arrange
        var generator = CreateGenerator(1);

        // Act & Assert - با استفاده از Reflection متد خصوصی را تست می‌کنیم
        var methodInfo = typeof(PersonGenerator).GetMethod("DetermineBodyType", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        Assert.NotNull(methodInfo);
        
        // تست مقادیر مختلف
        var testCases = new[]
        {
            new { Weight = 45f, Height = 170f, Expected = BodyType.LeanMuscular }, // BMI = 15.6
            new { Weight = 60f, Height = 170f, Expected = BodyType.Fit },          // BMI = 20.8
            new { Weight = 80f, Height = 170f, Expected = BodyType.Overweight },    // BMI = 27.7
            new { Weight = 100f, Height = 170f, Expected = BodyType.Obese }         // BMI = 34.6
        };
        
        foreach (var testCase in testCases)
        {
            var result = methodInfo.Invoke(generator, new object[] { testCase.Weight, testCase.Height });
            Assert.Equal(testCase.Expected, result);
        }
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
    public void CurrentId_ShouldNotChangeWithoutCreatingPerson()
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

    // ========== Integration Tests ==========

    [Fact]
    public void CreateMultiplePersons_ShouldGenerateUniqueIds()
    {
        // Arrange
        var generator = CreateGenerator(1);
        var persons = new List<Person>();

        // Act
        for (int i = 0; i < 20; i++)
        {
            if (i % 3 == 0)
                persons.Add(generator.CreateDemoCoach());
            else if (i % 3 == 1)
                persons.Add(generator.CreateDemoMember());
            else
                persons.Add(generator.CreateRandom());
        }

        // Assert
        var ids = persons.Select(p => p.Id).ToList();
        var distinctIds = ids.Distinct().ToList();
        Assert.Equal(ids.Count, distinctIds.Count);
    }

    [Fact]
    public void CreateRandom_ShouldGeneratePersonsWithAllGenders()
    {
        // Arrange
        var generator = CreateGenerator(1);
        var genders = new HashSet<Gender>();

        // Act
        for (int i = 0; i < 50; i++)
        {
            var person = generator.CreateRandom();
            genders.Add(person.Gender);
        }

        // Assert
        Assert.Contains(Gender.Male, genders);
        Assert.Contains(Gender.Female, genders);
    }

    [Fact]
    public void CreateRandom_ShouldGenerateValidAgeRange()
    {
        // Arrange
        var generator = CreateGenerator(1);

        // Act
        for (int i = 0; i < 100; i++)
        {
            var person = generator.CreateRandom();
            
            // Assert
            Assert.InRange(person.Age, 18, 55);
        }
    }

    [Fact]
    public void CreateRandom_ShouldSetCreatedAtInPast()
    {
        // Arrange
        var generator = CreateGenerator(1);

        // Act
        for (int i = 0; i < 50; i++)
        {
            var person = generator.CreateRandom();
            
            // Assert
            Assert.True(person.CreatedAt <= DateTime.UtcNow);
            Assert.True(person.CreatedAt >= DateTime.UtcNow.AddYears(-1));
        }
    }

    [Fact]
    public void DemoCoachesAndMembers_ShouldHaveCorrectCreatedAtDates()
    {
        // Arrange
        var generator = CreateGenerator(1);
        var now = DateTime.UtcNow;

        // Act
        var coach = generator.CreateDemoCoach();
        var member = generator.CreateDemoMember();

        // Assert
        Assert.True(coach.CreatedAt <= now.AddMonths(-6).AddDays(1));
        Assert.True(coach.CreatedAt >= now.AddMonths(-6).AddDays(-1));
        
        Assert.True(member.CreatedAt <= now.AddMonths(-3).AddDays(1));
        Assert.True(member.CreatedAt >= now.AddMonths(-3).AddDays(-1));
    }

    [Fact]
    public void CreateRandom_WithDifferentSeeds_ShouldGenerateDifferentResults()
    {
        // Arrange - ایجاد دو نمونه مستقل از SeedOptions با seedهای متفاوت
        var options1 = new SeedOptions
        {
            RandomSeed = 123,
            UserCount = 10
        };
        
        var options2 = new SeedOptions
        {
            RandomSeed = 456,
            UserCount = 10
        };
        
        var generator1 = new PersonGenerator(options1, 1);
        var generator2 = new PersonGenerator(options2, 1);

        // Act
        var person1 = generator1.CreateRandom();
        var person2 = generator2.CreateRandom();

        // Assert
        // احتمال اینکه با seedهای مختلف نتایج یکسان بیاید بسیار کم است
        bool isDifferent = person1.FirstName != person2.FirstName ||
                          person1.LastName != person2.LastName ||
                          person1.Username != person2.Username;
        Assert.True(isDifferent, "Expected different results with different seeds");
    }
}
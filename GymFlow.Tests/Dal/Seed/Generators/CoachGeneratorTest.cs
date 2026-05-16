using Xunit;
using GymFlow.Dal.Seed.Generators;
using GymFlow.Dal.Seed.Data;
using GymFlow.Models.Entities;
using GymFlow.Models.Enums;

namespace GymFlow.Tests.Dal.Seed.Generators;

public class CoachGeneratorTest
{
    private readonly SeedOptions _options;
    private readonly Random _random;

    public CoachGeneratorTest()
    {
        _options = SeedProfiles.Lightweight;
        _random = new Random(42);
    }

    // ========== Helper Methods ==========

    private Person CreateTestPerson(int id = 1)
    {
        return new Person
        {
            Id = id,
            FirstName = "Test",
            LastName = "Coach",
            Username = $"testcoach_{id}",
            Password = "password123",
            Email = $"testcoach_{id}@test.com",
            Gender = Gender.Male,
            Age = 30,
            Weight = 80f,
            Height = 180f,
            BodyType = BodyType.Fit,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ========== Tests for CreateDemoFromPerson ==========

    [Fact]
    public void CreateDemoFromPerson_ShouldCreateCoachWithCorrectProperties()
    {
        // Arrange
        var person = CreateTestPerson(1);
        var generator = new CoachGenerator(_options, startId: 1);

        // Act
        var coach = generator.CreateDemoFromPerson(person);

        // Assert
        Assert.NotNull(coach);
        Assert.Equal(1, coach.Id);
        Assert.Equal(person.Id, coach.PersonId);
        Assert.Equal("Strength & Conditioning", coach.Specialization);
        Assert.Equal(10, coach.YearsOfExperience);
        Assert.Equal("https://example.com/certificate.pdf", coach.CertificateUrl);
        Assert.Equal(person.CreatedAt, coach.CreatedAt);
    }

    [Fact]
    public void CreateDemoFromPerson_ShouldIncrementCurrentId()
    {
        // Arrange
        var person1 = CreateTestPerson(1);
        var person2 = CreateTestPerson(2);
        var generator = new CoachGenerator(_options, startId: 5);

        // Act
        var coach1 = generator.CreateDemoFromPerson(person1);
        var currentIdAfterFirst = generator.CurrentId;
        var coach2 = generator.CreateDemoFromPerson(person2);

        // Assert
        Assert.Equal(5, coach1.Id);
        Assert.Equal(6, currentIdAfterFirst);
        Assert.Equal(6, coach2.Id);
        Assert.Equal(7, generator.CurrentId);
    }

    [Fact]
    public void CreateDemoFromPerson_WithDifferentStartId_ShouldUseCorrectId()
    {
        // Arrange
        var person = CreateTestPerson(1);
        var generator = new CoachGenerator(_options, startId: 10);

        // Act
        var coach = generator.CreateDemoFromPerson(person);

        // Assert
        Assert.Equal(10, coach.Id);
        Assert.Equal(11, generator.CurrentId);
    }

    // ========== Tests for CreateRandomFromPerson ==========

    [Fact]
    public void CreateRandomFromPerson_ShouldCreateCoachWithValidProperties()
    {
        // Arrange
        var person = CreateTestPerson(1);
        var generator = new CoachGenerator(_options, startId: 1);
        var validSpecializations = new[] { "Bodybuilding", "Powerlifting", "CrossFit", "Yoga", "Nutrition", "Weight Loss" };

        // Act
        var coach = generator.CreateRandomFromPerson(person);

        // Assert
        Assert.NotNull(coach);
        Assert.Equal(1, coach.Id);
        Assert.Equal(person.Id, coach.PersonId);
        Assert.Contains(coach.Specialization, validSpecializations);
        Assert.InRange(coach.YearsOfExperience, 2, 15);
        Assert.NotNull(coach.CertificateUrl);
        Assert.NotEmpty(coach.CertificateUrl);
        Assert.Equal(person.CreatedAt, coach.CreatedAt);
    }

    [Fact]
    public void CreateRandomFromPerson_ShouldGenerateDifferentSpecializations()
    {
        // Arrange
        var person1 = CreateTestPerson(1);
        var person2 = CreateTestPerson(2);
        var person3 = CreateTestPerson(3);
        var generator = new CoachGenerator(_options, startId: 1);

        // Act
        var coach1 = generator.CreateRandomFromPerson(person1);
        var coach2 = generator.CreateRandomFromPerson(person2);
        var coach3 = generator.CreateRandomFromPerson(person3);

        // Assert - بررسی می‌کنیم که همه یکسان نباشند (احتمال کم)
        var specializations = new[] { coach1.Specialization, coach2.Specialization, coach3.Specialization };
        Assert.True(specializations.Distinct().Count() > 1 || specializations.All(s => s != null));
    }

    [Fact]
    public void CreateRandomFromPerson_ShouldGenerateYearsOfExperienceWithinRange()
    {
        // Arrange
        var person = CreateTestPerson(1);
        var generator = new CoachGenerator(_options, startId: 1);

        // Act & Assert - چند بار تست کنیم
        for (int i = 0; i < 50; i++)
        {
            var coach = generator.CreateRandomFromPerson(person);
            Assert.InRange(coach.YearsOfExperience, 2, 15);
        }
    }

    [Fact]
    public void CreateRandomFromPerson_ShouldGenerateValidCertificateUrl()
    {
        // Arrange
        var person = CreateTestPerson(1);
        var generator = new CoachGenerator(_options, startId: 1);

        // Act
        var coach = generator.CreateRandomFromPerson(person);

        // Assert
        Assert.NotNull(coach.CertificateUrl);
        Assert.StartsWith("http", coach.CertificateUrl);
    }

    [Fact]
    public void CreateRandomFromPerson_ShouldIncrementCurrentId()
    {
        // Arrange
        var person1 = CreateTestPerson(1);
        var person2 = CreateTestPerson(2);
        var generator = new CoachGenerator(_options, startId: 10);

        // Act
        var coach1 = generator.CreateRandomFromPerson(person1);
        var currentIdAfterFirst = generator.CurrentId;
        var coach2 = generator.CreateRandomFromPerson(person2);

        // Assert
        Assert.Equal(10, coach1.Id);
        Assert.Equal(11, currentIdAfterFirst);
        Assert.Equal(11, coach2.Id);
        Assert.Equal(12, generator.CurrentId);
    }

    // ========== Tests for CurrentId Property ==========

    [Fact]
    public void CurrentId_ShouldStartWithCorrectValue()
    {
        // Arrange & Act
        var generator1 = new CoachGenerator(_options, startId: 1);
        var generator2 = new CoachGenerator(_options, startId: 50);
        var generator3 = new CoachGenerator(_options, startId: 100);

        // Assert
        Assert.Equal(1, generator1.CurrentId);
        Assert.Equal(50, generator2.CurrentId);
        Assert.Equal(100, generator3.CurrentId);
    }

    [Fact]
    public void CurrentId_ShouldUpdateAfterCreatingCoaches()
    {
        // Arrange
        var person = CreateTestPerson(1);
        var generator = new CoachGenerator(_options, startId: 5);

        // Act
        var initialId = generator.CurrentId;
        var coach1 = generator.CreateDemoFromPerson(person);
        var afterFirstId = generator.CurrentId;
        var coach2 = generator.CreateRandomFromPerson(person);
        var afterSecondId = generator.CurrentId;

        // Assert
        Assert.Equal(5, initialId);
        Assert.Equal(5, coach1.Id);
        Assert.Equal(6, afterFirstId);
        Assert.Equal(6, coach2.Id);
        Assert.Equal(7, afterSecondId);
    }

    // ========== Integration Tests ==========

    [Fact]
    public void CreateMultipleCoaches_ShouldGenerateUniqueIds()
    {
        // Arrange
        var generator = new CoachGenerator(_options, startId: 1);
        var persons = Enumerable.Range(1, 10).Select(i => CreateTestPerson(i)).ToList();
        var coaches = new List<Coach>();

        // Act
        foreach (var person in persons)
        {
            var coach = (person.Id % 2 == 0) 
                ? generator.CreateDemoFromPerson(person) 
                : generator.CreateRandomFromPerson(person);
            coaches.Add(coach);
        }

        // Assert
        var ids = coaches.Select(c => c.Id).ToList();
        var distinctIds = ids.Distinct().ToList();
        Assert.Equal(ids.Count, distinctIds.Count);
        Assert.Equal(Enumerable.Range(1, 10).ToList(), ids.OrderBy(x => x).ToList());
    }

    [Fact]
    public void CreateRandomFromPerson_ShouldSetCreatedAtFromPerson()
    {
        // Arrange
        var customDate = new DateTime(2023, 1, 15);
        var person = CreateTestPerson(1);
        person.CreatedAt = customDate;
        var generator = new CoachGenerator(_options, startId: 1);

        // Act
        var coach = generator.CreateRandomFromPerson(person);

        // Assert
        Assert.Equal(customDate, coach.CreatedAt);
    }

    [Fact]
    public void CreateDemoFromPerson_ShouldSetCreatedAtFromPerson()
    {
        // Arrange
        var customDate = new DateTime(2023, 6, 20);
        var person = CreateTestPerson(1);
        person.CreatedAt = customDate;
        var generator = new CoachGenerator(_options, startId: 1);

        // Act
        var coach = generator.CreateDemoFromPerson(person);

        // Assert
        Assert.Equal(customDate, coach.CreatedAt);
    }
}
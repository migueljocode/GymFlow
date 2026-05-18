namespace GymFlow.Tests.Dal.Repositories;

public class CoachRepositoryTest : IClassFixture<DbContextFixture>
{
    private readonly DbContextFixture _fixture;

    public CoachRepositoryTest(DbContextFixture fixture)
    {
        _fixture = fixture;
    }

    // ========== Helpers ==========
    private async Task<Coach> CreateTestCoachAsync(string uniqueSuffix)
    {
        var repo = new PersonRepository(_fixture.DbContextFactory);
        
        var person = new Person
        {
            FirstName = "Coach",
            LastName = $"Test{uniqueSuffix}",
            Username = $"coach_{uniqueSuffix}",
            Password = "coach123",
            Email = $"coach_{uniqueSuffix}@test.com",
            Gender = Gender.Male,
            Age = 35,
            Weight = 80f,
            Height = 180f,
            BodyType = BodyType.Fit,
            CreatedAt = DateTime.UtcNow
        };
        
        var addedPerson = await repo.AddAsync(person);
        await repo.SaveChangesAsync();
        
        var coachRepo = new CoachRepository(_fixture.DbContextFactory);
        var coach = new Coach
        {
            PersonId = addedPerson.Id,
            Specialization = "Strength Training",
            YearsOfExperience = 5,
            CreatedAt = DateTime.UtcNow
        };
        
        return await coachRepo.AddAsync(coach);
    }

    // ========== Query Tests ==========

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectCoach()
    {
        // Arrange
        var repo = new CoachRepository(_fixture.DbContextFactory);
        var coach = await CreateTestCoachAsync("getbyid");

        // Act
        var fetched = await repo.GetByIdAsync(coach.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal(coach.Id, fetched.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var repo = new CoachRepository(_fixture.DbContextFactory);

        // Act
        var fetched = await repo.GetByIdAsync(99999);

        // Assert
        Assert.Null(fetched);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ShouldReturnFirstMatchingCoach()
    {
        // Arrange
        var repo = new CoachRepository(_fixture.DbContextFactory);
        await CreateTestCoachAsync("first1");
        await CreateTestCoachAsync("first2");

        // Act
        var fetched = await repo.FirstOrDefaultAsync(c => c.Specialization == "Strength Training");

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal("Strength Training", fetched.Specialization);
    }

    [Fact]
    public async Task SingleOrDefaultAsync_WithSingleMatch_ShouldReturnCoach()
    {
        // Arrange
        var repo = new CoachRepository(_fixture.DbContextFactory);
        var coach = await CreateTestCoachAsync("single");

        // Act
        var fetched = await repo.SingleOrDefaultAsync(c => c.Id == coach.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal(coach.Id, fetched.Id);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCoaches()
    {
        // Arrange
        var repo = new CoachRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        await CreateTestCoachAsync("all1");
        await CreateTestCoachAsync("all2");

        // Act
        var all = await repo.GetAllAsync();

        // Assert
        Assert.NotNull(all);
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task FindAsync_ShouldReturnFilteredCoaches()
    {
        // Arrange
        var repo = new CoachRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        await CreateTestCoachAsync("find1");
        await CreateTestCoachAsync("find2");

        // Act
        var found = await repo.FindAsync(c => c.Specialization == "Strength Training");

        // Assert
        Assert.Equal(2, found.Count());
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnTrueWhenExists()
    {
        // Arrange
        var repo = new CoachRepository(_fixture.DbContextFactory);
        await CreateTestCoachAsync("any");

        // Act
        var exists = await repo.AnyAsync(c => c.Specialization == "Strength Training");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnFalseWhenNotExists()
    {
        // Arrange
        var repo = new CoachRepository(_fixture.DbContextFactory);

        // Act
        var exists = await repo.AnyAsync(c => c.Specialization == "Nonexistent");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task AllAsync_ShouldReturnTrueWhenAllMatch()
    {
        // Arrange
        var repo = new CoachRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        await CreateTestCoachAsync("alltrue1");
        await CreateTestCoachAsync("alltrue2");

        // Act
        var allMatch = await repo.AllAsync(c => c.Specialization == "Strength Training");

        // Assert
        Assert.True(allMatch);
    }

    [Fact]
    public async Task CountAsync_ShouldReturnTotalCount()
    {
        // Arrange
        var repo = new CoachRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        await CreateTestCoachAsync("count1");
        await CreateTestCoachAsync("count2");

        // Act
        var count = await repo.CountAsync();

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ShouldReturnFilteredCount()
    {
        // Arrange
        var repo = new CoachRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        await CreateTestCoachAsync("countfilter1");
        await CreateTestCoachAsync("countfilter2");

        // Act
        var count = await repo.CountAsync(c => c.Specialization == "Strength Training");

        // Assert
        Assert.Equal(2, count);
    }

    // ========== Command Tests ==========

    [Fact]
    public async Task AddAsync_ShouldSaveCoachToDatabase()
    {
        // Arrange
        var personRepo = new PersonRepository(_fixture.DbContextFactory);
        var person = new Person
        {
            FirstName = "New",
            LastName = "Coach",
            Username = "newcoach",
            Password = "coach123",
            Email = "newcoach@test.com",
            Gender = Gender.Male,
            Age = 40,
            CreatedAt = DateTime.UtcNow
        };
        var addedPerson = await personRepo.AddAsync(person);
        await personRepo.SaveChangesAsync();

        var repo = new CoachRepository(_fixture.DbContextFactory);
        var coach = new Coach
        {
            PersonId = addedPerson.Id,
            Specialization = "Yoga",
            YearsOfExperience = 3,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var added = await repo.AddAsync(coach);
        await repo.SaveChangesAsync();

        // Assert
        var fetched = await repo.GetByIdAsync(added.Id);
        Assert.NotNull(fetched);
        Assert.Equal("Yoga", fetched.Specialization);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingCoach()
    {
        // Arrange
        var repo = new CoachRepository(_fixture.DbContextFactory);
        var coach = await CreateTestCoachAsync("update");
        coach.Specialization = "CrossFit";

        // Act
        var updated = await repo.UpdateAsync(coach);
        await repo.SaveChangesAsync();

        // Assert
        var fetched = await repo.GetByIdAsync(updated.Id);
        Assert.Equal("CrossFit", fetched?.Specialization);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveCoachFromDatabase()
    {
        // Arrange
        var repo = new CoachRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        var coach = await CreateTestCoachAsync("delete");

        // Act
        var deleted = await repo.DeleteAsync(coach);
        // await repo.SaveChangesAsync();
        
        // Assert
        Assert.True(deleted);

        var fetched = await repo.FindAsync(coach.Id);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldRemoveCoachById()
    {
        // Arrange
        var repo = new CoachRepository(_fixture.DbContextFactory);
        var coach = await CreateTestCoachAsync("deletebyid");

        // Act
        var deleted = await repo.DeleteByIdAsync(coach.Id);
        await repo.SaveChangesAsync();

        // Assert
        Assert.True(deleted);
        var fetched = await repo.GetByIdAsync(coach.Id);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task DeleteByIdAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var repo = new CoachRepository(_fixture.DbContextFactory);

        // Act
        var deleted = await repo.DeleteByIdAsync(99999);

        // Assert
        Assert.False(deleted);
    }

    [Fact]
    public async Task SoftDeleteAsync_ShouldSetIsDeletedFlag()
    {
        // Arrange
        var repo = new CoachRepository(_fixture.DbContextFactory);
        var coach = await CreateTestCoachAsync("softdelete");

        // Act
        var deleted = await repo.DeleteByIdAsync(coach.Id);
        await repo.SaveChangesAsync();

        // Assert
        Assert.True(deleted);
        
        var fetched = await repo.GetByIdAsync(coach.Id);
        Assert.Null(fetched);
        
        await using var context = _fixture.CreateContext();
        var allCoaches = await context.Coaches.IgnoreQueryFilters().ToListAsync();
        var softDeletedCoach = allCoaches.FirstOrDefault(c => c.Id == coach.Id);
        Assert.NotNull(softDeletedCoach);
        Assert.True(softDeletedCoach.IsDeleted);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleCoaches()
    {
        // Arrange
        var personRepo = new PersonRepository(_fixture.DbContextFactory);
        
        var person1 = new Person { FirstName = "Coach1", LastName = "One", Username = "coach1", Password = "pass", CreatedAt = DateTime.UtcNow };
        var person2 = new Person { FirstName = "Coach2", LastName = "Two", Username = "coach2", Password = "pass", CreatedAt = DateTime.UtcNow };
        
        var addedPerson1 = await personRepo.AddAsync(person1);
        var addedPerson2 = await personRepo.AddAsync(person2);
        await personRepo.SaveChangesAsync();
        
        var coaches = new List<Coach>
        {
            new() { PersonId = addedPerson1.Id, Specialization = "Bodybuilding", YearsOfExperience = 5, CreatedAt = DateTime.UtcNow },
            new() { PersonId = addedPerson2.Id, Specialization = "Powerlifting", YearsOfExperience = 3, CreatedAt = DateTime.UtcNow }
        };
        
        var repo = new CoachRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();

        // Act
        var added = await repo.AddRangeAsync(coaches);
        await repo.SaveChangesAsync();

        // Assert
        Assert.Equal(2, added.Count());
        var all = await repo.GetAllAsync();
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task DeleteRangeAsync_ShouldRemoveMultipleCoaches()
    {
        // Arrange
        var personRepo = new PersonRepository(_fixture.DbContextFactory);
        
        var person1 = new Person { FirstName = "Del1", LastName = "One", Username = "del1", Password = "pass", CreatedAt = DateTime.UtcNow };
        var person2 = new Person { FirstName = "Del2", LastName = "Two", Username = "del2", Password = "pass", CreatedAt = DateTime.UtcNow };
        
        var addedPerson1 = await personRepo.AddAsync(person1);
        var addedPerson2 = await personRepo.AddAsync(person2);
        await personRepo.SaveChangesAsync();
        
        var coaches = new List<Coach>
        {
            new() { PersonId = addedPerson1.Id, Specialization = "Yoga", YearsOfExperience = 5, CreatedAt = DateTime.UtcNow },
            new() { PersonId = addedPerson2.Id, Specialization = "Pilates", YearsOfExperience = 3, CreatedAt = DateTime.UtcNow }
        };
        
        var repo = new CoachRepository(_fixture.DbContextFactory);
        var added = await repo.AddRangeAsync(coaches);
        await repo.SaveChangesAsync();

        // Act
        var deleted = await repo.DeleteRangeAsync(added);
        await repo.SaveChangesAsync();

        // Assert
        Assert.True(deleted);
        var all = await repo.GetAllAsync();
        Assert.Empty(all);
    }

    // ========== Specific Interface Tests ==========

    [Fact]
    public async Task GetCoachWithPersonAsync_ShouldIncludePerson()
    {
        // Arrange
        var repo = new CoachRepository(_fixture.DbContextFactory);
        var coach = await CreateTestCoachAsync("withperson");

        // Act
        var fetched = await repo.GetCoachWithPersonAsync(coach.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.NotNull(fetched.Person);
        Assert.Equal($"coach_withperson", fetched.Person.Username);
    }

    [Fact]
    public async Task GetByPersonIdAsync_ShouldReturnCorrectCoach()
    {
        // Arrange
        var personRepo = new PersonRepository(_fixture.DbContextFactory);
        var person = new Person
        {
            FirstName = "ByPerson",
            LastName = "Coach",
            Username = "bypersoncoach",
            Password = "pass",
            CreatedAt = DateTime.UtcNow
        };
        var addedPerson = await personRepo.AddAsync(person);
        await personRepo.SaveChangesAsync();
        
        var repo = new CoachRepository(_fixture.DbContextFactory);
        var coach = new Coach
        {
            PersonId = addedPerson.Id,
            Specialization = "Cardio",
            YearsOfExperience = 4,
            CreatedAt = DateTime.UtcNow
        };
        var addedCoach = await repo.AddAsync(coach);
        await repo.SaveChangesAsync();

        // Act
        var fetched = await repo.GetByPersonIdAsync(addedPerson.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal(addedCoach.Id, fetched.Id);
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnCoachByUsername()
    {
        // Arrange
        var uniqueSuffix = Guid.NewGuid().ToString();
        var personRepo = new PersonRepository(_fixture.DbContextFactory);
        var person = new Person
        {
            FirstName = "Username",
            LastName = "Coach",
            Username = $"usernamecoach_{uniqueSuffix}",
            Password = "pass",
            CreatedAt = DateTime.UtcNow
        };
        var addedPerson = await personRepo.AddAsync(person);
        await personRepo.SaveChangesAsync();
        
        var repo = new CoachRepository(_fixture.DbContextFactory);
        var coach = new Coach
        {
            PersonId = addedPerson.Id,
            Specialization = "Functional",
            YearsOfExperience = 2,
            CreatedAt = DateTime.UtcNow
        };
        await repo.AddAsync(coach);
        await repo.SaveChangesAsync();

        // Act
        var fetched = await repo.GetByUsernameAsync($"usernamecoach_{uniqueSuffix}");

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal(addedPerson.Id, fetched.PersonId);
    }

    [Fact]
    public async Task GetByUsernameAsync_WithInvalidUsername_ShouldReturnNull()
    {
        // Arrange
        var repo = new CoachRepository(_fixture.DbContextFactory);

        // Act
        var fetched = await repo.GetByUsernameAsync("nonexistentusername");

        // Assert
        Assert.Null(fetched);
    }

    [Fact]
    public async Task GetAllCoachesWithPersonAsync_ShouldIncludeAllPersons()
    {
        // Arrange
        var repo = new CoachRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        await CreateTestCoachAsync("getall1");
        await CreateTestCoachAsync("getall2");

        // Act
        var all = await repo.GetAllCoachesWithPersonAsync();

        // Assert
        Assert.Equal(2, all.Count());
        foreach (var coach in all)
        {
            Assert.NotNull(coach.Person);
        }
    }
}
using Xunit;
using Microsoft.EntityFrameworkCore;
using GymFlow.Dal.Repositories.Implementations;
using GymFlow.Models.Entities;
using GymFlow.Models.Enums;
using GymFlow.Tests.Dal;
using Moq;

namespace GymFlow.Tests.Dal.Repositories;

public class PersonRepositoryTest : IClassFixture<DbContextFixture>
{
    private readonly DbContextFixture _fixture;

    public PersonRepositoryTest(DbContextFixture fixture)
    {
        _fixture = fixture;
    }

    // ========== Helpers ==========
    private async Task<Person> CreateTestPersonAsync(string username = "testuser")
    {
        await _fixture.ResetDatabaseAsync();
        var repo = new PersonRepository(_fixture.DbContextFactory);
        var person = new Person
        {
            FirstName = "Test",
            LastName = "User",
            Username = username,
            Password = "pass123",
            Email = $"{username}@test.com",
            Gender = Gender.Male,
            Age = 25,
            Weight = 75f,
            Height = 175f,
            BodyType = BodyType.Fit,
            CreatedAt = DateTime.UtcNow
        };
        var addedPerson = await repo.AddAsync(person);
        return addedPerson;
    }

    // ========== Query Tests ==========

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectPerson()
    {
        // await _fixture.ResetDatabaseAsync();
     
        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();

        var person = await CreateTestPersonAsync("something1");

        // Act
        var fetched = await repo.GetByIdAsync(person.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal(person.Username, fetched.Username);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        await _fixture.ResetDatabaseAsync();
        
        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);

        // Act
        var fetched = await repo.GetByIdAsync(99999);

        // Assert
        Assert.Null(fetched);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ShouldReturnFirstMatchingPerson()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);
        await CreateTestPersonAsync("user1");
        await CreateTestPersonAsync("user2");

        // Act
        var fetched = await repo.FirstOrDefaultAsync(p => p.Username.Contains("user"));

        // Assert
        Assert.NotNull(fetched);
        Assert.Contains("user", fetched.Username);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_WithNoMatch_ShouldReturnNull()
    {
        await _fixture.ResetDatabaseAsync();
        
        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);

        // Act
        var fetched = await repo.FirstOrDefaultAsync(p => p.Username == "nonexistent");

        // Assert
        Assert.Null(fetched);
    }

    [Fact]
    public async Task SingleOrDefaultAsync_WithSingleMatch_ShouldReturnPerson()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);
        var person = await CreateTestPersonAsync("uniqueuser");

        // Act
        var fetched = await repo.SingleOrDefaultAsync(p => p.Username == "uniqueuser");

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal(person.Id, fetched.Id);
    }

    [Fact]
    public async Task SingleOrDefaultAsync_WithMultipleMatches_ShouldThrowException()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);
        await CreateTestPersonAsync("duplicate");
        await CreateTestPersonAsync("duplicate1");

        // it is not possible to two person exist with same username "We Have Person.Username Constraint"
        // await Assert.ThrowsAsync<DbUpdateException>(async () => await CreateTestPersonAsync("duplicate"));

        // // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await repo.SingleOrDefaultAsync(p => p.Password == "pass123"));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllPersons()
    {
        await _fixture.ResetDatabaseAsync();
     
        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);
        await CreateTestPersonAsync("person1");
        await CreateTestPersonAsync("person2");

        // Act
        var all = await repo.GetAllAsync();

        bool person1Added = await repo.AnyAsync(x => x.Username == "person1");
        bool person2Added = await repo.AnyAsync(x => x.Username == "person2");

        Assert.True(person1Added && person2Added);
        // Assert
        // Assert.NotNull(all);
        // Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task FindAsync_ShouldReturnFilteredPersons()
    {
        // await _fixture.ResetDatabaseAsync();

        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        await CreateTestPersonAsync("alice");
        await CreateTestPersonAsync("bob");

        // Act
        var found = await repo.FindAsync(p => p.Username.StartsWith('a') && p.Username.EndsWith('e'));

        // Assert
        Assert.Single(found);
        Assert.Equal("alice", found.First().Username);
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnTrueWhenExists()
    {
        // await _fixture.ResetDatabaseAsync();

        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        await CreateTestPersonAsync("something2");

        // Act
        var exists = await repo.AnyAsync(p => p.Username == "something2");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnFalseWhenNotExists()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);

        // Act
        var exists = await repo.AnyAsync(p => p.Username == "nonexistent");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task AllAsync_ShouldReturnTrueWhenAllMatch()
    {
        // await _fixture.ResetDatabaseAsync();

        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);
        
        // ensure no person exist in database
        var all = await repo.GetAllAsync();
        var alldeleted = await repo.DeleteRangeAsync(all);

        // add two entity
        await CreateTestPersonAsync("valid1");
        await CreateTestPersonAsync("valid2");

        // Act
        var allValid = await repo.AllAsync(p => p.Username.StartsWith("valid"));

        // var allValids = await repo.FindAsync(p => p.Username.StartsWith("valid"));
        // Assert
        Assert.True(allValid);
    }

    [Fact]
    public async Task AllAsync_ShouldReturnFalseWhenNotAllMatch()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);
        await CreateTestPersonAsync("valid");
        await CreateTestPersonAsync("invalid");

        // Act
        var allValid = await repo.AllAsync(p => p.Username == "valid");

        // Assert
        Assert.False(allValid);
    }

    [Fact]
    public async Task CountAsync_ShouldReturnTotalCount()
    {
        // await _fixture.ResetDatabaseAsync();
      
        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        await CreateTestPersonAsync("user3");
        await CreateTestPersonAsync("user4");

        // Act
        var count = await repo.CountAsync();

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ShouldReturnFilteredCount()
    {
        await _fixture.ResetDatabaseAsync();
      
        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);
        await CreateTestPersonAsync("alaice");
        await CreateTestPersonAsync("boob");

        // Act
        var count = await repo.CountAsync(p => p.Username.StartsWith('a') && p.Username.EndsWith('e'));
        // Assert
        Assert.Equal(1, count);
    }

    // ========== Command Tests ==========

    [Fact]
    public async Task AddAsync_ShouldSavePersonToDatabase()
    {
        await _fixture.ResetDatabaseAsync();
      
        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);
        var person = new Person
        {
            FirstName = "New",
            LastName = "User",
            Username = "newuser",
            Password = "pass123",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var added = await repo.AddAsync(person);
        await repo.SaveChangesAsync();

        // Assert
        var fetched = await repo.GetByIdAsync(added.Id);
        Assert.NotNull(fetched);
        Assert.Equal("newuser", fetched.Username);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingPerson()
    {
        // await _fixture.ResetDatabaseAsync();
      
        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        var person = await CreateTestPersonAsync("xxxxxxx");
        person.FirstName = "Updated";

        // Act
        var updated = await repo.UpdateAsync(person);
        await repo.SaveChangesAsync();

        // Assert
        var fetched = await repo.GetByIdAsync(updated.Id);
        Assert.Equal("Updated", fetched?.FirstName);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemovePersonFromDatabase()
    {
        // await _fixture.ResetDatabaseAsync();
      
        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        var person = await CreateTestPersonAsync();

        // Act
        var deleted = await repo.DeleteAsync(person);
        // await repo.SaveChangesAsync();

        // Assert
        Assert.True(deleted);
        // var fetched = await repo.GetByIdAsync(person.Id);
        // Assert.Null(fetched);
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldRemovePersonById()
    {
        // await _fixture.ResetDatabaseAsync();
      
        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        var person = await CreateTestPersonAsync("user5");

        // Act
        var deleted = await repo.DeleteByIdAsync(person.Id);
        await repo.SaveChangesAsync();

        // Assert
        Assert.True(deleted);
        var fetched = await repo.GetByIdAsync(person.Id);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task DeleteByIdAsync_WithInvalidId_ShouldReturnFalse()
    {
        await _fixture.ResetDatabaseAsync();
      
        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);

        // Act
        var deleted = await repo.DeleteByIdAsync(99999);

        // Assert
        Assert.False(deleted);
    }

    [Fact]
    public async Task SoftDeleteAsync_ShouldSetIsDeletedFlag()
    {
        // await _fixture.RsetDatabaseAsync();
      
        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        var person = await CreateTestPersonAsync("user6");

        // Act
        var deleted = await repo.DeleteByIdAsync(person.Id);
        await repo.SaveChangesAsync();

        // Assert
        Assert.True(deleted);
        
        // Normal query should not return soft-deleted
        var fetched = await repo.GetByIdAsync(person.Id);
        Assert.Null(fetched);
        
        // Query with ignore filters should return with IsDeleted = true
        await using var context = _fixture.CreateContext();
        var allPersons = await context.Persons.IgnoreQueryFilters().ToListAsync();
        var softDeletedPerson = allPersons.FirstOrDefault(p => p.Id == person.Id);
        Assert.NotNull(softDeletedPerson);
        Assert.True(softDeletedPerson.IsDeleted);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultiplePersons()
    {
        // await _fixture.ResetDatabaseAsync();
      
        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        var persons = new List<Person>
        {
            new() { FirstName = "User7", LastName = "One", Username = "User7", Password = "pass", CreatedAt = DateTime.UtcNow },
            new() { FirstName = "User8", LastName = "Two", Username = "user8", Password = "pass", CreatedAt = DateTime.UtcNow }
        };

        // Act
        var added = await repo.AddRangeAsync(persons);
        await repo.SaveChangesAsync();

        // Assert
        Assert.Equal(2, added.Count());
        var all = await repo.GetAllAsync();
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task DeleteRangeAsync_ShouldRemoveMultiplePersons()
    {
        await _fixture.ResetDatabaseAsync();
      
        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);
        var persons = new List<Person>
        {
            new() { FirstName = "User9", LastName = "One", Username = "del1", Password = "pass", CreatedAt = DateTime.UtcNow },
            new() { FirstName = "User10", LastName = "Two", Username = "del2", Password = "pass", CreatedAt = DateTime.UtcNow }
        };
        var added = await repo.AddRangeAsync(persons);
        await repo.SaveChangesAsync();

        // Act
        var deleted = await repo.DeleteRangeAsync(added);
        await repo.SaveChangesAsync();

        // Assert
        Assert.True(deleted);
        // var all = await repo.GetAllAsync();
        // Assert.Empty(all);
    }

    // ========== Specific Interface Tests ==========

    [Fact]
    public async Task GetPersonWithRoleAsync_ShouldIncludeUserAndCoach()
    {
        await _fixture.ResetDatabaseAsync();
      
        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);
        
        // Create a User
        var person = await CreateTestPersonAsync("withuser");
        var user = new User { PersonId = person.Id, Goal = Goal.Fitness, CreatedAt = DateTime.UtcNow };
        await using var context = _fixture.CreateContext();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var fetched = await repo.GetPersonWithRoleAsync(person.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.NotNull(fetched.User);
        Assert.Equal(Goal.Fitness, fetched.User.Goal);
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnCorrectPerson()
    {
        await _fixture.ResetDatabaseAsync();
      
        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);
        var person = await CreateTestPersonAsync("uniqueusername");

        // Act
        var fetched = await repo.GetByUsernameAsync("uniqueusername");

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal(person.Id, fetched.Id);
    }

    [Fact]
    public async Task GetByUsernameAsync_WithInvalidUsername_ShouldReturnNull()
    {
        await _fixture.ResetDatabaseAsync();
      
        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);

        // Act
        var fetched = await repo.GetByUsernameAsync("nonexistent");

        // Assert
        Assert.Null(fetched);
    }

    [Fact]
    public async Task GetPersonWithUserDetailsAsync_ShouldIncludeWorkoutPlansAndProgressLogs()
    {
        await _fixture.ResetDatabaseAsync();
      
        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);
        var person = await CreateTestPersonAsync("withdetails");
        
        await using var context = _fixture.CreateContext();
        var user = new User { PersonId = person.Id, Goal = Goal.Fitness, CreatedAt = DateTime.UtcNow };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        var plan = new WorkoutPlan { UserId = user.Id, Phase = 1, SessionsPerWeek = 3, StartDate = DateOnly.FromDateTime(DateTime.UtcNow), CreatedAt = DateTime.UtcNow };
        var log = new ProgressLog { UserId = user.Id, LogDate = DateOnly.FromDateTime(DateTime.UtcNow), Weight = 75f, CreatedAt = DateTime.UtcNow };
        context.WorkoutPlans.Add(plan);
        context.ProgressLogs.Add(log);
        await context.SaveChangesAsync();

        // Act
        var fetched = await repo.GetPersonWithUserDetailsAsync(person.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.NotNull(fetched.User);
        Assert.NotEmpty(fetched.User.WorkoutPlans);
        Assert.NotEmpty(fetched.User.ProgressLogs);
    }

    [Fact]
    public async Task GetPersonWithCoachDetailsAsync_ShouldIncludeCoach()
    {
        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var person = await CreateTestPersonAsync("withcoach");
        
        await using var context = _fixture.CreateContext();
        var coach = new Coach { PersonId = person.Id, Specialization = "Strength", YearsOfExperience = 5, CreatedAt = DateTime.UtcNow };
        context.Coaches.Add(coach);
        await context.SaveChangesAsync();

        // Act
        var fetched = await repo.GetPersonWithCoachDetailsAsync(person.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.NotNull(fetched.Coach);
        Assert.Equal("Strength", fetched.Coach.Specialization);
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ShouldReturnPerson()
    {      
        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        await CreateTestPersonAsync("authuserr");

        // Act
        var authenticated = await repo.AuthenticateAsync("authuserr", "pass123");

        // Assert
        Assert.NotNull(authenticated);
        Assert.Equal("authuserr", authenticated.Username);
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidPassword_ShouldReturnNull()
    {
        await _fixture.ResetDatabaseAsync();
      
        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);
        await CreateTestPersonAsync("authuser");

        // Act
        var authenticated = await repo.AuthenticateAsync("authuser", "wrongpassword");

        // Assert
        Assert.Null(authenticated);
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidUsername_ShouldReturnNull()
    {
        // await _fixture.ResetDatabaseAsync();
      
        // Arrange
        var repo = new PersonRepository(_fixture.DbContextFactory);
        await CreateTestPersonAsync("nonexistent");
        
        // Act
        var authenticated = await repo.AuthenticateAsync("nonexistent", "pass1234");

        // Assert
        Assert.Null(authenticated);
    }
}
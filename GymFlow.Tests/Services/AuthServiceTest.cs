using Xunit;
using Microsoft.EntityFrameworkCore;
using GymFlow.Dal.Context;
using GymFlow.Dal.Factories;
using GymFlow.Models.Entities;
using GymFlow.Models.Enums;
using GymFlow.Services.Implementations;

namespace GymFlow.Tests.Services;

public class AuthServiceTest : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly AuthService _authService;
    private readonly string _dbName;

    public AuthServiceTest()
    {
        _dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"DataSource=file:{_dbName}.db?mode=memory&cache=shared")
            .Options;
        
        _context = new AppDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
        
        _factory = new AppDbContextFactory(options);
        _authService = new AuthService(_factory);
    }

    // ========== Helper Methods ==========

    private async Task<Person> CreateTestPersonAsync(string username, string password, bool createUser = true)
    {
        var person = new Person
        {
            Username = username,
            Password = password,
            FirstName = "Test",
            LastName = "User",
            Email = $"{username}@test.com",
            Gender = Gender.Male,
            Age = 25,
            Weight = 75f,
            Height = 175f,
            BodyType = BodyType.Fit,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Persons.Add(person);
        await _context.SaveChangesAsync();
        
        if (createUser)
        {
            var user = new User
            {
                PersonId = person.Id,
                Goal = Goal.Fitness,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        
        return person;
    }

    // ========== AuthenticateAsync Tests ==========

    [Fact]
    public async Task AuthenticateAsync_WithValidCoachCredentials_ShouldReturnUser()
    {
        // Arrange
        await CreateTestPersonAsync("coach", "coach123");

        // Act
        var result = await _authService.AuthenticateAsync("coach", "coach123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("coach", result.Person?.Username);
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidMemberCredentials_ShouldReturnUser()
    {
        // Arrange
        await CreateTestPersonAsync("member", "member123");

        // Act
        var result = await _authService.AuthenticateAsync("member", "member123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("member", result.Person?.Username);
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ShouldReturnUserWithCorrectProperties()
    {
        // Arrange
        await CreateTestPersonAsync("testuser", "password123");

        // Act
        var result = await _authService.AuthenticateAsync("testuser", "password123");

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Person);
        Assert.Equal("testuser", result.Person.Username);
        Assert.Equal("password123", result.Person.Password);
        Assert.Equal("Test", result.Person.FirstName);
        Assert.Equal("User", result.Person.LastName);
    }

    [Fact]
    public async Task AuthenticateAsync_WithWrongPassword_ShouldReturnNull()
    {
        // Arrange
        await CreateTestPersonAsync("testuser", "correct123");

        // Act
        var result = await _authService.AuthenticateAsync("testuser", "wrongpassword");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAsync_WithNonExistentUser_ShouldReturnNull()
    {
        // Act
        var result = await _authService.AuthenticateAsync("nonexistent", "anypassword");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAsync_WithEmptyUsername_ShouldReturnNull()
    {
        // Arrange
        await CreateTestPersonAsync("testuser", "password123");

        // Act
        var result = await _authService.AuthenticateAsync("", "password123");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAsync_WithEmptyPassword_ShouldReturnNull()
    {
        // Arrange
        await CreateTestPersonAsync("testuser", "password123");

        // Act
        var result = await _authService.AuthenticateAsync("testuser", "");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAsync_WithNullUsername_ShouldReturnNull()
    {
        // Arrange
        await CreateTestPersonAsync("testuser", "password123");

        // Act
        var result = await _authService.AuthenticateAsync(null!, "password123");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAsync_WithNullPassword_ShouldReturnNull()
    {
        // Arrange
        await CreateTestPersonAsync("testuser", "password123");

        // Act
        var result = await _authService.AuthenticateAsync("testuser", null!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenPersonExistsButNoUser_ShouldReturnNull()
    {
        // Arrange - فقط Person بساز بدون User
        await CreateTestPersonAsync("persononly", "pass123", createUser: false);

        // Act
        var result = await _authService.AuthenticateAsync("persononly", "pass123");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAsync_CaseSensitiveUsername_ShouldWork()
    {
        // Arrange
        await CreateTestPersonAsync("TestUser", "password123");

        // Act
        var resultLower = await _authService.AuthenticateAsync("testuser", "password123");
        var resultUpper = await _authService.AuthenticateAsync("TESTUSER", "password123");
        var resultExact = await _authService.AuthenticateAsync("TestUser", "password123");

        // Assert
        Assert.Null(resultLower);
        Assert.Null(resultUpper);
        Assert.NotNull(resultExact);
    }

    [Fact]
    public async Task AuthenticateAsync_CaseSensitivePassword_ShouldWork()
    {
        // Arrange
        await CreateTestPersonAsync("testuser", "Password123");

        // Act
        var resultLower = await _authService.AuthenticateAsync("testuser", "password123");
        var resultUpper = await _authService.AuthenticateAsync("testuser", "PASSWORD123");
        var resultExact = await _authService.AuthenticateAsync("testuser", "Password123");

        // Assert
        Assert.Null(resultLower);
        Assert.Null(resultUpper);
        Assert.NotNull(resultExact);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnUserWithPersonIncluded()
    {
        // Arrange
        await CreateTestPersonAsync("testuser", "password123");

        // Act
        var result = await _authService.AuthenticateAsync("testuser", "password123");

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Person);
        Assert.Equal("testuser", result.Person.Username);
        Assert.Equal("Test", result.Person.FirstName);
        Assert.Equal("User", result.Person.LastName);
    }

    [Fact]
    public async Task AuthenticateAsync_MultipleUsers_ShouldAuthenticateCorrectly()
    {
        // Arrange
        await CreateTestPersonAsync("user1", "pass1");
        await CreateTestPersonAsync("user2", "pass2");
        await CreateTestPersonAsync("user3", "pass3");

        // Act
        var result1 = await _authService.AuthenticateAsync("user1", "pass1");
        var result2 = await _authService.AuthenticateAsync("user2", "pass2");
        var result3 = await _authService.AuthenticateAsync("user3", "pass3");
        var wrongUser = await _authService.AuthenticateAsync("user1", "wrong");

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotNull(result3);
        Assert.Null(wrongUser);
        Assert.Equal("user1", result1.Person?.Username);
        Assert.Equal("user2", result2.Person?.Username);
        Assert.Equal("user3", result3.Person?.Username);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldHandleSpecialCharactersInPassword()
    {
        // Arrange
        var specialPassword = "P@ssw0rd!@#$%^&*()";
        await CreateTestPersonAsync("specialuser", specialPassword);

        // Act
        var result = await _authService.AuthenticateAsync("specialuser", specialPassword);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("specialuser", result.Person?.Username);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldHandleLongUsernameAndPassword()
    {
        // Arrange
        var longUsername = new string('a', 100);
        var longPassword = new string('b', 100);
        await CreateTestPersonAsync(longUsername, longPassword);

        // Act
        var result = await _authService.AuthenticateAsync(longUsername, longPassword);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(longUsername, result.Person?.Username);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnNull_WhenPasswordIsEmptyString()
    {
        // Arrange
        await CreateTestPersonAsync("testuser", "password123");

        // Act
        var result = await _authService.AuthenticateAsync("testuser", string.Empty);

        // Assert
        Assert.Null(result);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }
}
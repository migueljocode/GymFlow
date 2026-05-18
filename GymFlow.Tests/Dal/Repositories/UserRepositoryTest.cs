namespace GymFlow.Tests.Dal.Repositories;

public class UserRepositoryTest : IClassFixture<DbContextFixture>
{
    private readonly DbContextFixture _fixture;

    public UserRepositoryTest(DbContextFixture fixture)
    {
        _fixture = fixture;
    }

    // ========== Helpers ==========
    private async Task<User> CreateTestUserAsync(string uniqueSuffix, Goal goal = Goal.Fitness)
    {
        var personRepo = new PersonRepository(_fixture.DbContextFactory);
        
        var person = new Person
        {
            FirstName = "Test",
            LastName = $"User{uniqueSuffix}",
            Username = $"testuser_{uniqueSuffix}",
            Password = "pass123",
            Email = $"testuser_{uniqueSuffix}@test.com",
            Gender = Gender.Male,
            Age = 25,
            Weight = 75f,
            Height = 175f,
            BodyType = BodyType.Fit,
            CreatedAt = DateTime.UtcNow
        };
        
        var addedPerson = await personRepo.AddAsync(person);
        await personRepo.SaveChangesAsync();
        
        var userRepo = new UserRepository(_fixture.DbContextFactory);
        var user = new User
        {
            PersonId = addedPerson.Id,
            Goal = goal,
            EstimatedCaloriesIntake = 2500,
            CreatedAt = DateTime.UtcNow
        };
        
        return await userRepo.AddAsync(user);
    }

    // ========== Query Tests ==========

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectUser()
    {
        // Arrange
        var repo = new UserRepository(_fixture.DbContextFactory);
        var user = await CreateTestUserAsync("getbyid");

        // Act
        var fetched = await repo.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal(user.Id, fetched.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var repo = new UserRepository(_fixture.DbContextFactory);

        // Act
        var fetched = await repo.GetByIdAsync(99999);

        // Assert
        Assert.Null(fetched);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ShouldReturnFirstMatchingUser()
    {
        // Arrange
        var repo = new UserRepository(_fixture.DbContextFactory);
        await CreateTestUserAsync("first1", Goal.MuscleGain);
        await CreateTestUserAsync("first2", Goal.FatLoss);

        // Act
        var fetched = await repo.FirstOrDefaultAsync(u => u.Goal == Goal.MuscleGain);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal(Goal.MuscleGain, fetched.Goal);
    }

    [Fact]
    public async Task SingleOrDefaultAsync_WithSingleMatch_ShouldReturnUser()
    {
        // Arrange
        var repo = new UserRepository(_fixture.DbContextFactory);
        var user = await CreateTestUserAsync("single");

        // Act
        var fetched = await repo.SingleOrDefaultAsync(u => u.Id == user.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal(user.Id, fetched.Id);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var repo = new UserRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        await CreateTestUserAsync("all1");
        await CreateTestUserAsync("all2");

        // Act
        var all = await repo.GetAllAsync();

        // Assert
        Assert.NotNull(all);
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task FindAsync_ShouldReturnFilteredUsers()
    {
        // Arrange
        var repo = new UserRepository(_fixture.DbContextFactory);
        await CreateTestUserAsync("find1", Goal.MuscleGain);
        await CreateTestUserAsync("find2", Goal.FatLoss);

        // Act
        var found = await repo.FindAsync(u => u.Goal == Goal.MuscleGain);

        // Assert
        Assert.Single(found);
        Assert.Equal(Goal.MuscleGain, found.First().Goal);
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnTrueWhenExists()
    {
        // Arrange
        var repo = new UserRepository(_fixture.DbContextFactory);
        await CreateTestUserAsync("any");

        // Act
        var exists = await repo.AnyAsync(u => u.Goal == Goal.Fitness);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnFalseWhenNotExists()
    {
        // Arrange
        var repo = new UserRepository(_fixture.DbContextFactory);

        // Act
        var exists = await repo.AnyAsync(u => u.Goal == (Goal)999);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task AllAsync_ShouldReturnTrueWhenAllMatch()
    {
        // Arrange
        var repo = new UserRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        await CreateTestUserAsync("alltrue1", Goal.Fitness);
        await CreateTestUserAsync("alltrue2", Goal.Fitness);

        // Act
        var allMatch = await repo.AllAsync(u => u.Goal == Goal.Fitness);

        // Assert
        Assert.True(allMatch);
    }

    [Fact]
    public async Task AllAsync_ShouldReturnFalseWhenNotAllMatch()
    {
        // Arrange
        var repo = new UserRepository(_fixture.DbContextFactory);
        await CreateTestUserAsync("allfalse1", Goal.Fitness);
        await CreateTestUserAsync("allfalse2", Goal.MuscleGain);

        // Act
        var allMatch = await repo.AllAsync(u => u.Goal == Goal.Fitness);

        // Assert
        Assert.False(allMatch);
    }

    [Fact]
    public async Task CountAsync_ShouldReturnTotalCount()
    {
        // Arrange
        var repo = new UserRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        await CreateTestUserAsync("count1");
        await CreateTestUserAsync("count2");

        // Act
        var count = await repo.CountAsync();

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ShouldReturnFilteredCount()
    {
        // Arrange
        var repo = new UserRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        await CreateTestUserAsync("countfilter1", Goal.MuscleGain);
        await CreateTestUserAsync("countfilter2", Goal.FatLoss);

        // Act
        var count = await repo.CountAsync(u => u.Goal == Goal.MuscleGain);

        // Assert
        Assert.Equal(1, count);
    }

    // ========== Command Tests ==========

    [Fact]
    public async Task AddAsync_ShouldSaveUserToDatabase()
    {
        // Arrange
        var personRepo = new PersonRepository(_fixture.DbContextFactory);
        var person = new Person
        {
            FirstName = "New",
            LastName = "User",
            Username = "newuser",
            Password = "pass123",
            Email = "newuser@test.com",
            Gender = Gender.Male,
            Age = 30,
            CreatedAt = DateTime.UtcNow
        };
        var addedPerson = await personRepo.AddAsync(person);
        await personRepo.SaveChangesAsync();

        var repo = new UserRepository(_fixture.DbContextFactory);
        var user = new User
        {
            PersonId = addedPerson.Id,
            Goal = Goal.Fitness,
            EstimatedCaloriesIntake = 2000,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var added = await repo.AddAsync(user);
        await repo.SaveChangesAsync();

        // Assert
        var fetched = await repo.GetByIdAsync(added.Id);
        Assert.NotNull(fetched);
        Assert.Equal(Goal.Fitness, fetched.Goal);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingUser()
    {
        // Arrange
        var repo = new UserRepository(_fixture.DbContextFactory);
        var user = await CreateTestUserAsync("update");
        user.Goal = Goal.MuscleGain;

        // Act
        var updated = await repo.UpdateAsync(user);
        await repo.SaveChangesAsync();

        // Assert
        var fetched = await repo.GetByIdAsync(updated.Id);
        Assert.Equal(Goal.MuscleGain, fetched?.Goal);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteUser()
    {
        // Arrange
        var repo = new UserRepository(_fixture.DbContextFactory);
        var user = await CreateTestUserAsync("delete");

        // Act
        var deleted = await repo.DeleteAsync(user);

        // Assert
        Assert.True(deleted);
        
        var fetched = await repo.GetByIdAsync(user.Id);
        Assert.Null(fetched);
        
        await using var context = _fixture.CreateContext();
        var allUsers = await context.Users.IgnoreQueryFilters().ToListAsync();
        var deletedUser = allUsers.FirstOrDefault(u => u.Id == user.Id);
        Assert.NotNull(deletedUser);
        Assert.True(deletedUser.IsDeleted);
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldSoftDeleteUserById()
    {
        // Arrange
        var repo = new UserRepository(_fixture.DbContextFactory);
        var user = await CreateTestUserAsync("deletebyid");

        // Act
        var deleted = await repo.DeleteByIdAsync(user.Id);
        await repo.SaveChangesAsync();

        // Assert
        Assert.True(deleted);
        var fetched = await repo.GetByIdAsync(user.Id);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task DeleteByIdAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var repo = new UserRepository(_fixture.DbContextFactory);

        // Act
        var deleted = await repo.DeleteByIdAsync(99999);

        // Assert
        Assert.False(deleted);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleUsers()
    {
        // Arrange
        var personRepo = new PersonRepository(_fixture.DbContextFactory);
        
        var person1 = new Person { FirstName = "User1", LastName = "One", Username = "user1", Password = "pass", CreatedAt = DateTime.UtcNow };
        var person2 = new Person { FirstName = "User2", LastName = "Two", Username = "user2", Password = "pass", CreatedAt = DateTime.UtcNow };
        
        var addedPerson1 = await personRepo.AddAsync(person1);
        var addedPerson2 = await personRepo.AddAsync(person2);
        await personRepo.SaveChangesAsync();
        
        var users = new List<User>
        {
            new() { PersonId = addedPerson1.Id, Goal = Goal.Fitness, CreatedAt = DateTime.UtcNow },
            new() { PersonId = addedPerson2.Id, Goal = Goal.MuscleGain, CreatedAt = DateTime.UtcNow }
        };
        
        var repo = new UserRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();

        // Act
        var added = await repo.AddRangeAsync(users);
        await repo.SaveChangesAsync();

        // Assert
        Assert.Equal(2, added.Count());
        var all = await repo.GetAllAsync();
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task DeleteRangeAsync_ShouldSoftDeleteMultipleUsers()
    {
        // Arrange
        var personRepo = new PersonRepository(_fixture.DbContextFactory);
        
        var person1 = new Person { FirstName = "Del1", LastName = "One", Username = "deluser1", Password = "pass", CreatedAt = DateTime.UtcNow };
        var person2 = new Person { FirstName = "Del2", LastName = "Two", Username = "deluser2", Password = "pass", CreatedAt = DateTime.UtcNow };
        
        var addedPerson1 = await personRepo.AddAsync(person1);
        var addedPerson2 = await personRepo.AddAsync(person2);
        await personRepo.SaveChangesAsync();
        
        var users = new List<User>
        {
            new() { PersonId = addedPerson1.Id, Goal = Goal.Fitness, CreatedAt = DateTime.UtcNow },
            new() { PersonId = addedPerson2.Id, Goal = Goal.MuscleGain, CreatedAt = DateTime.UtcNow }
        };
        
        var repo = new UserRepository(_fixture.DbContextFactory);
        var added = await repo.AddRangeAsync(users);
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
    public async Task GetUserWithPersonAsync_ShouldIncludePerson()
    {
        // Arrange
        var repo = new UserRepository(_fixture.DbContextFactory);
        var user = await CreateTestUserAsync("withperson");

        // Act
        var fetched = await repo.GetUserWithPersonAsync(user.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.NotNull(fetched.Person);
        Assert.Equal($"testuser_withperson", fetched.Person.Username);
    }

    [Fact]
    public async Task GetUserWithWorkoutPlansAsync_ShouldIncludeWorkoutPlans()
    {
        // Arrange
        var repo = new UserRepository(_fixture.DbContextFactory);
        var user = await CreateTestUserAsync("withworkout");
        
        // Add a workout plan
        var planRepo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        var plan = new WorkoutPlan
        {
            UserId = user.Id,
            Phase = 1,
            SessionsPerWeek = 3,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedAt = DateTime.UtcNow
        };
        await planRepo.AddAsync(plan);
        await planRepo.SaveChangesAsync();

        // Act
        var fetched = await repo.GetUserWithWorkoutPlansAsync(user.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.NotNull(fetched.WorkoutPlans);
        Assert.NotEmpty(fetched.WorkoutPlans);
    }

    [Fact]
    public async Task GetUserWithProgressLogsAsync_ShouldIncludeProgressLogs()
    {
        // Arrange
        var repo = new UserRepository(_fixture.DbContextFactory);
        var user = await CreateTestUserAsync("withprogress");
        
        // Add a progress log
        var logRepo = new ProgressLogRepository(_fixture.DbContextFactory);
        var log = new ProgressLog
        {
            UserId = user.Id,
            LogDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Weight = 75f,
            CreatedAt = DateTime.UtcNow
        };
        await logRepo.AddAsync(log);
        await logRepo.SaveChangesAsync();

        // Act
        var fetched = await repo.GetUserWithProgressLogsAsync(user.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.NotNull(fetched.ProgressLogs);
        Assert.NotEmpty(fetched.ProgressLogs);
    }

    [Fact]
    public async Task GetByPersonIdAsync_ShouldReturnUserByPersonId()
    {
        // Arrange
        var user = await CreateTestUserAsync("bypersonid");

        // Act
        var fetched = await new UserRepository(_fixture.DbContextFactory).GetByPersonIdAsync(user.PersonId);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal(user.Id, fetched.Id);
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnUserByUsername()
    {
        // Arrange
        var uniqueSuffix = Guid.NewGuid().ToString();
        var personRepo = new PersonRepository(_fixture.DbContextFactory);
        var person = new Person
        {
            FirstName = "Username",
            LastName = "User",
            Username = $"usernameuser_{uniqueSuffix}",
            Password = "pass",
            Email = $"user_{uniqueSuffix}@test.com",
            Gender = Gender.Male,
            Age = 25,
            CreatedAt = DateTime.UtcNow
        };
        var addedPerson = await personRepo.AddAsync(person);
        await personRepo.SaveChangesAsync();
        
        var userRepo = new UserRepository(_fixture.DbContextFactory);
        var user = new User
        {
            PersonId = addedPerson.Id,
            Goal = Goal.Fitness,
            CreatedAt = DateTime.UtcNow
        };
        await userRepo.AddAsync(user);
        await userRepo.SaveChangesAsync();

        // Act
        var fetched = await userRepo.GetByUsernameAsync($"usernameuser_{uniqueSuffix}");

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal(addedPerson.Id, fetched.PersonId);
    }

    [Fact]
    public async Task GetByUsernameAsync_WithInvalidUsername_ShouldReturnNull()
    {
        // Arrange
        var repo = new UserRepository(_fixture.DbContextFactory);

        // Act
        var fetched = await repo.GetByUsernameAsync("nonexistentuser");

        // Assert
        Assert.Null(fetched);
    }

    [Fact]
    public async Task GetAllUsersWithPersonAsync_ShouldIncludeAllPersons()
    {
        // Arrange
        var repo = new UserRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        await CreateTestUserAsync("getall1");
        await CreateTestUserAsync("getall2");

        // Act
        var all = await repo.GetAllUsersWithPersonAsync();

        // Assert
        Assert.Equal(2, all.Count());
        foreach (var user in all)
        {
            Assert.NotNull(user.Person);
        }
    }

    [Fact]
    public async Task GetUserWithCompleteHistoryAsync_ShouldIncludeAllRelatedData()
    {
        // Arrange
        var repo = new UserRepository(_fixture.DbContextFactory);
        var user = await CreateTestUserAsync("completehistory");
        
        // Add workout plan
        var planRepo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        var plan = new WorkoutPlan
        {
            UserId = user.Id,
            Phase = 1,
            SessionsPerWeek = 3,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedAt = DateTime.UtcNow
        };
        await planRepo.AddAsync(plan);
        await planRepo.SaveChangesAsync();
        
        // Add progress log
        var logRepo = new ProgressLogRepository(_fixture.DbContextFactory);
        var log = new ProgressLog
        {
            UserId = user.Id,
            LogDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Weight = 75f,
            CreatedAt = DateTime.UtcNow
        };
        await logRepo.AddAsync(log);
        await logRepo.SaveChangesAsync();

        // Act
        var fetched = await repo.GetUserWithCompleteHistoryAsync(user.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.NotNull(fetched.Person);
        Assert.NotNull(fetched.WorkoutPlans);
        Assert.NotEmpty(fetched.WorkoutPlans);
        Assert.NotNull(fetched.ProgressLogs);
        Assert.NotEmpty(fetched.ProgressLogs);
    }

    [Fact]
    public async Task GetUserByEmailAsync_ShouldReturnUserByEmail()
    {
        // Arrange
        var uniqueSuffix = Guid.NewGuid().ToString();
        var personRepo = new PersonRepository(_fixture.DbContextFactory);
        var email = $"test_{uniqueSuffix}@example.com";
        var person = new Person
        {
            FirstName = "Email",
            LastName = "Test",
            Username = $"emailuser_{uniqueSuffix}",
            Password = "pass",
            Email = email,
            Gender = Gender.Male,
            Age = 25,
            CreatedAt = DateTime.UtcNow
        };
        var addedPerson = await personRepo.AddAsync(person);
        await personRepo.SaveChangesAsync();
        
        var userRepo = new UserRepository(_fixture.DbContextFactory);
        var user = new User
        {
            PersonId = addedPerson.Id,
            Goal = Goal.Fitness,
            CreatedAt = DateTime.UtcNow
        };
        await userRepo.AddAsync(user);
        await userRepo.SaveChangesAsync();

        // Act
        var fetched = await userRepo.GetUserByEmailAsync(email);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal(addedPerson.Id, fetched.PersonId);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithInvalidEmail_ShouldReturnNull()
    {
        // Arrange
        var repo = new UserRepository(_fixture.DbContextFactory);

        // Act
        var fetched = await repo.GetUserByEmailAsync("nonexistent@example.com");

        // Assert
        Assert.Null(fetched);
    }
}
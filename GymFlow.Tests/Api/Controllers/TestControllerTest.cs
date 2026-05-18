namespace GymFlow.Tests.Api.Controllers;

public class TestControllerTest : ControllerTestFixture, IDisposable
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IExerciseRepository> _mockExerciseRepo;
    private readonly Mock<IWorkoutPlanRepository> _mockWorkoutPlanRepo;
    private readonly Mock<IPersonRepository> _mockPersonRepo;
    private readonly AppDbContext _realContext;
    private readonly TestController _controller;

    public TestControllerTest()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockExerciseRepo = new Mock<IExerciseRepository>();
        _mockWorkoutPlanRepo = new Mock<IWorkoutPlanRepository>();
        _mockPersonRepo = new Mock<IPersonRepository>();
        
        // استفاده از دیتابیس واقعی در حافظه به جای Mock
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=file:test.db?mode=memory&cache=shared")
            .Options;
        
        _realContext = new AppDbContext(options);
        _realContext.Database.OpenConnection();
        _realContext.Database.EnsureCreated();
        
        _controller = CreateController<TestController>(
            _mockUserRepo.Object,
            _mockExerciseRepo.Object,
            _mockWorkoutPlanRepo.Object,
            _realContext,
            _mockPersonRepo.Object);
    }

    #region Health

    [Fact]
    public void Health_ShouldReturnHealthyStatus()
    {
        // Act
        var result = _controller.Health();

        // Assert
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        Assert.Equal("healthy", response.Data.GetProperty("status").GetString());
        Assert.NotNull(response.Data.GetProperty("timestamp").GetString());
    }

    #endregion

    #region DatabaseStatusAsync

    [Fact]
    public async Task DatabaseStatusAsync_WhenDatabaseIsAccessible_ReturnsStatus()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.CountAsync()).ReturnsAsync(10);
        _mockExerciseRepo.Setup(r => r.CountAsync()).ReturnsAsync(30);
        _mockWorkoutPlanRepo.Setup(r => r.CountAsync()).ReturnsAsync(5);

        // Act
        var result = await _controller.DatabaseStatusAsync();

        // Assert
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        Assert.True(response.Data.GetProperty("connected").GetBoolean());
        Assert.Equal(10, response.Data.GetProperty("userCount").GetInt32());
        Assert.Equal(30, response.Data.GetProperty("exerciseCount").GetInt32());
        Assert.Equal(5, response.Data.GetProperty("workoutPlanCount").GetInt32());
    }

    [Fact]
    public async Task DatabaseStatusAsync_WhenExceptionOccurs_ReturnsError()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.CountAsync()).ThrowsAsync(new Exception("Connection failed"));

        // Act
        var result = await _controller.DatabaseStatusAsync();

        // Assert
        var errorResponse = ParseErrorResponse(result, 500);
        Assert.False(errorResponse.Success);
        Assert.Contains("Database connection failed", errorResponse.Error);
    }

    #endregion

    #region GetDemoUserAsync

    [Fact]
    public async Task GetDemoUserAsync_WhenDemoUserExists_ReturnsUser()
    {
        // Arrange
        var demoUser = new User
        {
            Id = 1,
            Goal = Goal.Fitness,
            Person = new Person
            {
                FirstName = "Demo",
                LastName = "User",
                Email = "demo@gymflow.com",
                Weight = 75f
            }
        };
        _mockUserRepo.Setup(r => r.GetUserByEmailAsync("demo@gymflow.com")).ReturnsAsync(demoUser);

        // Act
        var result = await _controller.GetDemoUserAsync();

        // Assert
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        Assert.Equal(1, response.Data.GetProperty("Id").GetInt32());
        Assert.Equal("Demo", response.Data.GetProperty("firstName").GetString());
        Assert.Equal("User", response.Data.GetProperty("lastName").GetString());
        Assert.Equal("demo@gymflow.com", response.Data.GetProperty("email").GetString());
        Assert.Equal(75f, response.Data.GetProperty("weight").GetSingle());
        Assert.Equal("Demo user available. Use this account for testing.", 
            response.Data.GetProperty("message").GetString());
    }

    [Fact]
    public async Task GetDemoUserAsync_WhenDemoUserNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetUserByEmailAsync("demo@gymflow.com")).ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetDemoUserAsync();

        // Assert
        var errorResponse = ParseErrorResponse(result, 404);
        Assert.False(errorResponse.Success);
        Assert.Equal("Demo user not found", errorResponse.Error);
    }

    #endregion

    #region GetSummaryAsync

    [Fact]
    public async Task GetSummaryAsync_ReturnsSystemSummary()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = 1, Goal = Goal.Fitness, Person = new Person { Age = 25, Weight = 75f } },
            new() { Id = 2, Goal = Goal.MuscleGain, Person = new Person { Age = 30, Weight = 80f } }
        };
        var plans = new List<WorkoutPlan>
        {
            new() { Id = 1, IsActive = true },
            new() { Id = 2, IsActive = false }
        };
        var exercises = new List<Exercise>
        {
            new() { Name = "Bench Press", PrimaryMuscleGroup = MuscleGroup.Chest },
            new() { Name = "Squat", PrimaryMuscleGroup = MuscleGroup.Legs }
        };

        _mockUserRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(users);
        _mockWorkoutPlanRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(plans);
        _mockExerciseRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(exercises);

        // Act
        var result = await _controller.GetSummaryAsync();

        // Assert
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        
        var usersSummary = response.Data.GetProperty("users");
        Assert.Equal(2, usersSummary.GetProperty("total").GetInt32());
        Assert.Equal(27.5f, usersSummary.GetProperty("averageAge").GetSingle());
        Assert.Equal(77.5f, usersSummary.GetProperty("averageWeight").GetSingle());
        
        var plansSummary = response.Data.GetProperty("workoutPlans");
        Assert.Equal(2, plansSummary.GetProperty("total").GetInt32());
        Assert.Equal(1, plansSummary.GetProperty("active").GetInt32());
        
        var exercisesSummary = response.Data.GetProperty("exercises");
        Assert.Equal(2, exercisesSummary.GetProperty("total").GetInt32());
    }

    #endregion

    #region GetAllUsersAsync

    [Fact]
    public async Task GetAllUsersAsync_ReturnsPersonsList()
    {
        // Arrange
        var persons = new List<Person>
        {
            new() { Id = 1, Username = "user1", Password = "pass1", FirstName = "John", LastName = "Doe" },
            new() { Id = 2, Username = "user2", Password = "pass2", FirstName = "Jane", LastName = "Smith" }
        };
        
        await _realContext.Persons.AddRangeAsync(persons);
        await _realContext.SaveChangesAsync();

        // Act
        var result = await _controller.GetAllUsersAsync();

        // Assert
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        
        var usersArray = response.Data.EnumerateArray().ToList();
        Assert.Equal(2, usersArray.Count);
        Assert.Contains(usersArray, u => u.GetProperty("Username").GetString() == "user1");
        Assert.Contains(usersArray, u => u.GetProperty("Username").GetString() == "user2");
    }

    #endregion

    #region GetPersonByUsernameAsync

    [Fact]
    public async Task GetPersonByUsernameAsync_WhenPersonExists_ReturnsPerson()
    {
        // Arrange
        var person = new Person
        {
            Id = 1,
            Username = "testuser",
            Password = "secret",
            FirstName = "Test",
            LastName = "User"
        };
        _mockPersonRepo.Setup(r => r.GetByUsernameAsync("testuser")).ReturnsAsync(person);

        // Act
        var result = await _controller.GetPersonByUsernameAsync("testuser");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var json = JsonSerializer.Serialize(okResult.Value);
        var response = JsonSerializer.Deserialize<JsonElement>(json);
        
        Assert.Equal(1, response.GetProperty("Id").GetInt32());
        Assert.Equal("testuser", response.GetProperty("Username").GetString());
        Assert.Equal("secret", response.GetProperty("Password").GetString());
        Assert.Equal("Test", response.GetProperty("FirstName").GetString());
        Assert.Equal("User", response.GetProperty("LastName").GetString());
    }

    [Fact]
    public async Task GetPersonByUsernameAsync_WhenPersonNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockPersonRepo.Setup(r => r.GetByUsernameAsync("notfound")).ReturnsAsync((Person?)null);

        // Act
        var result = await _controller.GetPersonByUsernameAsync("notfound");

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    #endregion

    public void Dispose()
    {
        _realContext.Database.CloseConnection();
        _realContext.Dispose();
    }
}
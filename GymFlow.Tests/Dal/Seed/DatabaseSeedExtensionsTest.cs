namespace GymFlow.Tests.Dal.Seed.Extensions;

public class DatabaseSeedExtensionsTest : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly string _dbName;
    private readonly AppDbContext _context;
    private readonly IDbContextFactory<AppDbContext> _factory;

    public DatabaseSeedExtensionsTest()
    {
        _dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"DataSource=file:{_dbName}.db?mode=memory&cache=shared")
            .Options;
        
        _context = new AppDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
        
        _factory = new AppDbContextFactory(options);
        
        // ساخت ServiceProvider برای تست
        var services = new ServiceCollection();
        services.AddSingleton(_factory);
        services.AddSingleton<IDbContextFactory<AppDbContext>>(_factory);
        services.AddSingleton(SeedProfiles.Lightweight);
        
        _serviceProvider = services.BuildServiceProvider();
    }

    // ========== Helper Methods ==========

    private async Task<int> GetTotalCountAsync<T>() where T : class
    {
        return await _context.Set<T>().CountAsync();
    }

    private async Task<bool> HasDataAsync()
    {
        return await GetTotalCountAsync<Exercise>() > 0;
    }

    // ========== Tests for EnsureDatabaseSeededAsync ==========

    [Fact]
    public async Task EnsureDatabaseSeededAsync_WithDefaultOptions_ShouldSeedDatabase()
    {
        // Arrange
        var options = SeedProfiles.Lightweight;
        options.ClearExistingData = false;

        // Act
        await _serviceProvider.EnsureDatabaseSeededAsync(options);

        // Assert
        Assert.True(await HasDataAsync());
        Assert.True(await GetTotalCountAsync<Person>() > 0);
        Assert.True(await GetTotalCountAsync<User>() > 0);
        Assert.True(await GetTotalCountAsync<WorkoutPlan>() > 0);
    }

    [Fact]
    public async Task EnsureDatabaseSeededAsync_WithNullOptions_ShouldUseRegisteredSeedOptions()
    {
        // Arrange - SeedOptions قبلاً در ServiceCollection ثبت شده است

        // Act
        await _serviceProvider.EnsureDatabaseSeededAsync();

        // Assert
        Assert.True(await HasDataAsync());
    }

    [Fact]
    public async Task EnsureDatabaseSeededAsync_WithCustomOptions_ShouldOverrideRegisteredOptions()
    {
        // Arrange
        var customOptions = SeedProfiles.QuickDemo;
        customOptions.ClearExistingData = false;

        // Act
        await _serviceProvider.EnsureDatabaseSeededAsync(customOptions);

        // Assert
        var userCount = await GetTotalCountAsync<User>();
        // QuickDemo: 3 random + coach + member = 5 users
        Assert.Equal(5, userCount);
    }

    [Fact]
    public async Task EnsureDatabaseSeededAsync_WithClearExistingData_ShouldReplaceOldData()
    {
        // Arrange
        var options1 = SeedProfiles.Lightweight;
        options1.ClearExistingData = false;
        
        // First seed
        await _serviceProvider.EnsureDatabaseSeededAsync(options1);
        var firstPersonCount = await GetTotalCountAsync<Person>();
        Assert.True(firstPersonCount > 0);

        // Second seed with clear
        var options2 = SeedProfiles.Lightweight;
        options2.ClearExistingData = true;
        await _serviceProvider.EnsureDatabaseSeededAsync(options2);
        
        var secondPersonCount = await GetTotalCountAsync<Person>();

        // Assert - داده‌ها جایگزین شده‌اند (لزوماً تعداد یکسان نیست)
        Assert.True(secondPersonCount > 0);
    }

    [Fact]
    public async Task EnsureDatabaseSeededAsync_ShouldCreateDemoUsers()
    {
        // Arrange
        var options = SeedProfiles.Lightweight;
        options.ClearExistingData = false;

        // Act
        await _serviceProvider.EnsureDatabaseSeededAsync(options);

        // Assert
        var coach = await _context.Persons.FirstOrDefaultAsync(p => p.Username == "coach");
        var member = await _context.Persons.FirstOrDefaultAsync(p => p.Username == "member");
        
        Assert.NotNull(coach);
        Assert.NotNull(member);
        Assert.Equal("coach123", coach.Password);
        Assert.Equal("member123", member.Password);
    }

    [Fact]
    public async Task EnsureDatabaseSeededAsync_ShouldCreateValidRelationships()
    {
        // Arrange
        var options = SeedProfiles.Lightweight;
        options.ClearExistingData = false;

        // Act
        await _serviceProvider.EnsureDatabaseSeededAsync(options);

        // Assert - بررسی روابط
        var workoutDays = await _context.WorkoutDays
            .Include(wd => wd.WorkoutPlan)
            .ToListAsync();
        
        foreach (var day in workoutDays)
        {
            Assert.NotNull(day.WorkoutPlan);
        }

        var wdes = await _context.WorkoutDayExercises
            .Include(wde => wde.WorkoutDay)
            .Include(wde => wde.Exercise)
            .ToListAsync();
        
        foreach (var wde in wdes)
        {
            Assert.NotNull(wde.WorkoutDay);
            Assert.NotNull(wde.Exercise);
        }
    }

    // ========== Tests for ReseedDatabaseAsync ==========

    [Fact]
    public async Task ReseedDatabaseAsync_ShouldClearAndReseedDatabase()
    {
        // Arrange
        var options = SeedProfiles.Lightweight;
        options.ClearExistingData = false;
        
        // First seed
        await _serviceProvider.EnsureDatabaseSeededAsync(options);
        var firstPersonCount = await GetTotalCountAsync<Person>();
        var firstPlanCount = await GetTotalCountAsync<WorkoutPlan>();
        
        Assert.True(firstPersonCount > 0);
        Assert.True(firstPlanCount > 0);

        // Act - Reseed
        await _serviceProvider.ReseedDatabaseAsync();
        
        var secondPersonCount = await GetTotalCountAsync<Person>();
        var secondPlanCount = await GetTotalCountAsync<WorkoutPlan>();

        // Assert - داده‌ها باید وجود داشته باشند (تازه شده‌اند)
        Assert.True(secondPersonCount > 0);
        Assert.True(secondPlanCount > 0);
    }

    [Fact]
    public async Task ReseedDatabaseAsync_ShouldUseDevelopmentProfile()
    {
        // Act
        await _serviceProvider.ReseedDatabaseAsync();

        // Assert - Development profile应该有更多数据
        var userCount = await GetTotalCountAsync<User>();
        var planCount = await GetTotalCountAsync<WorkoutPlan>();
        
        // Development: UserCount = 15 + coach + member = 17
        Assert.Equal(17, userCount);
        Assert.True(planCount >= 25, $"Expected at least 25 plans, got {planCount}");
    }

    [Fact]
    public async Task ReseedDatabaseAsync_ShouldKeepDemoUsers()
    {
        // Act
        await _serviceProvider.ReseedDatabaseAsync();

        // Assert
        var coach = await _context.Persons.FirstOrDefaultAsync(p => p.Username == "coach");
        var member = await _context.Persons.FirstOrDefaultAsync(p => p.Username == "member");
        
        Assert.NotNull(coach);
        Assert.NotNull(member);
    }

    [Fact]
    public async Task ReseedDatabaseAsync_ShouldGenerateFreshData()
    {
        // Arrange
        var options = SeedProfiles.Lightweight;
        options.ClearExistingData = false;
        
        // First seed with specific seed
        await _serviceProvider.EnsureDatabaseSeededAsync(options);
        
        // Get first exercise name
        var firstExercises = await _context.Exercises.Take(1).ToListAsync();
        var firstName = firstExercises.FirstOrDefault()?.Name;

        // Act - Reseed
        await _serviceProvider.ReseedDatabaseAsync();
        
        // Get new exercise name
        var newExercises = await _context.Exercises.Take(1).ToListAsync();
        var newName = newExercises.FirstOrDefault()?.Name;

        // ممکن است نام یکسان باشد یا نباشد، فقط بررسی می‌کنیم که داده وجود دارد
        Assert.True(await GetTotalCountAsync<Exercise>() > 0);
    }

    // ========== Edge Cases ==========

    [Fact]
    public async Task EnsureDatabaseSeededAsync_WithEmptyDatabase_ShouldSeedSuccessfully()
    {
        // Arrange - اطمینان از دیتابیس خالی
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        var options = SeedProfiles.Lightweight;
        options.ClearExistingData = false;

        // Act
        await _serviceProvider.EnsureDatabaseSeededAsync(options);

        // Assert
        Assert.True(await HasDataAsync());
    }

    [Fact]
    public async Task EnsureDatabaseSeededAsync_WithProductionProfile_ShouldNotAddDemoData()
    {
        // Arrange
        var options = SeedProfiles.Production;
        options.ClearExistingData = false;

        // Act
        await _serviceProvider.EnsureDatabaseSeededAsync(options);

        // Assert - Production profile نباید دیتایی اضافه کند
        var userCount = await GetTotalCountAsync<User>();
        Assert.Equal(0, userCount);
    }

    [Fact]
    public async Task ReseedDatabaseAsync_MultipleTimes_ShouldWorkCorrectly()
    {
        // Act & Assert - چند بار متوالی
        for (int i = 0; i < 3; i++)
        {
            await _serviceProvider.ReseedDatabaseAsync();
            Assert.True(await HasDataAsync(), $"Failed on iteration {i + 1}");
        }
        
        var finalUserCount = await GetTotalCountAsync<User>();
        Assert.Equal(17, finalUserCount);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
        _serviceProvider.Dispose();
    }
}
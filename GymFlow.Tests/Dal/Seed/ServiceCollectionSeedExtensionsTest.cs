using Xunit;
using Microsoft.Extensions.DependencyInjection;
using GymFlow.Dal.Seed.Data;
using GymFlow.Dal.Seed.Extensions;

namespace GymFlow.Tests.Dal.Seed.Extensions;

public class ServiceCollectionSeedExtensionsTest
{
    // ========== Helper Methods ==========

    private IServiceCollection CreateEmptyServiceCollection()
    {
        return new ServiceCollection();
    }

    private SeedOptions? GetRegisteredSeedOptions(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetService<SeedOptions>();
    }

    // ========== ConfigureSeedOptions Tests ==========

    [Fact]
    public void ConfigureSeedOptions_ShouldRegisterSeedOptionsAsSingleton()
    {
        // Arrange
        var services = CreateEmptyServiceCollection();
        var options = new SeedOptions { UserCount = 99 };

        // Act
        services.ConfigureSeedOptions(options);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var registeredOptions = GetRegisteredSeedOptions(serviceProvider);
        Assert.NotNull(registeredOptions);
        Assert.Same(options, registeredOptions);
        Assert.Equal(99, registeredOptions.UserCount);
    }

    [Fact]
    public void ConfigureSeedOptions_ShouldAllowMultipleCalls_LastOneWins()
    {
        // Arrange
        var services = CreateEmptyServiceCollection();
        var options1 = new SeedOptions { UserCount = 10 };
        var options2 = new SeedOptions { UserCount = 20 };

        // Act
        services.ConfigureSeedOptions(options1);
        services.ConfigureSeedOptions(options2);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var registeredOptions = GetRegisteredSeedOptions(serviceProvider);
        Assert.NotNull(registeredOptions);
        Assert.Same(options2, registeredOptions);
        Assert.Equal(20, registeredOptions.UserCount);
    }

    // ========== UseDevelopmentSeed Tests ==========

    [Fact]
    public void UseDevelopmentSeed_ShouldRegisterDevelopmentProfile()
    {
        // Arrange
        var services = CreateEmptyServiceCollection();

        // Act
        services.UseDevelopmentSeed();
        var serviceProvider = services.BuildServiceProvider();
        var registeredOptions = GetRegisteredSeedOptions(serviceProvider);

        // Assert
        Assert.NotNull(registeredOptions);
        Assert.Equal(15, registeredOptions.UserCount);
        Assert.True(registeredOptions.IncludeDemoUser);
        Assert.Equal(1337, registeredOptions.RandomSeed);
    }

    // ========== UseQuickDemoSeed Tests ==========

    [Fact]
    public void UseQuickDemoSeed_ShouldRegisterQuickDemoProfile()
    {
        // Arrange
        var services = CreateEmptyServiceCollection();

        // Act
        services.UseQuickDemoSeed();
        var serviceProvider = services.BuildServiceProvider();
        var registeredOptions = GetRegisteredSeedOptions(serviceProvider);

        // Assert
        Assert.NotNull(registeredOptions);
        Assert.Equal(3, registeredOptions.UserCount);
        Assert.True(registeredOptions.IncludeDemoUser);
        Assert.Equal(42, registeredOptions.RandomSeed);
    }

    // ========== UseLightweightSeed Tests ==========

    [Fact]
    public void UseLightweightSeed_ShouldRegisterLightweightProfile()
    {
        // Arrange
        var services = CreateEmptyServiceCollection();

        // Act
        services.UseLightweightSeed();
        var serviceProvider = services.BuildServiceProvider();
        var registeredOptions = GetRegisteredSeedOptions(serviceProvider);

        // Assert
        Assert.NotNull(registeredOptions);
        Assert.Equal(5, registeredOptions.UserCount);
        Assert.True(registeredOptions.IncludeDemoUser);
        Assert.Equal(123, registeredOptions.RandomSeed);
    }

    // ========== UseStressTestSeed Tests ==========

    [Fact]
    public void UseStressTestSeed_ShouldRegisterStressTestProfile()
    {
        // Arrange
        var services = CreateEmptyServiceCollection();

        // Act
        services.UseStressTestSeed();
        var serviceProvider = services.BuildServiceProvider();
        var registeredOptions = GetRegisteredSeedOptions(serviceProvider);

        // Assert
        Assert.NotNull(registeredOptions);
        Assert.Equal(50, registeredOptions.UserCount);
        Assert.True(registeredOptions.IncludeDemoUser);
        Assert.Null(registeredOptions.RandomSeed);
    }

    // ========== UseProductionSeed Tests ==========

    [Fact]
    public void UseProductionSeed_ShouldRegisterProductionProfile()
    {
        // Arrange
        var services = CreateEmptyServiceCollection();

        // Act
        services.UseProductionSeed();
        var serviceProvider = services.BuildServiceProvider();
        var registeredOptions = GetRegisteredSeedOptions(serviceProvider);

        // Assert
        Assert.NotNull(registeredOptions);
        Assert.Equal(0, registeredOptions.UserCount);
        Assert.False(registeredOptions.IncludeDemoUser);
        Assert.False(registeredOptions.RefreshOnStartup);
        Assert.False(registeredOptions.ClearExistingData);
        Assert.True(registeredOptions.SeedOnlyIfEmpty);
    }

    // ========== UseEmptySeed Tests ==========

    [Fact]
    public void UseEmptySeed_ShouldRegisterEmptyProfile()
    {
        // Arrange
        var services = CreateEmptyServiceCollection();

        // Act
        services.UseEmptySeed();
        var serviceProvider = services.BuildServiceProvider();
        var registeredOptions = GetRegisteredSeedOptions(serviceProvider);

        // Assert
        Assert.NotNull(registeredOptions);
        Assert.Equal(0, registeredOptions.UserCount);
        Assert.False(registeredOptions.IncludeDemoUser);
        Assert.False(registeredOptions.RefreshOnStartup);
        Assert.True(registeredOptions.ClearExistingData);
        Assert.False(registeredOptions.SeedOnlyIfEmpty);
    }

    // ========== UseSeedProfile Tests ==========

    [Theory]
    [InlineData("development", 15)]
    [InlineData("dev", 15)]
    [InlineData("quickdemo", 3)]
    [InlineData("demo", 3)]
    [InlineData("lightweight", 5)]
    [InlineData("light", 5)]
    [InlineData("stresstest", 50)]
    [InlineData("stress", 50)]
    [InlineData("performance", 50)]
    [InlineData("production", 0)]
    [InlineData("prod", 0)]
    [InlineData("empty", 0)]
    [InlineData("clean", 0)]
    [InlineData("none", 0)]
    public void UseSeedProfile_WithValidName_ShouldRegisterCorrectProfile(string profileName, int expectedUserCount)
    {
        // Arrange
        var services = CreateEmptyServiceCollection();

        // Act
        services.UseSeedProfile(profileName);
        var serviceProvider = services.BuildServiceProvider();
        var registeredOptions = GetRegisteredSeedOptions(serviceProvider);

        // Assert
        Assert.NotNull(registeredOptions);
        Assert.Equal(expectedUserCount, registeredOptions.UserCount);
    }

    [Fact]
    public void UseSeedProfile_WithInvalidName_ShouldRegisterDevelopmentProfile()
    {
        // Arrange
        var services = CreateEmptyServiceCollection();

        // Act
        services.UseSeedProfile("invalidname");
        var serviceProvider = services.BuildServiceProvider();
        var registeredOptions = GetRegisteredSeedOptions(serviceProvider);

        // Assert
        Assert.NotNull(registeredOptions);
        Assert.Equal(15, registeredOptions.UserCount);
        Assert.True(registeredOptions.IncludeDemoUser);
    }

    [Fact]
    public void UseSeedProfile_WithEmptyString_ShouldRegisterDevelopmentProfile()
    {
        // Arrange
        var services = CreateEmptyServiceCollection();

        // Act
        services.UseSeedProfile("");
        var serviceProvider = services.BuildServiceProvider();
        var registeredOptions = GetRegisteredSeedOptions(serviceProvider);

        // Assert
        Assert.NotNull(registeredOptions);
        Assert.Equal(15, registeredOptions.UserCount);
    }

    [Fact]
    public void UseSeedProfile_WithNull_ShouldRegisterDevelopmentProfile()
    {
        // Arrange
        var services = CreateEmptyServiceCollection();

        // Act
        services.UseSeedProfile(null!);
        var serviceProvider = services.BuildServiceProvider();
        var registeredOptions = GetRegisteredSeedOptions(serviceProvider);

        // Assert
        Assert.NotNull(registeredOptions);
        Assert.Equal(15, registeredOptions.UserCount);
    }

    // ========== Chaining Tests ==========

    [Fact]
    public void MultipleExtensions_ShouldBeChainable()
    {
        // Arrange
        var services = CreateEmptyServiceCollection();

        // Act
        services.UseDevelopmentSeed()
                .UseQuickDemoSeed()
                .UseLightweightSeed();

        var serviceProvider = services.BuildServiceProvider();
        var registeredOptions = GetRegisteredSeedOptions(serviceProvider);

        // Assert - آخرین فراخوانی (Lightweight) باید ثبت شود
        Assert.NotNull(registeredOptions);
        Assert.Equal(5, registeredOptions.UserCount);
    }

    [Fact]
    public void ConfigureSeedOptions_AfterUseProfile_ShouldOverride()
    {
        // Arrange
        var services = CreateEmptyServiceCollection();
        var customOptions = new SeedOptions { UserCount = 100 };

        // Act
        services.UseDevelopmentSeed();
        services.ConfigureSeedOptions(customOptions);
        var serviceProvider = services.BuildServiceProvider();
        var registeredOptions = GetRegisteredSeedOptions(serviceProvider);

        // Assert
        Assert.NotNull(registeredOptions);
        Assert.Same(customOptions, registeredOptions);
        Assert.Equal(100, registeredOptions.UserCount);
    }

    // ========== Service Registration Tests ==========

    [Fact]
    public void ConfigureSeedOptions_ShouldRegisterAsSingleton_NotTransientOrScoped()
    {
        // Arrange
        var services = CreateEmptyServiceCollection();
        var options = new SeedOptions();

        // Act
        services.ConfigureSeedOptions(options);
        
        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(SeedOptions));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void UseDevelopmentSeed_ShouldNotThrowException()
    {
        // Arrange
        var services = CreateEmptyServiceCollection();

        // Act & Assert
        var exception = Record.Exception(() => services.UseDevelopmentSeed());
        Assert.Null(exception);
    }

    [Fact]
    public void UseSeedProfile_WithAllProfileNames_ShouldNotThrowException()
    {
        // Arrange
        var services = CreateEmptyServiceCollection();
        var profileNames = new[] { "development", "quickdemo", "lightweight", "stresstest", "production", "empty" };

        // Act & Assert
        foreach (var name in profileNames)
        {
            var exception = Record.Exception(() => services.UseSeedProfile(name));
            Assert.Null(exception);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using GymFlow.Dal.Context;

namespace GymFlow.Dal.Seed.Data;

public class DatabaseSeeder
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly SeedOptions _options;

    public DatabaseSeeder(IDbContextFactory<AppDbContext> dbContextFactory, SeedOptions? options = null)
    {
        _dbContextFactory = dbContextFactory;
        _options = options ?? SeedProfiles.Development;
    }

    public async Task SeedAsync()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        if (_options.ClearExistingData && await context.Users.AnyAsync())
        {
            await ClearDatabaseAsync(context);
        }

        Console.WriteLine("🌱 Generating and seeding rich test data...");
        Console.WriteLine("===============================================");

        // 1. Exercises - فقط اگر قرار است دیتایی داشته باشیم
        var hasAnyData = _options.UserCount > 0 || _options.IncludeDemoUser;
        
        if (hasAnyData)
        {
            var exercises = DataGenerator.GenerateExercises(30);
            await context.Exercises.AddRangeAsync(exercises);
            await context.SaveChangesAsync();
            Console.WriteLine($"  ✅ Added {exercises.Count} exercises");
        }
        else
        {
            Console.WriteLine("  ⚠️ No exercises generated (no data requested)");
        }

        // 2. Persons - فقط اگر IncludeDemoUser فعال باشد یا UserCount > 0
        var personCount = _options.IncludeDemoUser ? _options.UserCount + 2 : _options.UserCount;
        if (personCount > 0)
        {
            var persons = DataGenerator.GeneratePersons(personCount);
            await context.Persons.AddRangeAsync(persons);
            await context.SaveChangesAsync();
            Console.WriteLine($"  ✅ Added {persons.Count} persons");

            // 3. Users
            var users = DataGenerator.GenerateUsers(persons);
            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
            Console.WriteLine($"  ✅ Added {users.Count} users");

            // 4. Coaches - فقط اگر IncludeDemoUser فعال باشد
            if (_options.IncludeDemoUser)
            {
                var coaches = DataGenerator.GenerateCoaches(persons);
                await context.Coaches.AddRangeAsync(coaches);
                await context.SaveChangesAsync();
                Console.WriteLine($"  ✅ Added {coaches.Count} coaches");
            }

            // 5. WorkoutPlans - فقط اگر کاربر وجود داشته باشد
            if (users.Any())
            {
                var plans = DataGenerator.GenerateWorkoutPlans(users);
                await context.WorkoutPlans.AddRangeAsync(plans);
                await context.SaveChangesAsync();
                Console.WriteLine($"  ✅ Added {plans.Count} workout plans");

                // 6. WorkoutDays
                var days = DataGenerator.GenerateWorkoutDays(plans);
                await context.WorkoutDays.AddRangeAsync(days);
                await context.SaveChangesAsync();
                Console.WriteLine($"  ✅ Added {days.Count} workout days");

                // 7. WorkoutDayExercises
                var exercises = await context.Exercises.ToListAsync();
                if (exercises.Any())
                {
                    var wdes = DataGenerator.GenerateWorkoutDayExercises(days, exercises);
                    await context.WorkoutDayExercises.AddRangeAsync(wdes);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"  ✅ Added {wdes.Count} workout day exercises");
                }

                // 8. ProgressLogs
                var logs = DataGenerator.GenerateProgressLogs(users, plans);
                await context.ProgressLogs.AddRangeAsync(logs);
                await context.SaveChangesAsync();
                Console.WriteLine($"  ✅ Added {logs.Count} progress logs");

                // 9. WorkoutSessions
                var sessions = DataGenerator.GenerateWorkoutSessions(days);
                await context.WorkoutSessions.AddRangeAsync(sessions);
                await context.SaveChangesAsync();
                Console.WriteLine($"  ✅ Added {sessions.Count} workout sessions");
            }
        }
        else
        {
            Console.WriteLine("  ⚠️ No persons generated (UserCount = 0 and IncludeDemoUser = false)");
        }

        if (_options.IncludeDemoUser)
        {
            Console.WriteLine("===============================================");
            Console.WriteLine("✅ DATABASE SEEDING COMPLETED SUCCESSFULLY!");
            Console.WriteLine($"   Coach: coach / coach123");
            Console.WriteLine($"   Member: member / member123");
        }
        else
        {
            Console.WriteLine("===============================================");
            Console.WriteLine("✅ DATABASE SEEDING COMPLETED SUCCESSFULLY!");
        }
    }

    private async Task ClearDatabaseAsync(AppDbContext context)
    {
        Console.WriteLine("🗑️ Clearing existing database data...");

        await context.WorkoutSessions.ExecuteDeleteAsync();
        await context.WorkoutDayExercises.ExecuteDeleteAsync();
        await context.ProgressLogs.ExecuteDeleteAsync();
        await context.WorkoutDays.ExecuteDeleteAsync();
        await context.WorkoutPlans.ExecuteDeleteAsync();
        await context.Coaches.ExecuteDeleteAsync();
        await context.Users.ExecuteDeleteAsync();
        await context.Persons.ExecuteDeleteAsync();
        await context.Exercises.ExecuteDeleteAsync();

        await context.Database.ExecuteSqlRawAsync("DELETE FROM sqlite_sequence;");
        Console.WriteLine("  ✅ Database cleared");
    }
}
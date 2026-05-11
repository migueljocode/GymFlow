using Person = GymFlow.Models.Entities.Person;

namespace GymFlow.Dal.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSet‌ها
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Coach> Coaches => Set<Coach>();
    public DbSet<WorkoutPlan> WorkoutPlans => Set<WorkoutPlan>();
    public DbSet<WorkoutDay> WorkoutDays => Set<WorkoutDay>();
    public DbSet<WorkoutSession> WorkoutSessions => Set<WorkoutSession>();
    public DbSet<ProgressLog> ProgressLogs => Set<ProgressLog>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<WorkoutDayExercise> WorkoutDayExercises => Set<WorkoutDayExercise>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // اعمال تمام کانفیگ‌ها
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
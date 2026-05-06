namespace GymFlow.Dal.Configurations;

public class ProgressLogConfiguration : BaseConfiguration<ProgressLog>
{
    public override void Configure(EntityTypeBuilder<ProgressLog> builder)
    {
        base.Configure(builder);
        
        builder.ToTable("ProgressLogs");

        builder.Property(pl => pl.LogDate)
            .IsRequired();

        builder.Property(pl => pl.Weight)
            .IsRequired()
            .HasPrecision(5, 2);

        builder.Property(pl => pl.BodyFatPercentage)
            .HasPrecision(5, 2);

        builder.Property(pl => pl.Notes)
            .HasMaxLength(500);

        builder.HasOne(pl => pl.User)
            .WithMany(u => u.ProgressLogs)
            .HasForeignKey(pl => pl.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pl => pl.WorkoutPlan)
            .WithMany(wp => wp.ProgressLogs)
            .HasForeignKey(pl => pl.WorkoutPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pl => pl.UserId)
            .HasDatabaseName("IX_ProgressLogs_UserId");

        builder.HasIndex(pl => pl.LogDate)
            .HasDatabaseName("IX_ProgressLogs_LogDate");

        builder.HasIndex(pl => new { pl.UserId, pl.LogDate })
            .IsUnique()
            .HasDatabaseName("IX_ProgressLogs_User_Date");
    }
}
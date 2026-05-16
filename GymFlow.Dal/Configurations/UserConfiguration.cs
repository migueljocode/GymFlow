using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GymFlow.Models.Entities;

namespace GymFlow.Dal.Configurations;

public class UserConfiguration : BaseConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);
        
        builder.ToTable("Users");
        
        builder.Property(u => u.Goal)
            .HasConversion<string>()
            .HasMaxLength(20);
        
        builder.Property(u => u.EstimatedCaloriesIntake)
            .IsRequired(false);
        
        // این خط مهم است - مطمئن شو HasDefaultValue دارد
        builder.Property(u => u.IsCompetitive)
            .IsRequired()
            .HasDefaultValueSql("0");
        
        builder.HasOne(u => u.Person)
            .WithOne(p => p.User)
            .HasForeignKey<User>(u => u.PersonId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(u => u.Coach)
            .WithMany(c => c.Clients)
            .HasForeignKey(u => u.CoachId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasMany(u => u.WorkoutPlans)
            .WithOne(w => w.User)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(u => u.ProgressLogs)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(u => u.PersonId)
            .IsUnique()
            .HasDatabaseName("IX_Users_PersonId");
        
        builder.HasIndex(u => u.CoachId)
            .HasDatabaseName("IX_Users_CoachId");
    }
}
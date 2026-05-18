namespace GymFlow.Dal.Configurations;

public class CoachConfiguration : BaseConfiguration<Coach>
{
    public override void Configure(EntityTypeBuilder<Coach> builder)
    {
        base.Configure(builder);
        
        builder.ToTable("Coaches");
        
        builder.Property(c => c.Specialization)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(c => c.YearsOfExperience)
            .IsRequired();
        
        builder.Property(c => c.CertificateUrl)
            .HasMaxLength(500);
        
        builder.HasOne(c => c.Person)
            .WithOne(p => p.Coach)
            .HasForeignKey<Coach>(c => c.PersonId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // رابطه با Users (یک به چند)
        builder.HasMany(c => c.Clients)
            .WithOne(u => u.Coach)
            .HasForeignKey(u => u.CoachId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasIndex(c => c.PersonId)
            .IsUnique()
            .HasDatabaseName("IX_Coaches_PersonId");
    }
}
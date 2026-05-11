using Person = GymFlow.Models.Entities.Person;

namespace GymFlow.Dal.Configurations;

public class PersonConfiguration : BaseConfiguration<Person>
{
    public override void Configure(EntityTypeBuilder<Person> builder)
    {
        base.Configure(builder);
        
        builder.ToTable("Persons");
        
        builder.Property(p => p.FirstName)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(p => p.LastName)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(p => p.Username)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(p => p.Password)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(p => p.Email)
            .HasMaxLength(100);
        
        builder.Property(p => p.Phone)
            .HasMaxLength(15);
        
        builder.Property(p => p.Gender)
            .HasConversion<string>()
            .HasMaxLength(10);
        
        builder.Property(p => p.BodyType)
            .HasConversion<string>()
            .HasMaxLength(20);
        
        builder.Property(p => p.Weight)
            .HasPrecision(5, 2);
        
        builder.Property(p => p.Height)
            .HasPrecision(5, 2);
        
        // ایندکس یکتا برای Username
        builder.HasIndex(p => p.Username)
            .IsUnique()
            .HasDatabaseName("IX_Persons_Username");
        
        // رابطه یک به یک با User
        builder.HasOne(p => p.User)
            .WithOne(u => u.Person)
            .HasForeignKey<User>(u => u.PersonId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // رابطه یک به یک با Coach
        builder.HasOne(p => p.Coach)
            .WithOne(c => c.Person)
            .HasForeignKey<Coach>(c => c.PersonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
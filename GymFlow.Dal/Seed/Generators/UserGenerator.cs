using Person = GymFlow.Models.Entities.Person;

namespace GymFlow.Dal.Seed.Generators;

/// <summary>
/// Generator for creating User entities linked to Persons
/// </summary>
public class UserGenerator
{
    private readonly Faker _faker;
    private readonly SeedOptions _options;
    private int _nextId;
    
    public UserGenerator(SeedOptions options, int startId = 1)
    {
        _options = options;
        _nextId = startId;
        _faker = new Faker("en");
        Randomizer.Seed = new Random(_options.RandomSeed ?? 42);
    }
    
    public User CreateFromPerson(Person person, bool isDemo = false)
    {
        if (isDemo)
        {
            return new User
            {
                Id = _nextId++,
                PersonId = person.Id,
                Goal = Goal.MuscleGain,
                EstimatedCaloriesIntake = 2500,
                // IsCompetitive = false,
                CreatedAt = person.CreatedAt
            };
        }
        
        return new User
        {
            Id = _nextId++,
            PersonId = person.Id,
            Goal = _faker.PickRandom<Goal>(),
            EstimatedCaloriesIntake = _faker.Random.Bool(0.7f) ? _faker.Random.Int(1800, 3200) : null,
            // IsCompetitive = _faker.Random.Bool(0.15f),
            CreatedAt = person.CreatedAt
        };
    }
    
    public int CurrentId => _nextId;
}
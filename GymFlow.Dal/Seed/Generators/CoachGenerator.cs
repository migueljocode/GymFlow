using Person = GymFlow.Models.Entities.Person;

namespace GymFlow.Dal.Seed.Generators;

/// <summary>
/// Generator for creating Coach entities linked to Persons
/// </summary>
public class CoachGenerator
{
    private readonly Faker _faker;
    private readonly SeedOptions _options;
    private int _nextId;
    
    public CoachGenerator(SeedOptions options, int startId = 1)
    {
        _options = options;
        _nextId = startId;
        _faker = new Faker("en");
        Randomizer.Seed = new Random(_options.RandomSeed ?? 42);
    }
    
    public Coach CreateDemoFromPerson(Person person)
    {
        return new Coach
        {
            Id = _nextId++,
            PersonId = person.Id,
            Specialization = "Strength & Conditioning",
            YearsOfExperience = 10,
            CertificateUrl = "https://example.com/certificate.pdf",
            CreatedAt = person.CreatedAt
        };
    }
    
    public Coach CreateRandomFromPerson(Person person)
    {
        var specializations = new[] { "Bodybuilding", "Powerlifting", "CrossFit", "Yoga", "Nutrition", "Weight Loss" };
        
        return new Coach
        {
            Id = _nextId++,
            PersonId = person.Id,
            Specialization = _faker.PickRandom(specializations),
            YearsOfExperience = _faker.Random.Int(2, 15),
            CertificateUrl = _faker.Internet.Url(),
            CreatedAt = person.CreatedAt
        };
    }
    
    public int CurrentId => _nextId;
}
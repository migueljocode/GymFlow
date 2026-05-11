using Person = GymFlow.Models.Entities.Person;

namespace GymFlow.Dal.Seed.Generators;

/// <summary>
/// Generator for creating Person entities
/// </summary>
public class PersonGenerator
{
    private readonly Faker _faker;
    private readonly SeedOptions _options;
    private int _nextId;
    
    public PersonGenerator(SeedOptions options, int startId = 1)
    {
        _options = options;
        _nextId = startId;
        _faker = new Faker("en");
        Randomizer.Seed = new Random(_options.RandomSeed ?? 42);
    }
    
    public Person CreateDemoCoach()
    {
        return new Person
        {
            Id = _nextId++,
            FirstName = "Master",
            LastName = "Coach",
            Username = "coach",
            Password = "coach123",
            Email = "coach@gymflow.com",
            Phone = "+1 (555) 000-0001",
            Gender = Gender.Male,
            Age = 35,
            Weight = 85.5f,
            Height = 182f,
            BodyType = BodyType.Fit,
            CreatedAt = DateTime.UtcNow.AddMonths(-6)
        };
    }
    
    public Person CreateDemoMember()
    {
        return new Person
        {
            Id = _nextId++,
            FirstName = "John",
            LastName = "Doe",
            Username = "member",
            Password = "member123",
            Email = "member@gymflow.com",
            Phone = "+1 (555) 000-0002",
            Gender = Gender.Male,
            Age = 25,
            Weight = 75.0f,
            Height = 175f,
            BodyType = BodyType.Fit,
            CreatedAt = DateTime.UtcNow.AddMonths(-3)
        };
    }
    
    public Person CreateRandom()
    {
        var gender = _faker.PickRandom<Gender>();
        var weight = gender == Gender.Female 
            ? _faker.Random.Float(50f, 85f) 
            : _faker.Random.Float(65f, 110f);
        var height = gender == Gender.Female 
            ? _faker.Random.Float(155f, 175f) 
            : _faker.Random.Float(170f, 190f);
        
        var firstName = _faker.Name.FirstName(gender == Gender.Male ? Bogus.DataSets.Name.Gender.Male : Bogus.DataSets.Name.Gender.Female);
        var lastName = _faker.Name.LastName();
        var username = $"{firstName.ToLower()}.{lastName.ToLower()}{_faker.Random.Int(1, 99)}";
        
        return new Person
        {
            Id = _nextId++,
            FirstName = firstName,
            LastName = lastName,
            Username = username,
            Password = "password123",
            Email = _faker.Internet.Email(firstName, lastName),
            Phone = _faker.Phone.PhoneNumber(),
            Gender = gender,
            Age = _faker.Random.Int(18, 55),
            Weight = weight,
            Height = height,
            BodyType = DetermineBodyType(weight, height),
            CreatedAt = _faker.Date.Past(1),
            UpdatedAt = _faker.Random.Bool(0.3f) ? _faker.Date.Recent(30) : null
        };
    }
    
    private BodyType DetermineBodyType(float weight, float height)
    {
        var bmi = weight / ((height / 100) * (height / 100));
        return bmi switch
        {
            < 18.5f => BodyType.LeanMuscular,
            < 25 => BodyType.Fit,
            < 30 => BodyType.Overweight,
            _ => BodyType.Obese
        };
    }
    
    public int CurrentId => _nextId;
}
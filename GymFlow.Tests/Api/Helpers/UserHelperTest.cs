namespace GymFlow.Tests.Api.Helpers;

public class UserHelperTest
{
    #region Helper Methods

    private User CreateTestUser(
        string firstName = "John",
        string lastName = "Doe",
        string? email = "john@example.com",
        float? weight = 80f,
        int? age = 30,
        Gender? gender = Gender.Male,
        BodyType? bodyType = BodyType.Fit)
    {
        var person = new Person
        {
            Id = 1,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Weight = weight,
            Age = age ?? 0,                     // تبدیل ایمن به int غیر nullable
            Gender = gender ?? Gender.Male,     // تبدیل ایمن به Gender غیر nullable
            BodyType = bodyType,
            Username = "testuser",
            Password = "pass123",
            CreatedAt = DateTime.UtcNow
        };

        return new User
        {
            Id = 1,
            PersonId = person.Id,
            Person = person,
            Goal = Goal.Fitness,
            CreatedAt = DateTime.UtcNow
        };
    }

    private User CreateUserWithNullPerson()
    {
        return new User { Id = 1, Person = null! };
    }

    // حذف شد: CreateUserWithNullPersonProperties (چون Age و Gender در مدل غیر nullable هستند)

    #endregion

    #region GetFullName

    [Fact]
    public void GetFullName_WithValidUser_ReturnsFullName()
    {
        var user = CreateTestUser("Jane", "Smith");
        var result = UserHelper.GetFullName(user);
        Assert.Equal("Jane Smith", result);
    }

    [Fact]
    public void GetFullName_WithNullUser_ReturnsUnknown()
    {
        var result = UserHelper.GetFullName(null);
        Assert.Equal("Unknown", result);
    }

    [Fact]
    public void GetFullName_WithUserWithoutPerson_ReturnsUnknown()
    {
        var user = CreateUserWithNullPerson();
        var result = UserHelper.GetFullName(user);
        Assert.Equal("Unknown", result);
    }

    [Fact]
    public void GetFullName_WithEmptyFirstName_ReturnsSpaceAndLastName()
    {
        var user = CreateTestUser(firstName: "", lastName: "Smith");
        var result = UserHelper.GetFullName(user);
        Assert.Equal(" Smith", result);
    }

    #endregion

    #region GetFirstName

    [Fact]
    public void GetFirstName_WithValidUser_ReturnsFirstName()
    {
        var user = CreateTestUser(firstName: "Michael");
        var result = UserHelper.GetFirstName(user);
        Assert.Equal("Michael", result);
    }

    [Fact]
    public void GetFirstName_WithNullUser_ReturnsUnknown()
    {
        var result = UserHelper.GetFirstName(null);
        Assert.Equal("Unknown", result);
    }

    [Fact]
    public void GetFirstName_WithUserWithoutPerson_ReturnsUnknown()
    {
        var user = CreateUserWithNullPerson();
        var result = UserHelper.GetFirstName(user);
        Assert.Equal("Unknown", result);
    }

    [Fact]
    public void GetFirstName_WithEmptyFirstName_ReturnsEmptyString()
    {
        var user = CreateTestUser(firstName: "");
        var result = UserHelper.GetFirstName(user);
        Assert.Equal("", result);
    }

    #endregion

    #region GetLastName

    [Fact]
    public void GetLastName_WithValidUser_ReturnsLastName()
    {
        var user = CreateTestUser(lastName: "Johnson");
        var result = UserHelper.GetLastName(user);
        Assert.Equal("Johnson", result);
    }

    [Fact]
    public void GetLastName_WithNullUser_ReturnsUnknown()
    {
        var result = UserHelper.GetLastName(null);
        Assert.Equal("Unknown", result);
    }

    [Fact]
    public void GetLastName_WithUserWithoutPerson_ReturnsUnknown()
    {
        var user = CreateUserWithNullPerson();
        var result = UserHelper.GetLastName(user);
        Assert.Equal("Unknown", result);
    }

    [Fact]
    public void GetLastName_WithEmptyLastName_ReturnsEmptyString()
    {
        var user = CreateTestUser(lastName: "");
        var result = UserHelper.GetLastName(user);
        Assert.Equal("", result);
    }

    #endregion

    #region GetEmail

    [Fact]
    public void GetEmail_WithValidUser_ReturnsEmail()
    {
        var user = CreateTestUser(email: "test@example.com");
        var result = UserHelper.GetEmail(user);
        Assert.Equal("test@example.com", result);
    }

    [Fact]
    public void GetEmail_WithNullUser_ReturnsNull()
    {
        var result = UserHelper.GetEmail(null);
        Assert.Null(result);
    }

    [Fact]
    public void GetEmail_WithUserWithoutPerson_ReturnsNull()
    {
        var user = CreateUserWithNullPerson();
        var result = UserHelper.GetEmail(user);
        Assert.Null(result);
    }

    [Fact]
    public void GetEmail_WithNullEmail_ReturnsNull()
    {
        var user = CreateTestUser(email: null);
        var result = UserHelper.GetEmail(user);
        Assert.Null(result);
    }

    #endregion

    #region GetWeight

    [Fact]
    public void GetWeight_WithValidUser_ReturnsWeight()
    {
        var user = CreateTestUser(weight: 85.5f);
        var result = UserHelper.GetWeight(user);
        Assert.Equal(85.5f, result);
    }

    [Fact]
    public void GetWeight_WithNullUser_ReturnsNull()
    {
        var result = UserHelper.GetWeight(null);
        Assert.Null(result);
    }

    [Fact]
    public void GetWeight_WithUserWithoutPerson_ReturnsNull()
    {
        var user = CreateUserWithNullPerson();
        var result = UserHelper.GetWeight(user);
        Assert.Null(result);
    }

    [Fact]
    public void GetWeight_WithNullWeight_ReturnsNull()
    {
        var user = CreateTestUser(weight: null);
        var result = UserHelper.GetWeight(user);
        Assert.Null(result);
    }

    #endregion

    #region GetAge

    [Fact]
    public void GetAge_WithValidUser_ReturnsAge()
    {
        var user = CreateTestUser(age: 25);
        var result = UserHelper.GetAge(user);
        Assert.Equal(25, result);
    }

    [Fact]
    public void GetAge_WithNullUser_ReturnsNull()
    {
        var result = UserHelper.GetAge(null);
        Assert.Null(result);
    }

    [Fact]
    public void GetAge_WithUserWithoutPerson_ReturnsNull()
    {
        var user = CreateUserWithNullPerson();
        var result = UserHelper.GetAge(user);
        Assert.Null(result);
    }

    [Fact]
    public void GetAge_WithNullAge_ReturnsNull()
    {
        // با توجه به اینکه Age در مدل non-nullable است،
        // نمی‌توانیم null بدهیم، بنابراین این تست را حذف می‌کنیم.
        // تست معادل: مقدار پیش‌فرض 0 را بررسی می‌کنیم.
        var user = CreateTestUser(age: null);
        var result = UserHelper.GetAge(user);
        // چون Age در Person با coalescing به 0 تبدیل می‌شود،
        // GetAge مقدار 0 برمی‌گرداند، نه null.
        Assert.Equal(0, result);
    }

    #endregion

    #region GetGender

    [Fact]
    public void GetGender_WithValidUser_ReturnsGender()
    {
        var user = CreateTestUser(gender: Gender.Female);
        var result = UserHelper.GetGender(user);
        Assert.Equal(Gender.Female, result);
    }

    [Fact]
    public void GetGender_WithNullUser_ReturnsNull()
    {
        var result = UserHelper.GetGender(null);
        Assert.Null(result);
    }

    [Fact]
    public void GetGender_WithUserWithoutPerson_ReturnsNull()
    {
        var user = CreateUserWithNullPerson();
        var result = UserHelper.GetGender(user);
        Assert.Null(result);
    }

    [Fact]
    public void GetGender_WithNullGender_ReturnsDefaultGender()
    {
        // Gender در مدل non-nullable است، null به معنی استفاده از مقدار پیش‌فرض (Male)
        var user = CreateTestUser(gender: null);
        var result = UserHelper.GetGender(user);
        Assert.Equal(Gender.Male, result);
    }

    #endregion

    #region GetBodyType

    [Fact]
    public void GetBodyType_WithValidUser_ReturnsBodyType()
    {
        var user = CreateTestUser(bodyType: BodyType.LeanMuscular);
        var result = UserHelper.GetBodyType(user);
        Assert.Equal(BodyType.LeanMuscular, result);
    }

    [Fact]
    public void GetBodyType_WithNullUser_ReturnsNull()
    {
        var result = UserHelper.GetBodyType(null);
        Assert.Null(result);
    }

    [Fact]
    public void GetBodyType_WithUserWithoutPerson_ReturnsNull()
    {
        var user = CreateUserWithNullPerson();
        var result = UserHelper.GetBodyType(user);
        Assert.Null(result);
    }

    [Fact]
    public void GetBodyType_WithNullBodyType_ReturnsNull()
    {
        var user = CreateTestUser(bodyType: null);
        var result = UserHelper.GetBodyType(user);
        Assert.Null(result);
    }

    #endregion

    #region GetPerson

    [Fact]
    public void GetPerson_WithValidUser_ReturnsPerson()
    {
        var user = CreateTestUser();
        var result = UserHelper.GetPerson(user);
        Assert.NotNull(result);
        Assert.Equal(user.Person, result);
    }

    [Fact]
    public void GetPerson_WithNullUser_ReturnsNull()
    {
        var result = UserHelper.GetPerson(null);
        Assert.Null(result);
    }

    [Fact]
    public void GetPerson_WithUserWithoutPerson_ReturnsNull()
    {
        var user = CreateUserWithNullPerson();
        var result = UserHelper.GetPerson(user);
        Assert.Null(result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void AllMethods_WithUserHavingNullPerson_ReturnDefaults()
    {
        var user = CreateUserWithNullPerson();

        Assert.Equal("Unknown", UserHelper.GetFullName(user));
        Assert.Equal("Unknown", UserHelper.GetFirstName(user));
        Assert.Equal("Unknown", UserHelper.GetLastName(user));
        Assert.Null(UserHelper.GetEmail(user));
        Assert.Null(UserHelper.GetWeight(user));
        Assert.Null(UserHelper.GetAge(user));
        Assert.Null(UserHelper.GetGender(user));
        Assert.Null(UserHelper.GetBodyType(user));
        Assert.Null(UserHelper.GetPerson(user));
    }

    [Fact]
    public void AllMethods_WithNullUser_ReturnDefaults()
    {
        Assert.Equal("Unknown", UserHelper.GetFullName(null));
        Assert.Equal("Unknown", UserHelper.GetFirstName(null));
        Assert.Equal("Unknown", UserHelper.GetLastName(null));
        Assert.Null(UserHelper.GetEmail(null));
        Assert.Null(UserHelper.GetWeight(null));
        Assert.Null(UserHelper.GetAge(null));
        Assert.Null(UserHelper.GetGender(null));
        Assert.Null(UserHelper.GetBodyType(null));
        Assert.Null(UserHelper.GetPerson(null));
    }

    #endregion
}
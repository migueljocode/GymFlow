using GymFlow.Models.Entities;
using GymFlow.Models.Enums;

namespace GymFlow.Api.Helpers;

public static class UserHelper
{
    /// <summary>
    /// دریافت نام کامل کاربر از Person
    /// </summary>
    public static string GetFullName(User? user)
    {
        if (user?.Person == null) return "Unknown";
        return $"{user.Person.FirstName} {user.Person.LastName}";
    }
    
    /// <summary>
    /// دریافت نام کوچک کاربر
    /// </summary>
    public static string GetFirstName(User? user)
    {
        return user?.Person?.FirstName ?? "Unknown";
    }
    
    /// <summary>
    /// دریافت نام خانوادگی کاربر
    /// </summary>
    public static string GetLastName(User? user)
    {
        return user?.Person?.LastName ?? "Unknown";
    }
    
    /// <summary>
    /// دریافت ایمیل کاربر
    /// </summary>
    public static string? GetEmail(User? user)
    {
        return user?.Person?.Email;
    }
    
    /// <summary>
    /// دریافت وزن کاربر
    /// </summary>
    public static float? GetWeight(User? user)
    {
        return user?.Person?.Weight;
    }
    
    /// <summary>
    /// دریافت سن کاربر
    /// </summary>
    public static int? GetAge(User? user)
    {
        return user?.Person?.Age;
    }
    
    /// <summary>
    /// دریافت جنسیت کاربر
    /// </summary>
    public static Gender? GetGender(User? user)
    {
        return user?.Person?.Gender;
    }
    
    /// <summary>
    /// دریافت تیپ بدنی کاربر
    /// </summary>
    public static BodyType? GetBodyType(User? user)
    {
        return user?.Person?.BodyType;
    }
    
    /// <summary>
    /// دریافت اطلاعات کامل Person (برای مواردی که نیاز به دسترسی مستقیم است)
    /// </summary>
    public static Person? GetPerson(User? user)
    {
        return user?.Person;
    }
}
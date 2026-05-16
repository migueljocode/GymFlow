using Microsoft.AspNetCore.Mvc;
using GymFlow.Api.Controllers.Base;
using GymFlow.Dal.Repositories.Interfaces;

namespace GymFlow.Api.Controllers;

[Tags("Coaches")]
public class CoachesController : ApiControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ICoachRepository _coachRepository;

    public CoachesController(IUserRepository userRepository, ICoachRepository coachRepository)
    {
        _userRepository = userRepository;
        _coachRepository = coachRepository;
    }

    [HttpGet("{userId:int}/clients")]
    public async Task<IActionResult> GetClientsAsync(int userId)
    {
        var user = await _userRepository.GetUserWithPersonAsync(userId);
        if (user == null)
            return NotFoundResponse("User", userId);

        var person = user.Person;
        if (person == null)
            return NotFoundResponse("Person for user", userId);

        var coach = await _coachRepository.GetByPersonIdAsync(person.Id);
        if (coach == null)
            return NotFoundResponse("Coach for user", userId);

        var clients = await _userRepository.FindAsync(u => u.CoachId == coach.Id);
        var result = clients.Select(c => new
        {
            c.Id,
            FullName = c.Person == null ? "Unknown" : $"{c.Person.FirstName} {c.Person.LastName}",
            c.Goal,
            CurrentWeight = c.Person?.Weight
        });

        return Success(result);
    }
}
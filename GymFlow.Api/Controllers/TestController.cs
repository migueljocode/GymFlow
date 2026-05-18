namespace GymFlow.Api.Controllers;

/// <summary>
/// Test controller for checking system health and database status
/// </summary>
[Tags("Test")]
public class TestController : ApiControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IExerciseRepository _exerciseRepository;
    private readonly IWorkoutPlanRepository _workoutPlanRepository; 
    private readonly AppDbContext _context;

    public TestController(
        IUserRepository userRepository,
        IExerciseRepository exerciseRepository,
        IWorkoutPlanRepository workoutPlanRepository,
        AppDbContext context,
        IPersonRepository personRepository)
    {
        _userRepository = userRepository;
        _exerciseRepository = exerciseRepository;
        _workoutPlanRepository = workoutPlanRepository;
        _context = context;
        _personRepository = personRepository;
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Success<object>(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
        });
    }

    /// <summary>
    /// Database status check
    /// </summary>
    [HttpGet("db-status")]
    public async Task<IActionResult> DatabaseStatusAsync()
    {
        try
        {
            var userCount = await _userRepository.CountAsync();
            var exerciseCount = await _exerciseRepository.CountAsync();
            var planCount = await _workoutPlanRepository.CountAsync();
            
            return Success<object>(new
            {
                connected = true,
                userCount,
                exerciseCount,
                workoutPlanCount = planCount,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return Error($"Database connection failed: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Get demo user
    /// </summary>
    [HttpGet("demo-user")]
    public async Task<IActionResult> GetDemoUserAsync()
    {
        var demoUser = await _userRepository.GetUserByEmailAsync("demo@gymflow.com");
        
        if (demoUser is null)
            return NotFoundResponse("Demo user");
        
        return Success<object>(new
    {
        demoUser.Id,
        firstName = UserHelper.GetFirstName(demoUser),
        lastName = UserHelper.GetLastName(demoUser),
        fullName = UserHelper.GetFullName(demoUser),
        email = UserHelper.GetEmail(demoUser),
        weight = UserHelper.GetWeight(demoUser),
        goal = demoUser.Goal,
        message = "Demo user available. Use this account for testing."
    });

    }

    /// <summary>
    /// Get system summary
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummaryAsync()
    {
        var users = await _userRepository.GetAllAsync();
        var userList = users.ToList();
        
        var plans = await _workoutPlanRepository.GetAllAsync();
        var exercises = await _exerciseRepository.GetAllAsync();
        
        return Success<object>(new
        {
            users = new
            {
                total = userList.Count,
                byGoal = userList.GroupBy(u => u.Goal.ToString())
                    .ToDictionary(g => g.Key, g => g.Count()),
                averageAge = (float)userList.Average(u => UserHelper.GetAge(u) ?? 0),
                averageWeight = (float)userList.Average(u => UserHelper.GetWeight(u) ?? 0),
            },
            workoutPlans = new
            {
                total = plans.Count(),
                active = plans.Count(p => p.IsActive)
            },
            exercises = new
            {
                total = exercises.Count(),
                byMuscleGroup = exercises.GroupBy(e => e.PrimaryMuscleGroup.ToString())
                    .ToDictionary(g => g.Key, g => g.Count())
            },
            timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsersAsync()
    {
        var persons = await _context.Persons
            .Select(p => new { p.Id, p.Username, p.Password, p.FirstName, p.LastName })
            .ToListAsync();
        
        return Success(persons);
    }

    [HttpGet("person/{username}")]
    public async Task<IActionResult> GetPersonByUsernameAsync(string username)
    {
        var person = await _personRepository.GetByUsernameAsync(username);
        if (person == null)
            return NotFound($"Person with username '{username}' not found");
        
        return Ok(new
        {
            person.Id,
            person.Username,
            person.Password,
            person.FirstName,
            person.LastName
        });
    }
}
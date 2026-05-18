namespace GymFlow.Api.Controllers.Base;

/// <summary>
/// Base controller with standardized response methods and common functionality
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Returns a standardized success response
    /// </summary>
    protected IActionResult Success<T>(T? data, string? message = null)
    {
        return Ok(new
        {
            success = true,
            message,
            data,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Returns a standardized success response with pagination
    /// </summary>
    protected IActionResult SuccessWithPagination<T>(T? data, int totalCount, int page, int pageSize, string? message = null)
    {
        return Ok(new
        {
            success = true,
            message,
            data,
            pagination = new
            {
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            },
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Returns a standardized created response (201)
    /// </summary>
    protected IActionResult CreatedResponse<T>(T data, string? message = null)
    {
        return StatusCode(201, new
        {
            success = true,
            message = message ?? "Resource created successfully",
            data,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Returns a standardized error response
    /// </summary>
    protected IActionResult Error(string message, int statusCode = 400, List<string>? errors = null)
    {
        return StatusCode(statusCode, new
        {
            success = false,
            error = message,
            errors,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Returns a not found response (404)
    /// </summary>
    protected IActionResult NotFoundResponse(string entityName, object? identifier = null)
    {
        var message = identifier is null
            ? $"{entityName} not found"
            : $"{entityName} with identifier '{identifier}' not found";
        
        return NotFound(new
        {
            success = false,
            error = message,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Returns a bad request response (400) with validation errors
    /// </summary>
    protected IActionResult ValidationErrorResponse()
    {
        var errors = ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value?.Errors.Select(e => e.ErrorMessage) ?? Array.Empty<string>())
            .ToList();

        return Error("Validation failed", 400, errors);
    }

    /// <summary>
    /// Gets the current user ID from claims (when authentication is added)
    /// </summary>
    protected int GetCurrentUserId()
    {
        // TEMPORARY: Returns demo user ID
        return 1;
    }

    /// <summary>
    /// Checks if the current user is authenticated (when authentication is added)
    /// </summary>
    protected bool IsAuthenticated()
    {
        // TEMPORARY: Returns true
        return true;
    }
}
namespace GymFlow.Web.Services;

public class ApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ApiClient> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiClient(
        IHttpClientFactory httpClientFactory,
        ILogger<ApiClient> logger,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    private string? GetAuthToken()
    {
        return _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
    }

    private void SetAuthToken(string? token)
    {
        if (token is null)
            _httpContextAccessor.HttpContext?.Session.Remove("AuthToken");
        else
            _httpContextAccessor.HttpContext?.Session.SetString("AuthToken", token);
    }

    public bool IsLoggedIn => !string.IsNullOrEmpty(GetAuthToken());

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient();
        var token = GetAuthToken();
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Add("Authorization", token);
        }
        return client;
    }

    private string GetBaseUrl()
    {
        return _configuration["ApiBaseUrl"] ?? "http://localhost:5291/";
    }

    public virtual async Task<int> GetUserIdAsync()
    {
        var result = await GetAsync<JsonElement?>("api/auth/me");
        if (result.HasValue)
        {
            if (result.Value.TryGetProperty("id", out var idProp))
                return idProp.GetInt32();
        }
        return 0;
    }

    public virtual async Task<(bool Success, int UserId)> LoginAsync(string username, string password)
    {
        using var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(GetBaseUrl());
        
        var loginData = new { username, password };
        var json = JsonSerializer.Serialize(loginData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await client.PostAsync("api/auth/login", content);
        var responseString = await response.Content.ReadAsStringAsync();
        
        if (response.IsSuccessStatusCode)
        {
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
            var token = $"Basic {credentials}";
            SetAuthToken(token);
            
            try
            {
                var result = JsonSerializer.Deserialize<ApiResponse<LoginResponseData>>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var userId = result?.Data?.Id ?? 0;
                return (true, userId);
            }
            catch
            {
                return (true, 0);
            }
        }
        
        return (false, 0);
    }

    public virtual void Logout()
    {
        SetAuthToken(null);
    }

    // ========== GET ==========
    public virtual async Task<T?> GetAsync<T>(string url)
    {
        try
        {
            using var client = CreateClient();
            var fullUrl = $"{GetBaseUrl()}{url}";

            _logger.LogInformation($"Calling API: {fullUrl}");

            var response = await client.GetAsync(fullUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<T>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result != null && result.Success ? result.Data : default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error GET {url}");
            return default;
        }
    }

    // ========== POST با بازگشت خطا ==========
    public virtual async Task<(T? Data, string? ErrorMessage)> PostWithErrorAsync<T>(string url, object data)
    {
        try
        {
            using var client = CreateClient();
            var fullUrl = $"{GetBaseUrl()}{url}";
            _logger.LogInformation($"POST Request to: {fullUrl}");

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(fullUrl, content);
            var responseJson = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation($"Response: {response.StatusCode} - {responseJson}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"API returned {response.StatusCode}: {responseJson}");
                
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiErrorResponse>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (default, errorResponse?.Error ?? $"خطا با کد {response.StatusCode}");
                }
                catch
                {
                    return (default, $"خطا با کد {response.StatusCode}");
                }
            }

            var result = JsonSerializer.Deserialize<ApiResponse<T>>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result != null && result.Success)
            {
                return (result.Data, null);
            }
            
            return (default, result?.Message ?? "خطای ناشناخته");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error POST {url}");
            return (default, ex.Message);
        }
    }

    // ========== POST (سازگاری با کدهای قبلی) ==========
    public virtual async Task<T?> PostAsync<T>(string url, object data)
    {
        var (result, _) = await PostWithErrorAsync<T>(url, data);
        return result;
    }

    // ========== PUT ==========
    public virtual async Task<bool> PutAsync(string url, object data)
    {
        try
        {
            using var client = CreateClient();
            var fullUrl = $"{GetBaseUrl()}{url}";

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(fullUrl, content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error PUT {url}");
            return false;
        }
    }

    // ========== DELETE ==========
    public virtual async Task<bool> DeleteAsync(string url)
    {
        try
        {
            using var client = CreateClient();
            var fullUrl = $"{GetBaseUrl()}{url}";

            var response = await client.DeleteAsync(fullUrl);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error DELETE {url}");
            return false;
        }
    }

    // ========== DOWNLOAD PDF ==========
    public virtual async Task<byte[]?> DownloadPdfAsync(string url)
    {
        try
        {
            using var client = CreateClient();
            var fullUrl = $"{GetBaseUrl()}{url}";

            _logger.LogInformation($"Downloading PDF from: {fullUrl}");

            var response = await client.GetAsync(fullUrl);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error downloading PDF from {url}");
            return null;
        }
    }

    public virtual async Task<bool> IsUserCoachAsync(int userId)
    {
        try
        {
            var response = await GetAsync<object>($"api/coaches/user/{userId}");
            return response != null;
        }
        catch
        {
            return false;
        }
    }
}
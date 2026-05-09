using System.Text;
using System.Text.Json;

namespace GymFlow.Web.Services;

public class ApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ApiClient> _logger;
    private readonly IConfiguration _configuration;

    public ApiClient(
        IHttpClientFactory httpClientFactory,
        ILogger<ApiClient> logger,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
    }

    private HttpClient CreateClient()
    {
        return _httpClientFactory.CreateClient();
    }

    private string GetBaseUrl()
    {
        return _configuration["ApiBaseUrl"] ?? "http://localhost:5291/";
    }

    // ========== GET ==========
    public async Task<T?> GetAsync<T>(string url)
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
            
            return result != null ? result.Data : default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error GET {url}");
            return default;
        }
    }

    // ========== POST ==========
    public async Task<T?> PostAsync<T>(string url, object data)
    {
        try
        {
            using var client = CreateClient();
            var fullUrl = $"{GetBaseUrl()}{url}";
            
            _logger.LogInformation($"Calling API POST: {fullUrl}");
            
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await client.PostAsync(fullUrl, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"API returned {response.StatusCode}: {errorContent}");
                return default;
            }
            
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<T>>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return result != null ? result.Data : default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error POST {url}");
            return default;
        }
    }

    // ========== PUT ==========
    public async Task<bool> PutAsync(string url, object data)
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
    public async Task<bool> DeleteAsync(string url)
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
    public async Task<byte[]?> DownloadPdfAsync(string url)
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
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public DateTime Timestamp { get; set; }
}
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using nekohub_maui.Models;
using nekohub_maui.Extensions;

namespace nekohub_maui.Services;

public sealed class PostsApiService : IPostsApi
{
    private readonly HttpClient _http;
    private readonly ILogger<PostsApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public PostsApiService(HttpClient http, ILogger<PostsApiService> logger)
    {
        _http = http;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<List<Post>> GetListAsync(bool? isPublished = null, string? keyword = null, string? sortBy = null, string? sortOrder = null, CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"api/posts{BuildQueryString(new Dictionary<string, string?>
        {
            ["isPublished"] = isPublished?.ToString().ToLowerInvariant(),
            ["keyword"] = keyword,
            ["sortBy"] = sortBy,
            ["sortOrder"] = sortOrder
        })}", UriKind.Relative);

        // Polly-like simple retry with exponential backoff (3 attempts)
        var delay = TimeSpan.FromMilliseconds(200);
        Exception? lastEx = null;
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, uri);
                using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if (resp.IsSuccessStatusCode)
                {
                    var posts = await resp.Content.ReadFromJsonAsync<List<Post>>(_jsonOptions, cancellationToken) ?? new List<Post>();
                    return posts;
                }
                await ThrowForResponse(resp, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastEx = ex;
                _logger.LogWarning(ex, "GET list attempt {Attempt} failed", attempt);
                if (attempt == 3) break;
                await Task.Delay(delay, cancellationToken);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
            }
        }
        throw lastEx ?? new Exception("Unknown error while fetching posts list");
    }

    public async Task<PagedResponse<Post>> GetPagedAsync(int page = 1, int pageSize = 10, bool? isPublished = null, string? keyword = null, string? sortBy = null, string? sortOrder = null, CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"api/posts/paged{BuildQueryString(new Dictionary<string, string?>
        {
            ["page"] = page.ToString(),
            ["pageSize"] = pageSize.ToString(),
            ["isPublished"] = isPublished?.ToString().ToLowerInvariant(),
            ["keyword"] = keyword,
            ["sortBy"] = sortBy,
            ["sortOrder"] = sortOrder
        })}", UriKind.Relative);

        // Retry GETs with exponential backoff
        var delay = TimeSpan.FromMilliseconds(200);
        Exception? lastEx = null;
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, uri);
                using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if (resp.IsSuccessStatusCode)
                {
                    var result = await resp.Content.ReadFromJsonAsync<PagedResponse<Post>>(_jsonOptions, cancellationToken) ?? new PagedResponse<Post>();
                    // Ensure null Items is handled
                    result.Items ??= new List<Post>();
                    return result;
                }
                await ThrowForResponse(resp, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastEx = ex;
                _logger.LogWarning(ex, "GET paged attempt {Attempt} failed", attempt);
                if (attempt == 3) break;
                await Task.Delay(delay, cancellationToken);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
            }
        }
        throw lastEx ?? new Exception("Unknown error while fetching paged posts");
    }

    public async Task<Post> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"api/posts/{id}", UriKind.Relative);
        var delay = TimeSpan.FromMilliseconds(200);
        Exception? lastEx = null;
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                using var resp = await _http.GetAsync(uri, cancellationToken);
                if (resp.IsSuccessStatusCode)
                {
                    var post = await resp.Content.ReadFromJsonAsync<Post>(_jsonOptions, cancellationToken) ?? new Post();
                    return post;
                }
                await ThrowForResponse(resp, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastEx = ex;
                _logger.LogWarning(ex, "GET by id attempt {Attempt} failed", attempt);
                if (attempt == 3) break;
                await Task.Delay(delay, cancellationToken);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
            }
        }
        throw lastEx ?? new Exception($"Unknown error while fetching post {id}");
    }

    public async Task<Post> CreateAsync(string title, string content, bool isPublished, CancellationToken cancellationToken = default)
    {
        var uri = new Uri("api/posts", UriKind.Relative);
        var body = new { title, content, isPublished };
        using var resp = await _http.PostAsJsonAsync(uri, body, _jsonOptions, cancellationToken);
        if (resp.IsSuccessStatusCode)
        {
            var created = await resp.Content.ReadFromJsonAsync<Post>(_jsonOptions, cancellationToken) ?? new Post();
            return created;
        }
        await ThrowForResponse(resp, cancellationToken);
        throw new InvalidOperationException("Unreachable");
    }

    public async Task<Post> UpdateAsync(int id, string title, string content, bool isPublished, CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"api/posts/{id}", UriKind.Relative);
        var body = new { id, title, content, isPublished };
        using var resp = await _http.PutAsJsonAsync(uri, body, _jsonOptions, cancellationToken);
        if (resp.IsSuccessStatusCode)
        {
            var updated = await resp.Content.ReadFromJsonAsync<Post>(_jsonOptions, cancellationToken) ?? new Post();
            return updated;
        }
        await ThrowForResponse(resp, cancellationToken);
        throw new InvalidOperationException("Unreachable");
    }

    public async Task<Post> PatchAsync(int id, PostPatchRequest patch, CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"api/posts/{id}", UriKind.Relative);
        var json = JsonSerializer.Serialize(patch, _jsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var resp = await _http.PatchAsync(uri, content, cancellationToken);
        if (resp.IsSuccessStatusCode)
        {
            var updated = await resp.Content.ReadFromJsonAsync<Post>(_jsonOptions, cancellationToken) ?? new Post();
            return updated;
        }
        await ThrowForResponse(resp, cancellationToken);
        throw new InvalidOperationException("Unreachable");
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"api/posts/{id}", UriKind.Relative);
        using var resp = await _http.DeleteAsync(uri, cancellationToken);
        if (resp.IsSuccessStatusCode)
        {
            return;
        }
        await ThrowForResponse(resp, cancellationToken);
    }

    public async Task<Post> PublishAsync(int id, CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"api/posts/{id}/publish", UriKind.Relative);
        using var resp = await _http.PostAsync(uri, content: null, cancellationToken);
        if (resp.IsSuccessStatusCode)
        {
            var post = await resp.Content.ReadFromJsonAsync<Post>(_jsonOptions, cancellationToken) ?? new Post();
            return post;
        }
        await ThrowForResponse(resp, cancellationToken);
        throw new InvalidOperationException("Unreachable");
    }

    public async Task<Post> UnpublishAsync(int id, CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"api/posts/{id}/unpublish", UriKind.Relative);
        using var resp = await _http.PostAsync(uri, content: null, cancellationToken);
        if (resp.IsSuccessStatusCode)
        {
            var post = await resp.Content.ReadFromJsonAsync<Post>(_jsonOptions, cancellationToken) ?? new Post();
            return post;
        }
        await ThrowForResponse(resp, cancellationToken);
        throw new InvalidOperationException("Unreachable");
    }

    public async Task<PostStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var uri = new Uri("api/posts/stats", UriKind.Relative);
        var delay = TimeSpan.FromMilliseconds(200);
        Exception? lastEx = null;
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                using var resp = await _http.GetAsync(uri, cancellationToken);
                if (resp.IsSuccessStatusCode)
                {
                    var stats = await resp.Content.ReadFromJsonAsync<PostStats>(_jsonOptions, cancellationToken) ?? new PostStats();
                    return stats;
                }
                await ThrowForResponse(resp, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastEx = ex;
                _logger.LogWarning(ex, "GET stats attempt {Attempt} failed", attempt);
                if (attempt == 3) break;
                await Task.Delay(delay, cancellationToken);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
            }
        }
        throw lastEx ?? new Exception("Unknown error while fetching stats");
    }

    private static string BuildQueryString(Dictionary<string, string?> values)
    {
        var parts = new List<string>();
        foreach (var kv in values)
        {
            if (string.IsNullOrWhiteSpace(kv.Value)) continue;
            parts.Add($"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}");
        }
        return parts.Count == 0 ? string.Empty : "?" + string.Join("&", parts);
    }

    private async Task ThrowForResponse(HttpResponseMessage resp, CancellationToken ct)
    {
        var status = resp.StatusCode;
        string? body = null;
        try
        {
            body = await resp.Content.ReadAsStringAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed reading error response body");
        }

        ValidationProblemDetails? validation = null;
        ProblemDetails? problem = null;

        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                validation = JsonSerializer.Deserialize<ValidationProblemDetails>(body, _jsonOptions);
                // Only consider it a validation problem if Errors has entries
                if (validation != null && (validation.Errors?.Count ?? 0) == 0)
                {
                    validation = null;
                }
            }
            catch { /* ignore */ }

            if (validation == null)
            {
                try { problem = JsonSerializer.Deserialize<ProblemDetails>(body, _jsonOptions); } catch { /* ignore */ }
            }
        }

        var message = problem?.Title ?? resp.ReasonPhrase ?? "API Error";
        throw new ApiException(status, message, body, validation, problem);
    }
}

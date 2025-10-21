using nekohub_maui.Models;

namespace nekohub_maui.Services;

public interface IPostsApi
{
    Task<List<Post>> GetListAsync(bool? isPublished = null, string? keyword = null, string? sortBy = null, string? sortOrder = null, CancellationToken cancellationToken = default);
    Task<PagedResponse<Post>> GetPagedAsync(int page = 1, int pageSize = 10, bool? isPublished = null, string? keyword = null, string? sortBy = null, string? sortOrder = null, CancellationToken cancellationToken = default);
    Task<Post> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Post> CreateAsync(string title, string content, bool isPublished, CancellationToken cancellationToken = default);
    Task<Post> UpdateAsync(int id, string title, string content, bool isPublished, CancellationToken cancellationToken = default);
    Task<Post> PatchAsync(int id, PostPatchRequest patch, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Post> PublishAsync(int id, CancellationToken cancellationToken = default);
    Task<Post> UnpublishAsync(int id, CancellationToken cancellationToken = default);
    Task<PostStats> GetStatsAsync(CancellationToken cancellationToken = default);
}

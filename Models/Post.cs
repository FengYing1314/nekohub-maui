namespace nekohub_maui.Models;

public sealed class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsPublished { get; set; }

    // Optional fields the backend may add in the future
    public string? Author { get; set; }
}

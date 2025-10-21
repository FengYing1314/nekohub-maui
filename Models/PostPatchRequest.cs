namespace nekohub_maui.Models;

public sealed class PostPatchRequest
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public bool? IsPublished { get; set; }
}

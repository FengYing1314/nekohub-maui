namespace nekohub_maui.Models;

// RFC7807 ProblemDetails minimal shape
public class ProblemDetails
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public int? Status { get; set; }
    public string? Detail { get; set; }
}

public sealed class ValidationProblemDetails : ProblemDetails
{
    public Dictionary<string, string[]> Errors { get; set; } = new();
}

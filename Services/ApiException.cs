using System.Net;
using nekohub_maui.Models;

namespace nekohub_maui.Services;

public class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string? ResponseBody { get; }
    public ValidationProblemDetails? Validation { get; }
    public ProblemDetails? Problem { get; }

    public ApiException(HttpStatusCode statusCode, string message, string? responseBody = null, ValidationProblemDetails? validation = null, ProblemDetails? problem = null)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
        Validation = validation;
        Problem = problem;
    }
}

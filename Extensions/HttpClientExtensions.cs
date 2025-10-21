namespace nekohub_maui.Extensions;

public static class HttpClientExtensions
{
    public static Task<HttpResponseMessage> PatchAsync(this HttpClient client, Uri requestUri, HttpContent? content, CancellationToken cancellationToken = default)
    {
        var req = new HttpRequestMessage(HttpMethod.Patch, requestUri)
        {
            Content = content
        };
        return client.SendAsync(req, cancellationToken);
    }
}

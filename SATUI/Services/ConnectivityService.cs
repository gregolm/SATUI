using System.Net.Http;

namespace SATUI.Services;

public class ConnectivityService : IConnectivityService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

    public ConnectivityService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<bool> IsReachableAsync(string url, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            return false;

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(DefaultTimeout);

            var client = _httpClientFactory.CreateClient(nameof(ConnectivityService));
            using var request = new HttpRequestMessage(HttpMethod.Head, url);
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);
            return response.IsSuccessStatusCode || (int)response.StatusCode < 500;
        }
        catch
        {
            return false;
        }
    }
}

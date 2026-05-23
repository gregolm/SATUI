using System.Net;

namespace SATUI.UITests.Infrastructure;

/// <summary>
/// Minimal embedded HTTP server that serves 200 OK responses.
/// Gives the app a reachable URL so connectivity checks pass during UI tests.
/// Uses a dynamically allocated port to avoid conflicts.
/// </summary>
public sealed class LocalHttpServer : IDisposable
{
    private readonly HttpListener _listener = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _serveTask;

    public string BaseUrl { get; }
    /// <summary>Just the host:port without protocol, ready to paste into the URL field.</summary>
    public string HostAndPort { get; }

    public LocalHttpServer()
    {
        // Get a free port from the OS
        var socket = new System.Net.Sockets.Socket(
            System.Net.Sockets.AddressFamily.InterNetwork,
            System.Net.Sockets.SocketType.Stream,
            System.Net.Sockets.ProtocolType.Tcp);
        socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        var port = ((IPEndPoint)socket.LocalEndPoint!).Port;
        socket.Close();

        BaseUrl = $"http://127.0.0.1:{port}/";
        HostAndPort = $"127.0.0.1:{port}";
        _listener.Prefixes.Add(BaseUrl);
        _listener.Start();
        _serveTask = Task.Run(ServeAsync);
    }

    private async Task ServeAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                var ctx = await _listener.GetContextAsync().ConfigureAwait(false);
                var body = "<html><body>SATUI UI Test Server</body></html>"u8.ToArray();
                ctx.Response.StatusCode = 200;
                ctx.Response.ContentLength64 = body.Length;
                await ctx.Response.OutputStream.WriteAsync(body).ConfigureAwait(false);
                ctx.Response.OutputStream.Close();
            }
            catch (HttpListenerException) { break; }
            catch { /* absorb transient errors */ }
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _listener.Stop();
        try { _serveTask.Wait(2_000); } catch { /* ignore */ }
        _cts.Dispose();
    }
}


namespace SATUI.Services;

public interface IConnectivityService
{
    Task<bool> IsReachableAsync(string url, CancellationToken cancellationToken = default);
}

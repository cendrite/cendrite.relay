public interface IConnectionIdFetcher
{
    Task<Dictionary<string, Guid>> FetchClientConnectionIds(string userTag, CancellationToken ct);
}

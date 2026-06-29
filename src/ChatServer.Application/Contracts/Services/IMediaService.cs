public interface IMediaService 
{
    public Task<string> StoreAsync(string id, Stream content, CancellationToken ct = default);
}

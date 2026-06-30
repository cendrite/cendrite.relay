namespace ChatServer.Application.Contracts.Services;

public interface IMediaService
{
    public Task StoreAsync(string id, Stream content, CancellationToken ct = default);
}

using ChatServer.Application.Contracts.Services;
using ChatServer.Infrastructure.Storage;

namespace ChatServer.Infrastructure.Services;

internal sealed class MediaService(IBlobStore blobStore) : IMediaService
{
    public async Task StoreAsync(
        string id,
        Stream content,
        CancellationToken ct = default) => await blobStore.PutAsync(id, content, ct);

    public void Delete(string id) => blobStore.Delete(id);

    public Stream Get(string id) => blobStore.Open(id);
}

using ChatServer.Infrastructure.Storage;

namespace ChatServer.Infrastructure.Services;

internal sealed class MediaService(IBlobStore blobStore) : IMediaService
{
    public async Task<string> StoreAsync(string id, Stream content, CancellationToken ct = default)
    {
        await blobStore.PutAsync(id, content, ct);
        return id;
    }

    public Stream Open(string id)
    {
        return blobStore.Open(id);
    }

    public void Delete(string id)
    {
        blobStore.Delete(id);
    }
}

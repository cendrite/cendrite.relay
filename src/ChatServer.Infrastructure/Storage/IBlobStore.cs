namespace ChatServer.Infrastructure.Storage;

internal interface IBlobStore
{
    Task PutAsync(string id, Stream content, CancellationToken ct = default);
    void Delete(string Id);
    Stream Open(string Id);
}

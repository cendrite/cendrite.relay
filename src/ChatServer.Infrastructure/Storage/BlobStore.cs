using ChatServer.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace ChatServer.Infrastructure.Storage;

internal sealed class BlobStore(IOptions<BlobStoreSettings> options) : IBlobStore
{
    public async Task PutAsync(string id, Stream content, CancellationToken ct = default)
    {
        var path = GeneratePath(id);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        var tmpPath = path + ".tmp-" + Guid.NewGuid().ToString("N");

        try
        {
            using var fileStream = new FileStream(tmpPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await content.CopyToAsync(fileStream, ct);
            await fileStream.FlushAsync(ct);
            fileStream.Close();
            File.Move(tmpPath, path, true);
        }
        catch
        {
            if (File.Exists(tmpPath))
            {
                File.Delete(tmpPath);
            }
            throw;
        }
    }

    public Stream Open(string id)
    {
        return new FileStream(
            GeneratePath(id),
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true);
    }

    public void Delete(string id)
    {
        var path = GeneratePath(id);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private string GeneratePath(string id)
    {
        if (string.IsNullOrEmpty(id) || id.Length < 4)
        {
            throw new UnauthorizedAccessException($"Invalid media id '{id}'.");
        }

        var root = Path.GetFullPath(options.Value.Root);
        var rootWithSeparator = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;

        var path = Path.GetFullPath(Path.Combine(root, id[..2], id.Substring(2, 2), id));

        if (!path.StartsWith(rootWithSeparator, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException($"Resolved media path escapes the blob store root for id '{id}'.");
        }

        return path;
    }
}

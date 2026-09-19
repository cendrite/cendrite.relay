using ChatServer.Application.Abstractions;

namespace ChatServer.Application.Specifications.Client;

internal sealed class ClientWithMediaByMediaIdSpec : Specification<Domain.Aggregates.Client>
{
    public ClientWithMediaByMediaIdSpec(string mediaId) : base(c => c.Media.Any(m => m.MediaId == mediaId))
    {
        AddIncludes($"{nameof(Domain.Aggregates.Client.Media)}");
    }
}

using ChatServer.Application.Features.ChangeEvent.Base;

namespace ChatServer.Application.Contracts.Services;

public interface IChangeEventNotifier
{
    Task NotifyAsync(string transientId, ChangeEvent changeEvent, CancellationToken cancellationToken = default);
}

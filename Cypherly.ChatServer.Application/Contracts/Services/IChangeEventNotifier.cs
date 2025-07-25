using Cypherly.ChatServer.Application.Features.ChangeEvent.Base;

namespace Cypherly.ChatServer.Application.Contracts;

public interface IChangeEventNotifier
{
    Task NotifyAsync(string transientId, ChangeEvent changeEvent, CancellationToken cancellationToken = default);
}
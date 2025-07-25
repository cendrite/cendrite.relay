using Cypherly.ChatServer.Application.Features.ChangeEvent.Base;

namespace Cypherly.ChatServer.API.Clients;

public interface IChangeEventClient : IClient
{
    Task ChangeNotification(ChangeEvent changeEvent);
}
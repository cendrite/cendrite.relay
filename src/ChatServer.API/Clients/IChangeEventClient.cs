using ChatServer.Application.Features.ChangeEvent.Base;

namespace ChatServer.API.Clients;

public interface IChangeEventClient : IClient
{
    Task ChangeNotification(ChangeEvent changeEvent);
}
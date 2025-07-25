using Cypherly.ChatServer.Domain.Abstractions;
using MediatR;

namespace Cypherly.ChatServer.Application.Abstractions;

public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent> where TDomainEvent : IDomainEvent
{

}
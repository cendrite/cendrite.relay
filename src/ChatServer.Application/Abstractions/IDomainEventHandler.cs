using ChatServer.Domain.Abstractions;
using MediatR;

namespace ChatServer.Application.Abstractions;

public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent> where TDomainEvent : IDomainEvent
{

}
using ChatServer.Domain.Common;
using Mediator;

namespace ChatServer.Application.Abstractions;

public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }

public interface IQueryLimited<TResponse> : IRequest<Result<TResponse>>
{
    public Guid UserProfileId { get; init; }
}
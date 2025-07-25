using Cypherly.ChatServer.Domain.Common;
using MediatR;

namespace Cypherly.ChatServer.Application.Abstractions;

public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }

public interface IQueryLimited<TResponse> : IRequest<Result<TResponse>>
{
    public Guid UserProfileId { get; init; }
}
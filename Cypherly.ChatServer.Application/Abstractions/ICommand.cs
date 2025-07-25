using Cypherly.ChatServer.Domain.Common;
using MediatR;

namespace Cypherly.ChatServer.Application.Abstractions;

public interface ICommand : IRequest<Result> { }
public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }

public interface ICommandId : ICommand
{
    public Guid Id { get; init; }
}

public interface ICommandId<TResponse> : ICommand<TResponse>
{
    public Guid Id { get; init; }
}
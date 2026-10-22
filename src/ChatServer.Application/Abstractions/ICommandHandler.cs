using ChatServer.Domain.Common;
using Mediator;

namespace ChatServer.Application.Abstractions;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result> where TCommand : ChatServer.Application.Abstractions.ICommand { }
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>> where TCommand : ChatServer.Application.Abstractions.ICommand<TResponse> { }
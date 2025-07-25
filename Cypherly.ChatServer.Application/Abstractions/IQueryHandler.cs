using Cypherly.ChatServer.Domain.Common;
using MediatR;

namespace Cypherly.ChatServer.Application.Abstractions;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>> where TQuery : ChatServer.Application.Abstractions.IQuery<TResponse> { }

public interface ILimitedQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>> where TQuery : ChatServer.Application.Abstractions.IQueryLimited<TResponse> { }
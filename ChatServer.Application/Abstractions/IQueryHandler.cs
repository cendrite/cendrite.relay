using ChatServer.Domain.Common;
using MediatR;

namespace ChatServer.Application.Abstractions;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>> where TQuery : ChatServer.Application.Abstractions.IQuery<TResponse> { }

public interface ILimitedQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>> where TQuery : ChatServer.Application.Abstractions.IQueryLimited<TResponse> { }
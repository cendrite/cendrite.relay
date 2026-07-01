using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Domain.Common;

namespace ChatServer.Application.Features.Media.Commands.Update;

public sealed class UpdateMediaCommandHandler(
    IClientRepository clRepo,
    IUnitOfWork uow) 
    : ICommandHandler<UpdateMediaCommand>
{
    public Task<Result> Handle(UpdateMediaCommand cmd, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}

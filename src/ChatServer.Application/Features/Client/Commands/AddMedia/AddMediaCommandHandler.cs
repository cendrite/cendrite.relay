using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Domain.Common;

public sealed class AddMediaCommandHandler(
    IClientRepository clRepo, IUnitOfWork uow) 
    : ICommandHandler<AddMediaCommand>
{
    public Task<Result> Handle(AddMediaCommand cmd, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}

using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Domain.Common;

namespace ChatServer.Application.Features.Client.Commands.UploadMedia;

public sealed class UploadMediaCommandHandler(
    IClientRepository clRepo, IUnitOfWork uow)
    : ICommandHandler<UploadMediaCommand>
{
    public Task<Result> Handle(UploadMediaCommand cmd, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}

namespace ChatServer.Application.Contracts.Repositories;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
using ChatServer.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChatServer.Infrastructure.Extensions;

public static class ConnectionExtensions
{
    public static void ClearStaleConnections(this IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        var logger = services.GetRequiredService<ILogger<ChatServerDbContext>>();

        try
        {
            var dbContext = services.GetRequiredService<ChatServerDbContext>();

            var removed = dbContext.Set<Connection>().ExecuteDelete();

            logger.LogInformation("Cleared {Count} stale connections", removed);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured while attempting to clear stale connections");
            throw;
        }
    }
}

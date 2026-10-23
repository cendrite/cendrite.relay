using ChatServer.Application.Contracts.Repositories;
using ChatServer.Infrastructure.Storage;
using Microsoft.Extensions.Logging;
using Quartz;

namespace ChatServer.Infrastructure.Jobs;

[DisallowConcurrentExecution]
internal sealed class MediaRetentionJob(
    IClientRepository clientRepository,
    IBlobStore blobStore,
    IUnitOfWork unitOfWork,
    ILogger<MediaRetentionJob> logger)
    : IJob
{
    private static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(30);
    private const int BatchSize = 100;

    public async Task Execute(IJobExecutionContext jobContext)
    {
        var ct = jobContext.CancellationToken;

        try
        {
            var cutoff = DateTime.UtcNow - RetentionPeriod;

            var clients = await clientRepository.GetWithExpiredMediaAsync(cutoff, BatchSize, ct);
            if (clients.Count is 0) return;

            foreach (var client in clients)
            {
                var expired = client.Media.ToList();

                foreach (var media in expired)
                {
                    blobStore.Delete(media.MediaId);
                    client.RemoveMedia(media);
                }

                logger.LogInformation(
                    "Purged {Count} media older than {Cutoff:o} for client {ClientId}.", expired.Count, cutoff, client.Id);
            }

            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error purging expired media");
        }
    }
}

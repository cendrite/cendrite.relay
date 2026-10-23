using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Contracts.Services;
using ChatServer.Infrastructure.Jobs;
using ChatServer.Infrastructure.Persistence.Context;
using ChatServer.Infrastructure.Storage;
using ChatServer.Test.Integration.Setup;
using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Quartz;

namespace ChatServer.Test.Integration.Jobs;

public class MediaRetentionJobTest : IntegrationTestbase
{
    private readonly MediaRetentionJob _sut;
    private readonly IMediaService _mediaService;

    public MediaRetentionJobTest(IntegrationTestFactory<Program, ChatServerDbContext> factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        _mediaService = scope.ServiceProvider.GetRequiredService<IMediaService>();

        _sut = new MediaRetentionJob(
            scope.ServiceProvider.GetRequiredService<IClientRepository>(),
            scope.ServiceProvider.GetRequiredService<IBlobStore>(),
            scope.ServiceProvider.GetRequiredService<IUnitOfWork>(),
            NullLogger<MediaRetentionJob>.Instance);
    }

    private async Task<Domain.Entities.Media> SeedMediaAsync(Domain.Aggregates.Client client, DateTime created)
    {
        var media = new Domain.Entities.Media(Guid.NewGuid().ToString(), client.Id, client.ConnectionId, 4);
        client.AddMedia(media);
        Db.Entry(media).Property(m => m.Created).CurrentValue = created;
        await _mediaService.StoreAsync(media.MediaId, new MemoryStream("blob"u8.ToArray()));
        return media;
    }

    [Fact]
    public async Task Execute_Should_Delete_Media_Older_Than_Retention_And_Keep_Recent_Media()
    {
        // Arrange
        var client = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());
        Db.Client.Add(client);

        var expired = await SeedMediaAsync(client, DateTime.UtcNow.AddDays(-31));
        var recent = await SeedMediaAsync(client, DateTime.UtcNow.AddDays(-1));
        await Db.SaveChangesAsync();

        var context = A.Fake<IJobExecutionContext>();
        A.CallTo(() => context.CancellationToken).Returns(CancellationToken.None);

        // Act
        await _sut.Execute(context);

        // Assert
        var stored = await Db.Media.AsNoTracking().Where(m => m.ClientId == client.Id).ToListAsync();
        stored.Should().ContainSingle(m => m.MediaId == recent.MediaId);

        var getExpired = () => _mediaService.Get(expired.MediaId);
        getExpired.Should().Throw<FileNotFoundException>();

        await using var recentBlob = _mediaService.Get(recent.MediaId);
        recentBlob.Should().NotBeNull();
    }

    [Fact]
    public async Task Execute_Should_Do_Nothing_When_No_Media_Is_Expired()
    {
        // Arrange
        var client = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());
        Db.Client.Add(client);

        var recent = await SeedMediaAsync(client, DateTime.UtcNow.AddDays(-1));
        await Db.SaveChangesAsync();

        var context = A.Fake<IJobExecutionContext>();
        A.CallTo(() => context.CancellationToken).Returns(CancellationToken.None);

        // Act
        await _sut.Execute(context);

        // Assert
        var stored = await Db.Media.AsNoTracking().Where(m => m.ClientId == client.Id).ToListAsync();
        stored.Should().ContainSingle(m => m.MediaId == recent.MediaId);
    }
}

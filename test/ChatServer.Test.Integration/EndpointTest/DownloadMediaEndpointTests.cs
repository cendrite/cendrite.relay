using ChatServer.Application.Contracts.Services;
using ChatServer.Infrastructure.Persistence.Context;
using ChatServer.Test.Integration.Setup;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChatServer.Test.Integration.EndpointTest;

public class DownloadMediaEndpointTest : IntegrationTestbase
{
    private readonly IMediaService _mediaService;

    public DownloadMediaEndpointTest(IntegrationTestFactory<Program, ChatServerDbContext> factory) : base(factory)
    {
        _mediaService = factory.Services.CreateScope().ServiceProvider.GetRequiredService<IMediaService>();
    }

    [Fact]
    public async Task DownloadMedia_Should_ReturnFileStream_WhenMediaExists()
    {
        // Arrange
        var testClient = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());
        Db.Client.Add(testClient);
        await Db.SaveChangesAsync();

        const string mediaId = "test-media-id";
        await UploadTestMediaAsync(mediaId, testClient, testClient.ConnectionId);

        var httpClient = CreateClient(testClient.Id.ToString());

        // Act
        var response = await httpClient.GetAsync($"/api/media?id={mediaId}&senderId={testClient.ConnectionId}");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/octet-stream");
        var responseContent = await response.Content.ReadAsByteArrayAsync();
        responseContent.Should().BeEquivalentTo(System.Text.Encoding.UTF8.GetBytes("Test media content"));

        var media = await Db.Client.AsNoTracking().Include(c => c.Media)
            .Where(c => c.Id == testClient.Id).FirstOrDefaultAsync();

        media.Should().NotBeNull();
        media!.Media.Should().ContainSingle(m => m.MediaId == mediaId && m.ClientId == testClient.Id);
        media.Media.First(m => m.MediaId == mediaId).Status.Should().Be(Domain.Enums.MediaStatus.Downloaded);
        media.Media.First(m => m.MediaId == mediaId).SenderId.Should().Be(testClient.ConnectionId);
    }

    [Fact]
    public async Task DownloadMedia_Should_Return500_WhenBlobIsMissing_ButMediaIsRegistered()
    {
        // Arrange
        var testClient = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());
        Db.Client.Add(testClient);
        await Db.SaveChangesAsync();

        // Media is registered for the client, but the blob was never written to the store.
        const string mediaId = "orphan-media-id";
        testClient.AddMedia(new Domain.Entities.Media(mediaId, testClient.Id, testClient.ConnectionId));
        await Db.SaveChangesAsync();

        var httpClient = CreateClient(testClient.Id.ToString());

        // Act - the status update succeeds, but opening the missing blob throws
        var response = await httpClient.GetAsync($"/api/media?id={mediaId}&senderId={testClient.ConnectionId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task DownloadMedia_Should_MarkOnlyRequestedMedia_AsDownloaded_WhenClientHasMultipleMedia()
    {
        // Arrange
        var testClient = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());
        Db.Client.Add(testClient);
        await Db.SaveChangesAsync();

        const string requestedMediaId = "media-one";
        const string otherMediaId = "media-two";
        await UploadTestMediaAsync(requestedMediaId, testClient, testClient.ConnectionId, "content-one");
        await UploadTestMediaAsync(otherMediaId, testClient, testClient.ConnectionId, "content-two");

        var httpClient = CreateClient(testClient.Id.ToString());

        // Act
        var response = await httpClient.GetAsync($"/api/media?id={requestedMediaId}&senderId={testClient.ConnectionId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsByteArrayAsync();
        responseContent.Should().BeEquivalentTo(System.Text.Encoding.UTF8.GetBytes("content-one"));

        var client = await Db.Client.AsNoTracking().Include(c => c.Media)
            .Where(c => c.Id == testClient.Id).FirstOrDefaultAsync();

        client.Should().NotBeNull();
        client!.Media.First(m => m.MediaId == requestedMediaId).Status.Should().Be(Domain.Enums.MediaStatus.Downloaded);
        client.Media.First(m => m.MediaId == otherMediaId).Status.Should().Be(Domain.Enums.MediaStatus.Pending);
    }

    [Fact]
    public async Task DownloadMedia_Should_RemainDownloaded_WhenRequestedMultipleTimes()
    {
        // Arrange
        var testClient = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());
        Db.Client.Add(testClient);
        await Db.SaveChangesAsync();

        const string mediaId = "test-media-id";
        await UploadTestMediaAsync(mediaId, testClient, testClient.ConnectionId);

        var httpClient = CreateClient(testClient.Id.ToString());

        // Act
        var first = await httpClient.GetAsync($"/api/media?id={mediaId}&senderId={testClient.ConnectionId}");
        var second = await httpClient.GetAsync($"/api/media?id={mediaId}&senderId={testClient.ConnectionId}");

        // Assert
        first.EnsureSuccessStatusCode();
        second.EnsureSuccessStatusCode();
        var responseContent = await second.Content.ReadAsByteArrayAsync();
        responseContent.Should().BeEquivalentTo(System.Text.Encoding.UTF8.GetBytes("Test media content"));

        var client = await Db.Client.AsNoTracking().Include(c => c.Media)
            .Where(c => c.Id == testClient.Id).FirstOrDefaultAsync();
        client!.Media.First(m => m.MediaId == mediaId).Status.Should().Be(Domain.Enums.MediaStatus.Downloaded);
    }

    [Fact]
    public async Task DownloadMedia_Should_Return404_AndLeaveStatusPending_WhenSenderIdMatchesNoClient()
    {
        // Arrange
        var testClient = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());
        Db.Client.Add(testClient);
        await Db.SaveChangesAsync();

        const string mediaId = "test-media-id";
        await UploadTestMediaAsync(mediaId, testClient, testClient.ConnectionId);

        var httpClient = CreateClient(testClient.Id.ToString());
        var unknownSenderId = Guid.NewGuid(); // matches no client's ConnectionId

        // Act
        var response = await httpClient.GetAsync($"/api/media?id={mediaId}&senderId={unknownSenderId}");

        // Assert - no bytes are served and the status is left untouched
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

        var client = await Db.Client.AsNoTracking().Include(c => c.Media)
            .Where(c => c.Id == testClient.Id).FirstOrDefaultAsync();
        client!.Media.First(m => m.MediaId == mediaId).Status.Should().Be(Domain.Enums.MediaStatus.Pending);
    }

    [Fact]
    public async Task DownloadMedia_Should_Return404_WhenMediaNotRegisteredForSender()
    {
        // Arrange
        var testClient = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());
        Db.Client.Add(testClient);
        await Db.SaveChangesAsync();

        var httpClient = CreateClient(testClient.Id.ToString());

        // Act - client exists and senderId matches its ConnectionId, but no media with this id is registered
        var response = await httpClient.GetAsync($"/api/media?id=unregistered-media-id&senderId={testClient.ConnectionId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    private async Task UploadTestMediaAsync(
        string mediaId,
        Domain.Aggregates.Client client,
        Guid senderId,
        string content = "Test media content")
    {
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        await _mediaService.StoreAsync(mediaId, stream, CancellationToken.None);

        var media = new Domain.Entities.Media(mediaId, client.Id, senderId);
        client.AddMedia(media);
        await Db.SaveChangesAsync();
    }
}

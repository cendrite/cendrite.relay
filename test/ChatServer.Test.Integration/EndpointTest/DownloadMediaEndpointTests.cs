using ChatServer.Application.Contracts.Services;
using ChatServer.Infrastructure.Persistence.Context;
using ChatServer.Test.Integration.Setup;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChatServer.Test.Integration.EndpointTest;

public class DownloadMediaEndpointTest(
    IntegrationTestFactory<Program, ChatServerDbContext> factory) 
    : IntegrationTestbase(factory)
{
    private readonly IMediaService _mediaService = factory.Services.CreateScope().ServiceProvider.GetRequiredService<IMediaService>();

    [Fact]
    public async Task DownloadMedia_Should_ReturnFileStream_WhenBlobExists()
    {
        // Arrange
        const string mediaId = "test-media-id";
        await StoreBlobAsync(mediaId, "Test media content");

        var httpClient = CreateClient(Guid.NewGuid().ToString());

        // Act
        var response = await httpClient.GetAsync($"/api/media?id={mediaId}");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/octet-stream");
        var responseContent = await response.Content.ReadAsByteArrayAsync();
        responseContent.Should().BeEquivalentTo(System.Text.Encoding.UTF8.GetBytes("Test media content"));
    }

    [Fact]
    public async Task DownloadMedia_Should_Return500_WhenBlobDoesNotExist()
    {
        // Arrange
        var httpClient = CreateClient(Guid.NewGuid().ToString());

        // Act - this media id was never stored in the blob store
        var response = await httpClient.GetAsync("/api/media?id=missing-media-id");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task DownloadMedia_Should_ReturnCorrectFile_WhenMultipleBlobsExist()
    {
        // Arrange
        await StoreBlobAsync("media-one", "content-one");
        await StoreBlobAsync("media-two", "content-two");

        var httpClient = CreateClient(Guid.NewGuid().ToString());

        // Act
        var response = await httpClient.GetAsync("/api/media?id=media-one");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsByteArrayAsync();
        responseContent.Should().BeEquivalentTo(System.Text.Encoding.UTF8.GetBytes("content-one"));
    }

    [Fact]
    public async Task DownloadMedia_Should_NotChangeMediaStatus_BecauseGetIsAPureRead()
    {
        // Arrange - a registered media in its default (Pending) state
        var testClient = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());
        Db.Client.Add(testClient);
        await Db.SaveChangesAsync();

        const string mediaId = "test-media-id";
        await StoreBlobAsync(mediaId, "Test media content");
        testClient.AddMedia(new Domain.Entities.Media(mediaId, testClient.Id, testClient.ConnectionId));
        await Db.SaveChangesAsync();

        var httpClient = CreateClient(testClient.Id.ToString());

        // Act - downloading twice must not flip the status; only the ack endpoint may do that
        await httpClient.GetAsync($"/api/media?id={mediaId}");
        var response = await httpClient.GetAsync($"/api/media?id={mediaId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var client = await Db.Client.AsNoTracking().Include(c => c.Media)
            .Where(c => c.Id == testClient.Id).FirstOrDefaultAsync();
        client!.Media.First(m => m.MediaId == mediaId).Status.Should().Be(Domain.Enums.MediaStatus.Pending);
    }

    private async Task StoreBlobAsync(string mediaId, string content)
    {
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        await _mediaService.StoreAsync(mediaId, stream, CancellationToken.None);
    }
}

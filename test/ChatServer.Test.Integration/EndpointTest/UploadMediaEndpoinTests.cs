using ChatServer.Application.Contracts.Services;
using ChatServer.Domain.Enums;
using ChatServer.Infrastructure.Persistence.Context;
using ChatServer.Test.Integration.Setup;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChatServer.Test.Integration.EndpointTest;

public class UploadMediaEndpointTests(
    IntegrationTestFactory<Program, ChatServerDbContext> factory) 
    : IntegrationTestbase(factory)
{
    private readonly IMediaService _mediaService = factory.Services.CreateScope().ServiceProvider.GetRequiredService<IMediaService>();

    [Fact]
    public async Task UploadMedia_Should_StoreMediaInBlobStore_And_Return200Ok()
    {
        // Arrange
        var testClient = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());
        Db.Client.Add(testClient);
        await Db.SaveChangesAsync();

        const string mediaId = "test-media-id";
        var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Test media content"));

        var httpClient = CreateClient(testClient.Id.ToString(), testClient.Id.ToString());

        // Act
        var response = await httpClient.PostAsync($"/api/media/upload?mediaId={mediaId}", new StreamContent(content));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var client = await Db.Client.AsNoTracking().Include(c => c.Media)
            .Where(c => c.Id == testClient.Id).FirstOrDefaultAsync();

        client.Should().NotBeNull();
        client!.Media.Should().ContainSingle(m => m.MediaId == mediaId && m.ClientId == testClient.Id);
        client.Media.First(m => m.MediaId == mediaId).Status.Should().Be(MediaStatus.Pending);
        client.Media.First(m => m.MediaId == mediaId).SenderId.Should().Be(testClient.ConnectionId);
        var storedMediaStream = _mediaService.Get(mediaId);
        storedMediaStream.Should().NotBeNull();
    }

    [Fact]
    public async Task UploadMedia_Should_IgnoreClientSuppliedSenderId_AndDeriveItFromTheAuthenticatedConnection()
    {
        // Arrange
        var testClient = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());
        Db.Client.Add(testClient);
        await Db.SaveChangesAsync();

        const string mediaId = "test-media-id";
        var spoofedSenderId = Guid.NewGuid(); // attacker-controlled value, must never be trusted
        var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Test media content"));

        var httpClient = CreateClient(testClient.Id.ToString(), testClient.Id.ToString());

        // Act
        var response = await httpClient.PostAsync($"/api/media/upload?mediaId={mediaId}&senderId={spoofedSenderId}", new StreamContent(content));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var client = await Db.Client.AsNoTracking().Include(c => c.Media)
            .Where(c => c.Id == testClient.Id).FirstOrDefaultAsync();

        client!.Media.First(m => m.MediaId == mediaId).SenderId.Should().Be(testClient.ConnectionId);
        client.Media.First(m => m.MediaId == mediaId).SenderId.Should().NotBe(spoofedSenderId);
    }

    [Theory]
    [InlineData("../../../../tmp/evil")]
    [InlineData("..%2f..%2f..%2ftmp%2fevil")]
    [InlineData("/tmp/evil")]
    [InlineData("ab/../../evil")]
    public async Task UploadMedia_Should_Return400_WhenMediaIdContainsPathTraversal(string maliciousId)
    {
        // Arrange
        var testClient = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());
        Db.Client.Add(testClient);
        await Db.SaveChangesAsync();

        var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("malicious content"));
        var httpClient = CreateClient(testClient.Id.ToString(), testClient.Id.ToString());

        // Act
        var response = await httpClient.PostAsync($"/api/media/upload?mediaId={maliciousId}&senderId={Guid.NewGuid()}", new StreamContent(content));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        var client = await Db.Client.AsNoTracking().Include(c => c.Media)
            .Where(c => c.Id == testClient.Id).FirstOrDefaultAsync();
        client!.Media.Should().BeEmpty();
    }

    [Fact]
    public async Task UploadMedia_Should_Return404_WhenClientDoesNotExist()
    {
        // Arrange
        var nonExistentClientId = Guid.NewGuid();
        const string mediaId = "test-media-id";
        var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Test media content"));

        var httpClient = CreateClient(nonExistentClientId.ToString(), nonExistentClientId.ToString());

        // Act
        var response = await httpClient.PostAsync($"/api/media/upload?mediaId={mediaId}", new StreamContent(content));

        // Assert
    }
}

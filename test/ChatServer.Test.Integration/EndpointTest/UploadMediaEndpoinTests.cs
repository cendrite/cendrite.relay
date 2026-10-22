using System.Net.Http.Json;
using System.Text.Json;
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
    public async Task UploadMedia_Should_StoreMediaInBlobStore_And_ReturnGeneratedMediaId()
    {
        // Arrange
        var testClient = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());
        Db.Client.Add(testClient);
        await Db.SaveChangesAsync();

        var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Test media content"));

        var httpClient = CreateClient(testClient.Id.ToString(), testClient.Id.ToString());

        // Act
        var response = await httpClient.PostAsync("/api/media/upload", new StreamContent(content));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var mediaId = await ReadMediaIdAsync(response);
        Guid.TryParse(mediaId, out _).Should().BeTrue();

        var client = await Db.Client.AsNoTracking().Include(c => c.Media)
            .Where(c => c.Id == testClient.Id).FirstOrDefaultAsync();

        client.Should().NotBeNull();
        client!.Media.Should().ContainSingle(m => m.MediaId == mediaId && m.ClientId == testClient.Id);
        client.Media.First(m => m.MediaId == mediaId).Status.Should().Be(MediaStatus.Pending);
        client.Media.First(m => m.MediaId == mediaId).SenderId.Should().Be(testClient.ConnectionId);
        await using var storedMediaStream = _mediaService.Get(mediaId);
        using var reader = new StreamReader(storedMediaStream);
        (await reader.ReadToEndAsync()).Should().Be("Test media content");
    }

    [Fact]
    public async Task UploadMedia_Should_IgnoreClientSuppliedSenderId_AndDeriveItFromTheAuthenticatedConnection()
    {
        // Arrange
        var testClient = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());
        Db.Client.Add(testClient);
        await Db.SaveChangesAsync();

        var spoofedSenderId = Guid.NewGuid();
        var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Test media content"));

        var httpClient = CreateClient(testClient.Id.ToString(), testClient.Id.ToString());

        // Act
        var response = await httpClient.PostAsync($"/api/media/upload?senderId={spoofedSenderId}", new StreamContent(content));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var mediaId = await ReadMediaIdAsync(response);

        var client = await Db.Client.AsNoTracking().Include(c => c.Media)
            .Where(c => c.Id == testClient.Id).FirstOrDefaultAsync();

        client!.Media.First(m => m.MediaId == mediaId).SenderId.Should().Be(testClient.ConnectionId);
        client.Media.First(m => m.MediaId == mediaId).SenderId.Should().NotBe(spoofedSenderId);
    }

    [Fact]
    public async Task UploadMedia_Should_IgnoreClientSuppliedMediaId_And_NotOverwriteExistingMedia()
    {
        // Arrange
        var owner = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());
        var attacker = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());
        Db.Client.AddRange(owner, attacker);
        await Db.SaveChangesAsync();

        var ownerClient = CreateClient(owner.Id.ToString(), owner.Id.ToString());
        var ownerResponse = await ownerClient.PostAsync("/api/media/upload",
            new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("original content"))));
        var existingMediaId = await ReadMediaIdAsync(ownerResponse);

        var attackerClient = CreateClient(attacker.Id.ToString(), attacker.Id.ToString());

        // Act
        var response = await attackerClient.PostAsync($"/api/media/upload?mediaId={existingMediaId}",
            new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("replaced content"))));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var mediaId = await ReadMediaIdAsync(response);
        mediaId.Should().NotBe(existingMediaId);

        await using var storedMediaStream = _mediaService.Get(existingMediaId);
        using var reader = new StreamReader(storedMediaStream);
        (await reader.ReadToEndAsync()).Should().Be("original content");
    }

    [Fact]
    public async Task UploadMedia_Should_Return404_WhenClientDoesNotExist()
    {
        // Arrange
        var nonExistentClientId = Guid.NewGuid();
        var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Test media content"));

        var httpClient = CreateClient(nonExistentClientId.ToString(), nonExistentClientId.ToString());

        // Act
        var response = await httpClient.PostAsync("/api/media/upload", new StreamContent(content));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    private static async Task<string> ReadMediaIdAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("mediaId").GetString()!;
    }
}

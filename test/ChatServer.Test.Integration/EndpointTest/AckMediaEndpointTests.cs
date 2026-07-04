using ChatServer.Infrastructure.Persistence.Context;
using ChatServer.Test.Integration.Setup;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Test.Integration.EndpointTest;

public class AckMediaEndpointTest(
    IntegrationTestFactory<Program, ChatServerDbContext> factory) 
    : IntegrationTestbase(factory)
{
    [Fact]
    public async Task AckMedia_Should_MarkMediaAsDownloaded_AndReturn200()
    {
        // Arrange
        var testClient = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());
        Db.Client.Add(testClient);
        await Db.SaveChangesAsync();

        const string mediaId = "test-media-id";
        await RegisterMediaAsync(mediaId, testClient, testClient.ConnectionId);

        var httpClient = CreateClient(testClient.Id.ToString());

        // Act
        var response = await httpClient.PostAsync($"/api/media/{mediaId}/ack?senderId={testClient.ConnectionId}", null);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var client = await Db.Client.AsNoTracking().Include(c => c.Media)
            .Where(c => c.Id == testClient.Id).FirstOrDefaultAsync();
        client!.Media.First(m => m.MediaId == mediaId).Status.Should().Be(Domain.Enums.MediaStatus.Downloaded);
    }

    [Fact]
    public async Task AckMedia_Should_Return404_AndLeaveStatusPending_WhenSenderIdMatchesNoClient()
    {
        // Arrange
        var testClient = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());
        Db.Client.Add(testClient);
        await Db.SaveChangesAsync();

        const string mediaId = "test-media-id";
        await RegisterMediaAsync(mediaId, testClient, testClient.ConnectionId);

        var httpClient = CreateClient(testClient.Id.ToString());
        var unknownSenderId = Guid.NewGuid(); // matches no client's ConnectionId

        // Act
        var response = await httpClient.PostAsync($"/api/media/{mediaId}/ack?senderId={unknownSenderId}", null);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

        var client = await Db.Client.AsNoTracking().Include(c => c.Media)
            .Where(c => c.Id == testClient.Id).FirstOrDefaultAsync();
        client!.Media.First(m => m.MediaId == mediaId).Status.Should().Be(Domain.Enums.MediaStatus.Pending);
    }

    [Fact]
    public async Task AckMedia_Should_Return404_WhenMediaNotRegisteredForSender()
    {
        // Arrange
        var testClient = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());
        Db.Client.Add(testClient);
        await Db.SaveChangesAsync();

        var httpClient = CreateClient(testClient.Id.ToString());

        // Act - client exists and senderId matches its ConnectionId, but no media with this id is registered
        var response = await httpClient.PostAsync($"/api/media/unregistered-media-id/ack?senderId={testClient.ConnectionId}", null);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AckMedia_Should_BeIdempotent_RemainingDownloaded_WhenAckedTwice()
    {
        // Arrange
        var testClient = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());
        Db.Client.Add(testClient);
        await Db.SaveChangesAsync();

        const string mediaId = "test-media-id";
        await RegisterMediaAsync(mediaId, testClient, testClient.ConnectionId);

        var httpClient = CreateClient(testClient.Id.ToString());

        // Act
        var first = await httpClient.PostAsync($"/api/media/{mediaId}/ack?senderId={testClient.ConnectionId}", null);
        var second = await httpClient.PostAsync($"/api/media/{mediaId}/ack?senderId={testClient.ConnectionId}", null);

        // Assert
        first.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        second.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var client = await Db.Client.AsNoTracking().Include(c => c.Media)
            .Where(c => c.Id == testClient.Id).FirstOrDefaultAsync();
        client!.Media.First(m => m.MediaId == mediaId).Status.Should().Be(Domain.Enums.MediaStatus.Downloaded);
    }

    private async Task RegisterMediaAsync(string mediaId, Domain.Aggregates.Client client, Guid senderId)
    {
        client.AddMedia(new Domain.Entities.Media(mediaId, client.Id, senderId));
        await Db.SaveChangesAsync();
    }
}

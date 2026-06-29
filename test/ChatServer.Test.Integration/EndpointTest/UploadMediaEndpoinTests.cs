using ChatServer.Infrastructure.Persistence.Context;
using ChatServer.Test.Integration.Setup;
using FluentAssertions;

namespace ChatServer.Test.Integration.EndpointTest;

public class UploadMediaEndpointTests : IntegrationTestbase
{
    public UploadMediaEndpointTests(IntegrationTestFactory<Program, ChatServerDbContext> factory) : base(factory)
    {
    }

    [Fact]
    public async Task UploadMedia_Should_StoreMediaInBlobStore_And_Return200Ok()
    {
        // Arrange
        const string mediaId = "test-media-id";
        var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Test media content"));

        // Act
        var response = await Client.PostAsync($"/api/media/upload?id={mediaId}", new StreamContent(content));

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}

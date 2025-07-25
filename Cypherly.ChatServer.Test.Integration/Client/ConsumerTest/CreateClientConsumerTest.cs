using AutoFixture;
using Cypherly.ChatServer.Application.Contracts.Repositories;
using Cypherly.ChatServer.Application.Features.Client.Consumers;
using Cypherly.ChatServer.Infrastructure.Persistence.Context;
using Cypherly.ChatServer.Test.Integration.Setup;
using Cypherly.Message.Contracts.Messages.Client;
using FakeItEasy;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cypherly.ChatServer.Test.Integration.Client.ConsumerTest;

public class CreateClientConsumerTest : IntegrationTestbase
{
    private readonly CreateClientConsumer _sut;
    private readonly Fixture _fixture = new();
    public CreateClientConsumerTest(IntegrationTestFactory<Program, ChatServerDbContext> factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        var clientRepository = scope.ServiceProvider.GetRequiredService<IClientRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CreateClientConsumer>>();

        _sut = new CreateClientConsumer(clientRepository, unitOfWork, logger);
    }


    [Fact]
    public async void Consume_ValidRequest_Should_Create_Client()
    {
        // Arrange
        var request = _fixture.Create<CreateClientMessage>();

        var fakeConsumeContext = A.Fake<ConsumeContext<CreateClientMessage>>();
        A.CallTo(() => fakeConsumeContext.Message).Returns(request);

        // Act
        await _sut.Consume(fakeConsumeContext);

        // Assert
        Db.Client.Count().Should().Be(1);
        Db.Client.First().Id.Should().Be(request.DeviceId);
        Db.Client.First().ConnectionId.Should().Be(request.ConnectionId);
    }
}
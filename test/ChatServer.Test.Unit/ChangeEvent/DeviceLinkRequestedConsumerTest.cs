using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Contracts.Services;
using ChatServer.Application.Features.ChangeEvent.Handlers;
using ChatServer.Application.Features.ChangeEvent.Services;
using ChatServer.Application.Abstractions;
using ChatServer.Domain.Aggregates;
using Cendrite.Message.Contracts.Messages.Device;
using FakeItEasy;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Test.Unit.ChangeEvent;

public sealed class DeviceLinkRequestedConsumerTest
{
    [Fact]
    public async Task Consume_Should_Only_Notify_The_Issuing_Device()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var issuing = new Client(Guid.NewGuid(), Guid.NewGuid(), userId);
        var repository = A.Fake<IClientRepository>();
        A.CallTo(() => repository.GetAsync(A<ISpecification<Client>>._, A<CancellationToken>._)).Returns(issuing);

        var sut = new DeviceLinkRequestedConsumer(new ChangeEventDispatcher(A.Fake<IChangeEventNotifier>()), repository, A.Fake<IUnitOfWork>(),
            A.Fake<ILogger<DeviceLinkRequestedConsumer>>());

        var context = A.Fake<ConsumeContext<DeviceLinkRequestedMessage>>();
        A.CallTo(() => context.Message).Returns(Message(userId, issuing));

        // Act
        await sut.Consume(context);

        // Assert
        Assert.Single(issuing.Events);
    }

    [Fact]
    public async Task Consume_Should_Ignore_A_Connection_Of_Another_User()
    {
        // Arrange
        var stranger = new Client(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var repository = A.Fake<IClientRepository>();
        A.CallTo(() => repository.GetAsync(A<ISpecification<Client>>._, A<CancellationToken>._)).Returns(stranger);

        var sut = new DeviceLinkRequestedConsumer(new ChangeEventDispatcher(A.Fake<IChangeEventNotifier>()), repository, A.Fake<IUnitOfWork>(),
            A.Fake<ILogger<DeviceLinkRequestedConsumer>>());

        var context = A.Fake<ConsumeContext<DeviceLinkRequestedMessage>>();
        A.CallTo(() => context.Message).Returns(Message(Guid.NewGuid(), stranger));

        // Act
        await sut.Consume(context);

        // Assert
        Assert.Empty(stranger.Events);
    }

    private static DeviceLinkRequestedMessage Message(Guid userId, Client issuing) => new()
    {
        CorrelationId = Guid.NewGuid(),
        UserId = userId,
        IssuingDeviceId = issuing.Id,
        IssuingConnectionId = issuing.ConnectionId,
        PendingLinkId = Guid.NewGuid(),
        DevicePublicKey = "key",
        DevicePlatform = "Linux",
    };
}

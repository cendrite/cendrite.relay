using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Contracts.Services;
using ChatServer.Application.Features.ChangeEvent.Handlers;
using ChatServer.Application.Features.ChangeEvent.Services;
using ChatServer.Domain.Aggregates;
using Cendrite.Message.Contracts.Messages.Device;
using FakeItEasy;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Test.Unit.ChangeEvent;

public sealed class DeviceAddedConsumerTest
{
    [Fact]
    public async Task Consume_Should_Tell_The_Users_Other_Devices_But_Not_The_New_One()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newDevice = new Client(Guid.NewGuid(), Guid.NewGuid(), userId);
        var existing = new Client(Guid.NewGuid(), Guid.NewGuid(), userId);

        var repository = A.Fake<IClientRepository>();
        A.CallTo(() => repository.GetActiveByUserIdAsync(userId, A<CancellationToken>._)).Returns([newDevice, existing]);
        var unitOfWork = A.Fake<IUnitOfWork>();

        var sut = new DeviceAddedConsumer(new ChangeEventDispatcher(A.Fake<IChangeEventNotifier>()), repository, unitOfWork,
            A.Fake<ILogger<DeviceAddedConsumer>>());

        var context = A.Fake<ConsumeContext<DeviceAddedMessage>>();
        A.CallTo(() => context.Message).Returns(new DeviceAddedMessage
        {
            CorrelationId = Guid.NewGuid(),
            UserId = userId,
            DeviceId = newDevice.Id,
            ConnectionId = newDevice.ConnectionId,
            DevicePlatform = "Linux",
        });

        // Act
        await sut.Consume(context);

        // Assert
        Assert.Contains($"\"deviceId\":\"{newDevice.Id}\"", Assert.Single(existing.Events).Data);
        Assert.Empty(newDevice.Events);
        A.CallTo(() => unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}

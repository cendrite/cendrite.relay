using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Contracts.Services;
using ChatServer.Application.Features.ChangeEvent.Handlers;
using ChatServer.Application.Features.ChangeEvent.Services;
using ChatServer.Domain.Aggregates;
using Cendrite.Message.Contracts.Messages.User;
using FakeItEasy;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Test.Unit.ChangeEvent;

public sealed class AccountKeyRotatedConsumerTest
{
    [Fact]
    public async Task Consume_Should_Tell_The_Users_Other_Devices_But_Not_The_Rotating_One()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var rotating = new Client(Guid.NewGuid(), Guid.NewGuid(), userId);
        var other = new Client(Guid.NewGuid(), Guid.NewGuid(), userId);

        var repository = A.Fake<IClientRepository>();
        A.CallTo(() => repository.GetActiveByUserIdAsync(userId, A<CancellationToken>._)).Returns([rotating, other]);
        var unitOfWork = A.Fake<IUnitOfWork>();

        var sut = new AccountKeyRotatedConsumer(new ChangeEventDispatcher(A.Fake<IChangeEventNotifier>()), repository, unitOfWork,
            A.Fake<ILogger<AccountKeyRotatedConsumer>>());

        var context = A.Fake<ConsumeContext<AccountKeyRotatedMessage>>();
        A.CallTo(() => context.Message).Returns(new AccountKeyRotatedMessage
        {
            CorrelationId = Guid.NewGuid(),
            UserId = userId,
            DeviceId = rotating.Id,
            ConnectionId = rotating.ConnectionId,
            DeviceName = "Work laptop",
        });

        // Act
        await sut.Consume(context);

        // Assert
        Assert.Single(other.Events);
        Assert.Empty(rotating.Events);
        A.CallTo(() => unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}

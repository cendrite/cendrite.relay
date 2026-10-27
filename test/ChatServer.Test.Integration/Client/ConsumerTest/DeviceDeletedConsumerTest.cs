using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Features.ChangeEvent.Handlers;
using ChatServer.Application.Features.ChangeEvent.Services;
using ChatServer.Infrastructure.Persistence.Context;
using ChatServer.Test.Integration.Setup;
using Cypherly.Message.Contracts.Messages.Device;
using FakeItEasy;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChatServer.Test.Integration.Client.ConsumerTest;

public class DeviceDeletedConsumerTest : IntegrationTestbase
{
    private readonly DeviceDeletedConsumer _sut;

    public DeviceDeletedConsumerTest(IntegrationTestFactory<Program, ChatServerDbContext> factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<ChangeEventDispatcher>();
        var clientRepository = scope.ServiceProvider.GetRequiredService<IClientRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DeviceDeletedConsumer>>();

        _sut = new DeviceDeletedConsumer(dispatcher, clientRepository, unitOfWork, logger);
    }

    [Fact]
    public async Task Consume_Given_Offline_Client_Should_Hold_Deleted_Event_And_Keep_Client()
    {
        // Arrange
        var client = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        await Db.Client.AddAsync(client);
        await Db.SaveChangesAsync();

        var fakeConsumeContext = A.Fake<ConsumeContext<DeviceDeletedMessage>>();
        A.CallTo(() => fakeConsumeContext.Message).Returns(new DeviceDeletedMessage
        {
            CorrelationId = Guid.NewGuid(),
            DeviceId = client.Id,
        });

        // Act
        await _sut.Consume(fakeConsumeContext);

        // Assert
        Db.Client.AsNoTracking().Any(c => c.Id == client.Id).Should().BeTrue();
        Db.Event.AsNoTracking().Any(e => e.RecipientId == client.ConnectionId).Should().BeTrue();
    }

    [Fact]
    public async Task Consume_Given_Missing_Client_Should_Not_Throw()
    {
        // Arrange
        var fakeConsumeContext = A.Fake<ConsumeContext<DeviceDeletedMessage>>();
        A.CallTo(() => fakeConsumeContext.Message).Returns(new DeviceDeletedMessage
        {
            CorrelationId = Guid.NewGuid(),
            DeviceId = Guid.NewGuid(),
        });

        // Act
        var act = async () => await _sut.Consume(fakeConsumeContext);

        // Assert
        await act.Should().NotThrowAsync();
    }
}

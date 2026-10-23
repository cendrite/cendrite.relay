using AutoFixture;
using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Contracts.Services;
using ChatServer.Application.Enums;
using ChatServer.Application.Features.ChangeEvent.Handlers;
using ChatServer.Application.Features.ChangeEvent.Services;
using ChatServer.Domain.Aggregates;
using ChatServer.Domain.Entities;
using Cypherly.Message.Contracts.Messages.Device;
using Cypherly.Message.Contracts.Messages.User;
using Cypherly.Message.Contracts.Responses.Device;
using FakeItEasy;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Test.Unit.ChangeEvent;

public sealed class ClientLogoutConsumerTest
{
    private readonly Fixture _fixture = new();
    private readonly IChangeEventNotifier _fakeChangeEventNotifier;
    private readonly IRequestClient<ConnectionIdsProxyMessage> _fakeConnectionIdsClient;
    private readonly IClientRepository _fakeClientRepository;
    private readonly IUnitOfWork _fakeUnitOfWork;

    private readonly ClientLogoutConsumer _sut;

    public ClientLogoutConsumerTest()
    {
        _fakeChangeEventNotifier = A.Fake<IChangeEventNotifier>();
        _fakeConnectionIdsClient = A.Fake<IRequestClient<ConnectionIdsProxyMessage>>();
        _fakeClientRepository = A.Fake<IClientRepository>();
        _fakeUnitOfWork = A.Fake<IUnitOfWork>();
        var fakeLogger = A.Fake<ILogger<ClientLogoutConsumer>>();

        _sut = new ClientLogoutConsumer(
                new ChangeEventDispatcher(_fakeChangeEventNotifier),
                _fakeClientRepository,
                _fakeUnitOfWork,
                _fakeConnectionIdsClient,
                fakeLogger);
    }

    [Fact]
    public async Task Consume_Given_Valid_ConnectedClient_Should_NotifyClient()
    {
        // Arrange
        var client = new Client(Guid.NewGuid(), Guid.NewGuid());
        var userLogoutMessage = _fixture.Build<UserLogoutMessage>()
            .With(c => c.DeviceId, client.Id)
            .Create();

        var connectionIdsResponse = new ConnectionIdsProxyResponse 
        {
            CorrelationId = userLogoutMessage.CorrelationId,
            IsSuccess = true,
            ConnectionIds = new Dictionary<Guid, List<Guid>>
            {
                { userLogoutMessage.UserId, new List<Guid> { client.ConnectionId } }
            },
            UserTag = "someTag"
        };

        var responseMessage = A.Fake<Response<ConnectionIdsProxyResponse>>();
        A.CallTo(() => responseMessage.Message).Returns(connectionIdsResponse);

        A.CallTo(() => _fakeConnectionIdsClient.GetResponse<ConnectionIdsProxyResponse>(A<ConnectionIdsProxyMessage>._, A<CancellationToken>._, A<RequestTimeout>._))
            .Returns(Task.FromResult(responseMessage));

        client.AddConnection(new Connection("transientId", client.Id, HubType.ChangeEvent.ToString()));

        A.CallTo(() => _fakeClientRepository.GetAsync(A<ISpecification<Client>>._, A<CancellationToken>._))
            .Returns(client);

        var fakeConsumeContext = A.Fake<ConsumeContext<UserLogoutMessage>>();
        A.CallTo(() => fakeConsumeContext.Message).Returns(userLogoutMessage);

        // Act
        await _sut.Consume(fakeConsumeContext);

        // Assert
        A.CallTo(() => _fakeChangeEventNotifier.NotifyAsync("transientId",
                A<Application.Features.ChangeEvent.Base.ChangeEvent>._,
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _fakeConnectionIdsClient.GetResponse<ConnectionIdsProxyResponse>(
                A<ConnectionIdsProxyMessage>.That.Matches(msg =>
                    msg.CorrelationId == userLogoutMessage.CorrelationId &&
                    msg.CausationId == userLogoutMessage.Id &&
                    msg.TenantId == userLogoutMessage.UserId),
                A<CancellationToken>._,
                A<RequestTimeout>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Consume_Given_No_Clients_Should_Not_Notify()
    {
        // Arrange
        var userLogoutMessage = _fixture.Create<UserLogoutMessage>();
        var fakeConsumeContext = A.Fake<ConsumeContext<UserLogoutMessage>>();
        A.CallTo(() => fakeConsumeContext.Message).Returns(userLogoutMessage);
        var connectionIdsResponse = new ConnectionIdsProxyResponse 
        {
            CorrelationId = userLogoutMessage.CorrelationId,
            IsSuccess = true,
            ConnectionIds = [],
            UserTag = "someTag"
        };

        var responseMessage = A.Fake<Response<ConnectionIdsProxyResponse>>();
        A.CallTo(() => responseMessage.Message).Returns(connectionIdsResponse);
        A.CallTo(() => _fakeConnectionIdsClient.GetResponse<ConnectionIdsProxyResponse>(A<ConnectionIdsProxyMessage>._, A<CancellationToken>._, A<RequestTimeout>._))
            .Returns(Task.FromResult(responseMessage));

        // Act
        await _sut.Consume(fakeConsumeContext);

        // Assert
        A.CallTo(() => _fakeChangeEventNotifier.NotifyAsync(A<string>._, A<Application.Features.ChangeEvent.Base.ChangeEvent>._, A<CancellationToken>._))
            .MustNotHaveHappened(); 

        A.CallTo(() => _fakeConnectionIdsClient.GetResponse<ConnectionIdsProxyResponse>(
                A<ConnectionIdsProxyMessage>.That.Matches(msg =>
                    msg.CorrelationId == userLogoutMessage.CorrelationId &&
                    msg.CausationId == userLogoutMessage.Id &&
                    msg.TenantId == userLogoutMessage.UserId),
                A<CancellationToken>._,
                A<RequestTimeout>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Consume_Given_FailedToRetrieveConnectionIds_Should_LogErrorAndThrow()
    {
        // Arrange
        var userLogoutMessage = _fixture.Create<UserLogoutMessage>();
        var fakeConsumeContext = A.Fake<ConsumeContext<UserLogoutMessage>>();
        A.CallTo(() => fakeConsumeContext.Message).Returns(userLogoutMessage);

        var connectionIdsResponse = new ConnectionIdsProxyResponse 
        {
            CorrelationId = userLogoutMessage.CorrelationId,
            IsSuccess = false,
            Error = "Some error",
            UserTag = "someTag"
        };

        var responseMessage = A.Fake<Response<ConnectionIdsProxyResponse>>();
        A.CallTo(() => responseMessage.Message).Returns(connectionIdsResponse);
        A.CallTo(() => _fakeConnectionIdsClient.GetResponse<ConnectionIdsProxyResponse>(A<ConnectionIdsProxyMessage>._, A<CancellationToken>._, A<RequestTimeout>._))
            .Returns(Task.FromResult(responseMessage)); 

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.Consume(fakeConsumeContext));

        A.CallTo(() => _fakeChangeEventNotifier.NotifyAsync(A<string>._, A<Application.Features.ChangeEvent.Base.ChangeEvent>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => _fakeConnectionIdsClient.GetResponse<ConnectionIdsProxyResponse>(
                A<ConnectionIdsProxyMessage>.That.Matches(msg =>
                    msg.CorrelationId == userLogoutMessage.CorrelationId &&
                    msg.CausationId == userLogoutMessage.Id &&
                    msg.TenantId == userLogoutMessage.UserId),
                A<CancellationToken>._,
                A<RequestTimeout>._))
            .MustHaveHappenedOnceExactly();
    }
}

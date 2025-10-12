using AutoFixture;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Features.Client.Consumers;
using ChatServer.Domain.Aggregates;
using Cypherly.Message.Contracts.Messages.Client;
using Cypherly.Message.Contracts.Responses.Client;
using FakeItEasy;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Test.Unit.ConsumerTest;

public class CreateClientConsumerTest
{
    private readonly IClientRepository _fakeRepository;
    private readonly IUnitOfWork _fakeUnitOfWork;
    private readonly Fixture _fixture = new();

    private readonly CreateClientConsumer _sut;

    public CreateClientConsumerTest()
    {
        _fakeRepository = A.Fake<IClientRepository>();
        _fakeUnitOfWork = A.Fake<IUnitOfWork>();
        var fakeLogger = A.Fake<ILogger<CreateClientConsumer>>();
        _sut = new CreateClientConsumer(_fakeRepository, _fakeUnitOfWork, fakeLogger);
    }

    [Fact]
    public async Task Consume_WhenCalled_ShouldCreateClient()
    {
        // Arrange
        var fakeContext = A.Fake<ConsumeContext<CreateClientMessage>>();
        var fakeMessage = _fixture.Create<CreateClientMessage>();
        A.CallTo(() => fakeContext.Message).Returns(fakeMessage);

        // Act
        await _sut.Consume(fakeContext);

        // Assert
        A.CallTo(() => _fakeRepository.CreateAsync(A<Client>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(CancellationToken.None)).MustHaveHappenedOnceExactly();
        A.CallTo(() => fakeContext.RespondAsync(A<CreateClientResponse>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Consume_WhenExceptionThrown_ShouldLogErrorAndRespondWithError()
    {
        // Arrange
        var fakeContext = A.Fake<ConsumeContext<CreateClientMessage>>();
        var fakeMessage = _fixture.Create<CreateClientMessage>();
        A.CallTo(() => fakeContext.Message).Returns(fakeMessage);
        A.CallTo(() => _fakeRepository.CreateAsync(A<Client>._)).Throws<Exception>();

        // Act
        await _sut.Consume(fakeContext);

        // Assert
        A.CallTo(() => _fakeRepository.CreateAsync(A<Client>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(CancellationToken.None)).MustNotHaveHappened();
        A.CallTo(() => fakeContext.RespondAsync(A<CreateClientResponse>._)).MustHaveHappenedOnceExactly();
    }
}

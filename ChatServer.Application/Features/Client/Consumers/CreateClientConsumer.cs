using ChatServer.Application.Contracts.Repositories;
using ChatServer.Domain.Common;
using Cypherly.Message.Contracts.Messages.Client;
using Cypherly.Message.Contracts.Responses.Client;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.Client.Consumers;

public class CreateClientConsumer(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateClientConsumer> logger)
    : IConsumer<CreateClientMessage>
{
    public async Task Consume(ConsumeContext<CreateClientMessage> context)
    {
        var message = context.Message;
        
        try
        {

            var client = new Domain.Aggregates.Client(message.DeviceId, message.ConnectionId);

            await clientRepository.CreateAsync(client);

            await unitOfWork.SaveChangesAsync();

            await context.RespondAsync(new CreateClientResponse
            {
                CausationId = message.CorrelationId,
                CorrelationId = message.CorrelationId,
                Error = null,
                IsSuccess = true
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An Exception occured while attempting to create a client");
            
            await context.RespondAsync(new CreateClientResponse
            {
                CausationId = message.CorrelationId,
                CorrelationId = message.CorrelationId,
                Error = Errors.General.UnspecifiedError("An exception occured while attempting to create a client").Message,
                IsSuccess = false
            });
        }
    }
}
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
    public async Task Consume(ConsumeContext<CreateClientMessage> ctx)
    {
        try
        {
            var client = new Domain.Aggregates.Client(ctx.Message.DeviceId, ctx.Message.ConnectionId, ctx.Message.UserId);

            await clientRepository.CreateAsync(client);

            await unitOfWork.SaveChangesAsync();

            await ctx.RespondAsync(new CreateClientResponse
            {
                CausationId = ctx.Message.CorrelationId,
                CorrelationId = ctx.Message.CorrelationId,
                Error = null,
                IsSuccess = true
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An Exception occured while attempting to create a client");
            
            await ctx.RespondAsync(new CreateClientResponse
            {
                CausationId = ctx.Message.CorrelationId,
                CorrelationId = ctx.Message.CorrelationId,
                Error = Error.Failure("An exception occured while attempting to create a client").Description,
                IsSuccess = false
            });
        }
    }
}

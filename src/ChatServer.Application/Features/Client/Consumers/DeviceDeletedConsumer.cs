using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Specifications.Client;
using Cypherly.Message.Contracts.Messages.Device;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.Client.Consumers;

public sealed class DeviceDeletedConsumer(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeviceDeletedConsumer> logger)
    : IConsumer<DeviceDeletedMessage>
{
    public async Task Consume(ConsumeContext<DeviceDeletedMessage> context)
    {
        try
        {
            var client = await clientRepository.GetAsync(new ClientByIdSpec(context.Message.DeviceId), context.CancellationToken);
            if (client is null)
            {
                logger.LogWarning("No client found for device id: {DeviceId}", context.Message.DeviceId);
                return;
            }

            clientRepository.Delete(client);
            await unitOfWork.SaveChangesAsync(context.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming DeviceDeletedMessage: {Message}", context.Message);
            throw;
        }
    }
}

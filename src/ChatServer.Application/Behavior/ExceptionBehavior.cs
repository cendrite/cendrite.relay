using ChatServer.Domain.Common;
using Mediator;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Behavior;

public class ExceptionBehavior<TRequest, TResponse>(
    ILogger<ExceptionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IMessage
    where TResponse : Result
{
    public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(request, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred while processing request of type {RequestType}", typeof(TRequest).Name);

            var error = Error.Failure("An unexpected error occurred: " + ex.Message);
            return ResultFactory.Fail<TResponse>(error);
        }
    }
}

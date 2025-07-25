using System.Reflection;
using ChatServer.Infrastructure.Messaging;
using ChatServer.Infrastructure.Settings;
using Cypherly.Message.Contracts.Abstractions;
using Cypherly.Message.Contracts.Messages.Client;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ChatServer.Infrastructure.Extensions;

internal static class MassTransitExtensions
{
    /// <summary>
    /// Configures Masstransit with RabbitMQ, including retry and circut breaker policies.
    /// Optionally allows configuring sagas or MassTransit components.
    /// Not providing a RabbitMqConfig will default to using the default endpoint configuration/>
    /// </summary>
    /// <param name="services">The service collection to add Masstransit to</param>
    /// <param name="consumerAssembly">The assembly containing consumers to register</param>
    /// <param name="masstransitConfig">Optional configuration for adding sagas or additional MassTransit Components</param>
    /// <param name="rabbitMqConfig">Additional RabbitMq Extensions(Endpoints, consumers[...])</param>
    /// <returns>The ServiceCollection <see cref="ServiceCollection"/></returns>
    /// <exception cref="InvalidOperationException">Exception thrown if RabbitMq <see cref="RabbitMqSettings"/> settings aren't configured; resulting in missing values for connection</exception>
    internal static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services,
        Assembly consumerAssembly, Action<IBusRegistrationConfigurator>? masstransitConfig = null,
        Action<IRabbitMqBusFactoryConfigurator, IBusRegistrationContext>? rabbitMqConfig = null)
    {
        services.AddMassTransit(x =>
        {
            masstransitConfig?.Invoke(x);

            x.AddConsumers(consumerAssembly);

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqSettings = context.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

                cfg.Host(rabbitMqSettings.Host, "/", h =>
                {
                    h.Username(rabbitMqSettings.Username ??
                               throw new InvalidOperationException("Cannot initialize RabbitMQ without a username"));
                    h.Password(rabbitMqSettings.Password ??
                               throw new InvalidOperationException("Cannot initialize RabbitMQ without a password"));
                });


                cfg.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(5)));

                cfg.UseCircuitBreaker(cb =>
                {
                    cb.TrackingPeriod = TimeSpan.FromMinutes(1);
                    cb.TripThreshold = 15;
                    cb.ActiveThreshold = 10;
                    cb.ResetInterval = TimeSpan.FromMinutes(5);
                });

                if (rabbitMqConfig != null)
                {
                    rabbitMqConfig.Invoke(cfg, context);
                }
                else
                {
                    cfg.ConfigureEndpoints(context);
                }
            });
        });

        services.AddProducer<ClientConnectedMessage>();

        return services;
    }

    /// <summary>
    /// Add a producer for a specific message type to the service collection
    /// </summary>
    /// <param name="services">the collection producer will be added to</param>
    /// <typeparam name="TMessage">the type the producer will handle. TMessage type should be of type <see cref="BaseMessage"/></typeparam>
    /// <returns><see cref="IServiceCollection"/></returns>
    private static void AddProducer<TMessage>(this IServiceCollection services)
        where TMessage : IBaseMessage
    {
        services.AddScoped<IProducer<TMessage>, Producer<TMessage>>();
    }
}
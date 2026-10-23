using System.Reflection;
using ChatServer.Application.Behavior;
using ChatServer.Application.Features.ChangeEvent.Services;
using FluentValidation;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace ChatServer.Application.Extensions;

public static class ApplicationExtensions
{
    public static void AddApplication(this IServiceCollection services, Assembly assembly)
    {
        services.AddValidatorsFromAssembly(assembly);
        services.AddScoped<ChangeEventDispatcher>();
        // Options are read by the source generator at compile time, so they must stay inline constants
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors = [typeof(ValidationBehavior<,>), typeof(ExceptionBehavior<,>)];
        });
    }
}

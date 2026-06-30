using FluentValidation;

namespace ChatServer.Application.Features.Client.Commands.Connect;

public class ConnectClientCommandValidator : AbstractValidator<ConnectClientCommand>
{
    public ConnectClientCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage($"The value cannot be empty: {nameof(ConnectClientCommand.ClientId)} ");

        RuleFor(x => x.TransientId)
            .NotEmpty()
            .WithMessage($"The value cannot be empty: {nameof(ConnectClientCommand.TransientId)} ");
    }
}
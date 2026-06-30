using FluentValidation;

namespace ChatServer.Application.Features.Client.Commands.Disconnect;

public class DisconnectCommandValidator : AbstractValidator<DisconnectClientCommand>
{
    public DisconnectCommandValidator()
    {
        RuleFor(x => x.TransientId)
            .NotEmpty()
            .WithMessage($"Value '{nameof(DisconnectClientCommand.TransientId)}' is required.");
    }
}
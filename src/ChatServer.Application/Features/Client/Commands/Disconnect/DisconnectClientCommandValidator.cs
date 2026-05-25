using ChatServer.Domain.Common;
using FluentValidation;

namespace ChatServer.Application.Features.Client.Commands.Disconnect;

public class DisconnectCommandValidator : AbstractValidator<DisconnectClientCommand>
{
    public DisconnectCommandValidator()
    {
        RuleFor(x => x.TransientId)
            .NotEmpty()
            .WithMessage(Errors.General.ValueIsRequired(nameof(DisconnectClientCommand.TransientId)).Message);
    }
}
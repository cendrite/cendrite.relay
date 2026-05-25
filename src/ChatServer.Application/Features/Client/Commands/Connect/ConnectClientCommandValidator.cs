using ChatServer.Domain.Common;
using FluentValidation;

namespace ChatServer.Application.Features.Client.Commands.Connect;

public class ConnectClientCommandValidator : AbstractValidator<ConnectClientCommand>
{
    public ConnectClientCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage(Errors.General.ValueIsEmpty(nameof(ConnectClientCommand.ClientId)).Message);

        RuleFor(x => x.TransientId)
            .NotEmpty()
            .WithMessage(Errors.General.ValueIsEmpty(nameof(ConnectClientCommand.TransientId)).Message);
    }
}
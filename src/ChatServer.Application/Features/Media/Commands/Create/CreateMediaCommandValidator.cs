using FluentValidation;

namespace ChatServer.Application.Features.Media.Commands.Create;

public sealed class CreateMediaCommandValidator : AbstractValidator<CreateMediaCommand>
{
    public CreateMediaCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty();

        RuleFor(x => x.MediaId)
            .NotEmpty();
    }
}

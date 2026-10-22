using FluentValidation;

namespace ChatServer.Application.Features.Media.Commands.Create;

public sealed class CreateMediaCommandValidator : AbstractValidator<CreateMediaCommand>
{
    public CreateMediaCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty();

        RuleFor(x => x.Size)
            .GreaterThan(0)
            .LessThanOrEqualTo(100 * 1024 * 1024);
    }
}

using FluentValidation;

namespace ChatServer.Application.Features.Media.Commands.Update;

public sealed class UpdateMediaCommandValidator : AbstractValidator<UpdateMediaCommand>
{
    public UpdateMediaCommandValidator()
    {
        RuleFor(x => x.MediaId)
            .NotEmpty().WithMessage("MediaId is required.");
    }
}

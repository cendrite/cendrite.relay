using FluentValidation;

namespace ChatServer.Application.Features.Client.Commands.UploadMedia;

public sealed class UploadMediaCommandValidator : AbstractValidator<UploadMediaCommand>
{
    public UploadMediaCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty();

        RuleFor(x => x.MediaId)
            .NotEmpty();
    }
}

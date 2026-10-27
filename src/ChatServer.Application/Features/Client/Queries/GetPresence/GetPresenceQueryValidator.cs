using FluentValidation;

namespace ChatServer.Application.Features.Client.Queries.GetPresence;

public class GetPresenceQueryValidator : AbstractValidator<GetPresenceQuery>
{
    public GetPresenceQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage($"The value cannot be empty: {nameof(GetPresenceQuery.UserId)} ");
    }
}

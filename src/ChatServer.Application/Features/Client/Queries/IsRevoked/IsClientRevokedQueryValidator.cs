using FluentValidation;

namespace ChatServer.Application.Features.Client.Queries.IsRevoked;

public class IsClientRevokedQueryValidator : AbstractValidator<IsClientRevokedQuery>
{
    public IsClientRevokedQueryValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage($"The value cannot be empty: {nameof(IsClientRevokedQuery.ClientId)} ");
    }
}

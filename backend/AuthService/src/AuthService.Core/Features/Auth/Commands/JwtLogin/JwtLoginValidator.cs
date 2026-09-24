using FluentValidation;
using Shared.Core.Validation;
using Shared.SharedKernel.Errors;

namespace AuthService.Core.Features.Auth.Commands.JwtLogin;

public sealed class JwtLoginValidator : AbstractValidator<JwtLoginCommand>
{
    public JwtLoginValidator()
    {
        RuleFor(c => c.Request.Email)
            .NotEmpty().WithError(GeneralErrors.ValueIsRequired("email"))
            .EmailAddress().WithError(GeneralErrors.ValueIsInvalid("email"));

        RuleFor(c => c.Request.Password)
            .NotEmpty().WithError(GeneralErrors.ValueIsRequired("password"));
    }
}
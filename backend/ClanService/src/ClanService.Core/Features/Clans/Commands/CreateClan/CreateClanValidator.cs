using ClanService.Contracts.Clans;
using ClanService.Domain;
using FluentValidation;
using Shared.Core.Validation;
using Shared.SharedKernel.Errors;

namespace ClanService.Core.Features.Clans.Commands.CreateClan;

public sealed class CreateClanValidator : AbstractValidator<CreateClanCommand>
{
    private const string TAG_PATTERN = "^[A-Za-z0-9]+$";

    public CreateClanValidator()
    {
        RuleFor(command => command.Request.Name)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("name"))
            .MaximumLength(ClanConstants.NAME_MAX_LENGTH)
            .WithError(GeneralErrors.LengthIsInvalid("name", max: ClanConstants.NAME_MAX_LENGTH));

        RuleFor(command => command.Request.Tag)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("tag"))
            .Length(ClanConstants.TAG_MIN_LENGTH, ClanConstants.TAG_MAX_LENGTH)
            .WithError(GeneralErrors.LengthIsInvalid(
                "tag",
                min: ClanConstants.TAG_MIN_LENGTH,
                max: ClanConstants.TAG_MAX_LENGTH))
            .Matches(TAG_PATTERN)
            .WithError(GeneralErrors.ValueIsInvalid("tag"));

        RuleFor(command => command.Request.Description)
            .MaximumLength(ClanConstants.DESCRIPTION_MAX_LENGTH)
            .WithError(GeneralErrors.LengthIsInvalid("description", max: ClanConstants.DESCRIPTION_MAX_LENGTH));
    }
}

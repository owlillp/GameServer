using ClanService.Contracts.Clans;
using ClanService.Core.Abstractions;
using ClanService.Domain;
using CSharpFunctionalExtensions;
using FluentValidation;
using Shared.Core.Abstractions;
using Shared.Core.Validation;
using Shared.Framework.Authentication.UserScope;
using Shared.SharedKernel.Errors;

namespace ClanService.Core.Features.Clans.Commands.CreateClan;

public sealed class CreateClanHandler(
    IValidator<CreateClanCommand> validator,
    IClanRepository repository,
    UserScopedData currentUser) : ICommandHandler<ClanSummaryDto, CreateClanCommand>
{
    public async Task<Result<ClanSummaryDto, Error>> Handle(
        CreateClanCommand command,
        CancellationToken cancellationToken = new ())
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        Guid userId = currentUser.RequireId();

        string name = command.Request.Name.Trim();
        string tag = command.Request.Tag.Trim().ToUpperInvariant();

        if (await repository.NameOrTagExistsAsync(name, tag, cancellationToken))
        {
            return Error.Conflict("clan.already.exists", "Клан с таким названием или тегом уже существует");
        }

        var clan = new Clan(name, tag, command.Request.Description, userId);
        await repository.AddAsync(clan, cancellationToken);

        return new ClanSummaryDto(
            clan.Id,
            clan.Name,
            clan.Tag,
            clan.LeaderId,
            clan.Members.Count,
            clan.CreatedAt);
    }
}

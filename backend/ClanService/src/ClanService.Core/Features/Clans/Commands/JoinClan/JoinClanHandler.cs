using ClanService.Core.Abstractions;
using ClanService.Domain;
using CSharpFunctionalExtensions;
using Shared.Core.Abstractions;
using Shared.Framework.Authentication.UserScope;
using Shared.SharedKernel.Errors;

namespace ClanService.Core.Features.Clans.Commands.JoinClan;

public sealed class JoinClanHandler(
    IClanRepository repository,
    UserScopedData currentUser) : ICommandHandler<JoinClanCommand>
{
    public async Task<UnitResult<Error>> Handle(
        JoinClanCommand command,
        CancellationToken cancellationToken = new ())
    {
        Guid userId = currentUser.RequireId();

        Clan? clan = await repository.GetByIdAsync(command.ClanId, cancellationToken);
        if (clan is null)
        {
            return GeneralErrors.NotFound(command.ClanId, "clan");
        }

        if (!clan.TryAddMember(userId))
        {
            return Error.Conflict("clan.already.member", "Вы уже состоите в этом клане");
        }

        await repository.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}

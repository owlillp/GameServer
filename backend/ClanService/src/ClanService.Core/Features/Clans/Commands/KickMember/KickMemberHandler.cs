using ClanService.Core.Abstractions;
using ClanService.Domain;
using CSharpFunctionalExtensions;
using Shared.Core.Abstractions;
using Shared.SharedKernel.Errors;

namespace ClanService.Core.Features.Clans.Commands.KickMember;

// Доступно только Admin (permission clans.admin) — назначается на эндпоинте.
public sealed class KickMemberHandler(IClanRepository repository) : ICommandHandler<KickMemberCommand>
{
    public async Task<UnitResult<Error>> Handle(
        KickMemberCommand command,
        CancellationToken cancellationToken = new ())
    {
        Clan? clan = await repository.GetByIdAsync(command.ClanId, cancellationToken);
        if (clan is null)
        {
            return GeneralErrors.NotFound(command.ClanId, "clan");
        }

        if (clan.IsLeader(command.UserId))
        {
            return Error.Validation("clan.leader.cannot.kick", "Лидера нельзя исключить из клана");
        }

        if (!clan.TryRemoveMember(command.UserId))
        {
            return GeneralErrors.NotFound(command.UserId, "clan member");
        }

        await repository.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}

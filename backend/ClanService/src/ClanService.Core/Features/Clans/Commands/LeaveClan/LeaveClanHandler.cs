using ClanService.Core.Abstractions;
using ClanService.Domain;
using CSharpFunctionalExtensions;
using Shared.Core.Abstractions;
using Shared.Framework.Authentication.UserScope;
using Shared.SharedKernel.Errors;

namespace ClanService.Core.Features.Clans.Commands.LeaveClan;

public sealed class LeaveClanHandler(
    IClanRepository repository,
    UserScopedData currentUser) : ICommandHandler<LeaveClanCommand>
{
    public async Task<UnitResult<Error>> Handle(
        LeaveClanCommand command,
        CancellationToken cancellationToken = new ())
    {
        Guid userId = currentUser.RequireId();

        Clan? clan = await repository.GetByIdAsync(command.ClanId, cancellationToken);
        if (clan is null)
        {
            return GeneralErrors.NotFound(command.ClanId, "clan");
        }

        // Передача лидерства пока не реализована — лидер должен удалить клан.
        if (clan.IsLeader(userId))
        {
            return Error.Validation("clan.leader.cannot.leave", "Лидер не может покинуть клан");
        }

        if (!clan.TryRemoveMember(userId))
        {
            return Error.Conflict("clan.not.member", "Вы не состоите в этом клане");
        }

        await repository.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}

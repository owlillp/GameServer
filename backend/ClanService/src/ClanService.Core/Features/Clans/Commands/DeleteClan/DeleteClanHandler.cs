using ClanService.Core.Abstractions;
using ClanService.Core.Authorization;
using ClanService.Domain;
using CSharpFunctionalExtensions;
using Shared.Core.Abstractions;
using Shared.Framework.Authentication.UserScope;
using Shared.SharedKernel.Errors;

namespace ClanService.Core.Features.Clans.Commands.DeleteClan;

public sealed class DeleteClanHandler(
    IClanRepository repository,
    UserScopedData currentUser) : ICommandHandler<DeleteClanCommand>
{
    public async Task<UnitResult<Error>> Handle(
        DeleteClanCommand command,
        CancellationToken cancellationToken = new ())
    {
        Guid userId = currentUser.RequireId();

        Clan? clan = await repository.GetByIdAsync(command.ClanId, cancellationToken);
        if (clan is null)
        {
            return GeneralErrors.NotFound(command.ClanId, "clan");
        }

        // Удалять может лидер или модератор/админ (permission clans.manage).
        if (!clan.IsLeader(userId) && !currentUser.HasPermission(ClanPermissions.CLANS_MANAGE))
        {
            return GeneralErrors.Forbidden();
        }

        repository.Remove(clan);
        await repository.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}

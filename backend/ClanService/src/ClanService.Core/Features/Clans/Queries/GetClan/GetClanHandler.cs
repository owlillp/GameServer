using AuthService.Contracts.HttpCommunication;
using AuthService.Contracts.Internal;
using ClanService.Contracts.Clans;
using ClanService.Core.Abstractions;
using ClanService.Domain;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Abstractions;
using Shared.SharedKernel.Errors;

namespace ClanService.Core.Features.Clans.Queries.GetClan;

public sealed class GetClanHandler(
    IReadDbContext readDbContext,
    IAuthServiceClient authServiceClient)
    : IQueryHandlerWithResult<ClanDetailsDto, GetClanQuery>
{
    public async Task<Result<ClanDetailsDto, Error>> Handle(
        GetClanQuery query,
        CancellationToken cancellationToken = new ())
    {
        Clan? clan = await readDbContext.ClansRead
            .Include(clan => clan.Members)
            .FirstOrDefaultAsync(clan => clan.Id == query.ClanId, cancellationToken);

        if (clan is null)
        {
            return GeneralErrors.NotFound(query.ClanId, "clan");
        }

        Dictionary<Guid, AuthUserLookupDto> users = await FetchUsersAsync(clan, cancellationToken);

        List<ClanMemberDto> members = clan.Members
            .OrderByDescending(member => member.UserId == clan.LeaderId)
            .ThenBy(member => member.JoinedAt)
            .Select(member => users.TryGetValue(member.UserId, out AuthUserLookupDto? user)
                ? new ClanMemberDto(member.UserId, user.Name, user.UserName, user.Email, member.JoinedAt)
                : new ClanMemberDto(member.UserId, null, null, null, member.JoinedAt))
            .ToList();

        return new ClanDetailsDto(
            clan.Id,
            clan.Name,
            clan.Tag,
            clan.Description,
            clan.LeaderId,
            clan.CreatedAt,
            clan.UpdatedAt,
            members);
    }

    // AuthService может быть недоступен — отдаём состав клана без имён,
    // не роняя чтение (graceful degradation).
    private async Task<Dictionary<Guid, AuthUserLookupDto>> FetchUsersAsync(
        Clan clan,
        CancellationToken cancellationToken)
    {
        Guid[] userIds = clan.Members.Select(member => member.UserId).ToArray();

        Result<IReadOnlyList<AuthUserLookupDto>, Error> result =
            await authServiceClient.GetUsersByIdsAsync(userIds, cancellationToken);

        return result.IsSuccess
            ? result.Value.ToDictionary(user => user.UserId)
            : [];
    }
}

using System.Net;
using ClanService.Contracts.Clans;
using ClanService.IntegrationTests.Infrastructure;
using Shared.SharedKernel;

namespace ClanService.IntegrationTests.Features.Clans;

// Kick — метод только для Admin (permission clans.admin).
public sealed class KickMemberTests : IntegrationTestsBase
{
    private const string ADMIN_ROLES = "Admin,User";
    private const string MODERATOR_ROLES = "Moderator,User";
    private const string USER_ROLES = "User";

    public KickMemberTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Kick_Anonymous_IsUnauthorized()
    {
        (Guid clanId, Guid memberId) = await SeedClanWithMemberAsync();

        ClearAuthorization();

        HttpResponseMessage response = await PostRawAsync($"/clans/{clanId}/members/{memberId}/kick");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Kick_RegularUser_IsForbidden()
    {
        (Guid clanId, Guid memberId) = await SeedClanWithMemberAsync();

        AuthorizeAs(roles: USER_ROLES);

        HttpResponseMessage response = await PostRawAsync($"/clans/{clanId}/members/{memberId}/kick");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Kick_Moderator_IsForbidden()
    {
        (Guid clanId, Guid memberId) = await SeedClanWithMemberAsync();

        AuthorizeAs(roles: MODERATOR_ROLES);

        HttpResponseMessage response = await PostRawAsync($"/clans/{clanId}/members/{memberId}/kick");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Kick_Admin_RemovesMember()
    {
        (Guid clanId, Guid memberId) = await SeedClanWithMemberAsync();

        AuthorizeAs(roles: ADMIN_ROLES);

        HttpResponseMessage response = await PostRawAsync($"/clans/{clanId}/members/{memberId}/kick");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Envelope<ClanDetailsDto> clan = await GetAsync<ClanDetailsDto>($"/clans/{clanId}");
        Assert.NotNull(clan.Result);
        Assert.Single(clan.Result.Members);
        Assert.DoesNotContain(
            clan.Result.Members,
            member => member.UserId == memberId);
    }

    [Fact]
    public async Task Kick_AdminCannotKickLeader()
    {
        (Guid clanId, Guid _) = await SeedClanWithMemberAsync();
        Guid leaderId = await GetLeaderIdAsync(clanId);

        AuthorizeAs(roles: ADMIN_ROLES);

        HttpResponseMessage response = await PostRawAsync($"/clans/{clanId}/members/{leaderId}/kick");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Kick_AdminKicksNonMember_ReturnsNotFound()
    {
        (Guid clanId, Guid _) = await SeedClanWithMemberAsync();

        AuthorizeAs(roles: ADMIN_ROLES);

        HttpResponseMessage response = await PostRawAsync($"/clans/{clanId}/members/{Guid.NewGuid()}/kick");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<(Guid ClanId, Guid MemberId)> SeedClanWithMemberAsync()
    {
        Guid leaderId = Guid.NewGuid();
        Guid memberId = Guid.NewGuid();

        AuthorizeAs(leaderId, roles: USER_ROLES);
        Envelope<ClanSummaryDto> created = await PostAsync<ClanSummaryDto>(
            "/clans",
            new CreateClanRequest("Moderated Clan", "mc", null));

        Assert.NotNull(created.Result);

        AuthorizeAs(memberId, roles: USER_ROLES);
        HttpResponseMessage join = await PostRawAsync($"/clans/{created.Result.Id}/join");
        Assert.Equal(HttpStatusCode.OK, join.StatusCode);

        return (created.Result.Id, memberId);
    }

    private async Task<Guid> GetLeaderIdAsync(Guid clanId)
    {
        AuthorizeAs(roles: USER_ROLES);
        Envelope<ClanDetailsDto> clan = await GetAsync<ClanDetailsDto>($"/clans/{clanId}");
        Assert.NotNull(clan.Result);

        return clan.Result.LeaderId;
    }
}

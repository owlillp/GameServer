using System.Net;
using ClanService.Contracts.Clans;
using ClanService.IntegrationTests.Infrastructure;
using Shared.SharedKernel;

namespace ClanService.IntegrationTests.Features.Clans;

public sealed class MembershipTests : IntegrationTestsBase
{
    public MembershipTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Join_AddsMember_And_SecondJoinReturnsConflict()
    {
        Guid leaderId = Guid.NewGuid();
        Guid memberId = Guid.NewGuid();

        AuthorizeAs(leaderId, roles: "User");
        Envelope<ClanSummaryDto> created = await PostAsync<ClanSummaryDto>(
            "/clans",
            new CreateClanRequest("Joinable", "jn", null));

        Assert.NotNull(created.Result);

        AuthorizeAs(memberId, roles: "User");
        HttpResponseMessage join = await PostRawAsync($"/clans/{created.Result.Id}/join");
        Assert.Equal(HttpStatusCode.OK, join.StatusCode);

        HttpResponseMessage secondJoin = await PostRawAsync($"/clans/{created.Result.Id}/join");
        Assert.Equal(HttpStatusCode.Conflict, secondJoin.StatusCode);
    }

    [Fact]
    public async Task Join_UnknownClan_ReturnsNotFound()
    {
        AuthorizeAs();

        HttpResponseMessage response = await PostRawAsync($"/clans/{Guid.NewGuid()}/join");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Leave_Leader_IsRejected()
    {
        AuthorizeAs();

        Envelope<ClanSummaryDto> created = await PostAsync<ClanSummaryDto>(
            "/clans",
            new CreateClanRequest("Leader Home", "lh", null));

        Assert.NotNull(created.Result);

        HttpResponseMessage response = await PostRawAsync($"/clans/{created.Result.Id}/leave");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Leave_Member_RemovesMembership()
    {
        Guid leaderId = Guid.NewGuid();
        Guid memberId = Guid.NewGuid();

        AuthorizeAs(leaderId, roles: "User");
        Envelope<ClanSummaryDto> created = await PostAsync<ClanSummaryDto>(
            "/clans",
            new CreateClanRequest("Leavable", "lv", null));

        Assert.NotNull(created.Result);

        AuthorizeAs(memberId, roles: "User");
        await PostRawAsync($"/clans/{created.Result.Id}/join");

        HttpResponseMessage leave = await PostRawAsync($"/clans/{created.Result.Id}/leave");
        Assert.Equal(HttpStatusCode.OK, leave.StatusCode);

        AuthorizeAs(roles: "User");

        Envelope<ClanDetailsDto> clan = await GetAsync<ClanDetailsDto>($"/clans/{created.Result.Id}");
        Assert.NotNull(clan.Result);
        Assert.Single(clan.Result.Members);
    }

    [Fact]
    public async Task Leave_WithoutMembership_ReturnsConflict()
    {
        AuthorizeAs();

        Envelope<ClanSummaryDto> created = await PostAsync<ClanSummaryDto>(
            "/clans",
            new CreateClanRequest("Closed", "cl", null));

        Assert.NotNull(created.Result);

        AuthorizeAs();

        HttpResponseMessage response = await PostRawAsync($"/clans/{created.Result.Id}/leave");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}

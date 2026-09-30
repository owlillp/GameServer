using System.Net;
using ClanService.Contracts.Clans;
using ClanService.IntegrationTests.Infrastructure;
using Shared.SharedKernel;

namespace ClanService.IntegrationTests.Features.Clans;

public sealed class GetClanTests : IntegrationTestsBase
{
    public GetClanTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Get_Anonymous_IsUnauthorized()
    {
        ClearAuthorization();

        HttpResponseMessage response = await HttpClient.GetAsync($"/clans/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_EnrichesMembersWithAuthServiceData()
    {
        Guid leaderId = Guid.NewGuid();
        Guid memberId = Guid.NewGuid();
        AuthServiceClient
            .AddUser(leaderId, "Leader Name", "leader", "leader@test.com")
            .AddUser(memberId, "Member Name", "member", "member@test.com");

        AuthorizeAs(leaderId, name: "Leader Name", email: "leader@test.com", roles: "User");
        Envelope<ClanSummaryDto> created = await PostAsync<ClanSummaryDto>(
            "/clans",
            new CreateClanRequest("Enriched", "en", "Описание"));

        Assert.NotNull(created.Result);

        AuthorizeAs(memberId, name: "Member Name", email: "member@test.com", roles: "User");
        HttpResponseMessage joinResponse = await PostRawAsync($"/clans/{created.Result.Id}/join");
        Assert.Equal(HttpStatusCode.OK, joinResponse.StatusCode);

        // Любой авторизованный пользователь может смотреть состав.
        AuthorizeAs(roles: "User");

        Envelope<ClanDetailsDto> envelope = await GetAsync<ClanDetailsDto>($"/clans/{created.Result.Id}");

        Assert.NotNull(envelope.Result);
        Assert.Equal("Enriched", envelope.Result.Name);
        Assert.Equal("Описание", envelope.Result.Description);
        Assert.Equal(2, envelope.Result.Members.Count);

        // Лидер идёт первым и обогащён данными из AuthService.
        ClanMemberDto leader = envelope.Result.Members[0];
        Assert.Equal(leaderId, leader.UserId);
        Assert.Equal("Leader Name", leader.Name);
        Assert.Equal("leader@test.com", leader.Email);

        ClanMemberDto member = envelope.Result.Members[1];
        Assert.Equal(memberId, member.UserId);
        Assert.Equal("Member Name", member.Name);
    }

    [Fact]
    public async Task Get_WithoutAuthServiceData_StillReturnsMembers()
    {
        Guid leaderId = Guid.NewGuid();
        AuthorizeAs(leaderId, roles: "User");
        Envelope<ClanSummaryDto> created = await PostAsync<ClanSummaryDto>(
            "/clans",
            new CreateClanRequest("Anon Members", "am", null));

        Assert.NotNull(created.Result);

        AuthorizeAs(roles: "User");

        Envelope<ClanDetailsDto> envelope = await GetAsync<ClanDetailsDto>($"/clans/{created.Result.Id}");

        Assert.NotNull(envelope.Result);
        ClanMemberDto member = Assert.Single(envelope.Result.Members);
        Assert.Equal(leaderId, member.UserId);
        Assert.Null(member.Name);
    }
}

using ClanService.Contracts.Clans;
using ClanService.IntegrationTests.Infrastructure;
using Shared.SharedKernel;

namespace ClanService.IntegrationTests.Features.Clans;

public sealed class CreateClanTests : IntegrationTestsBase
{
    public CreateClanTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateClan_Authorized_ReturnsClanWithLeaderAsMember()
    {
        Guid userId = Guid.NewGuid();
        AuthorizeAs(userId, name: "Leader", email: "leader@test.com", roles: "User");
        AuthServiceClient.AddUser(userId, "Leader", "leader", "leader@test.com");

        Envelope<ClanSummaryDto> envelope = await PostAsync<ClanSummaryDto>(
            "/clans",
            new CreateClanRequest("Night Watch", "nw", "Клан ночных стражей"));

        Assert.False(envelope.IsError);
        Assert.NotNull(envelope.Result);
        Assert.Equal("Night Watch", envelope.Result.Name);
        Assert.Equal("NW", envelope.Result.Tag);
        Assert.Equal(userId, envelope.Result.LeaderId);
        Assert.Equal(1, envelope.Result.MemberCount);
    }

    [Fact]
    public async Task CreateClan_Anonymous_IsUnauthorized()
    {
        ClearAuthorization();

        HttpResponseMessage response = await PostRawAsync(
            "/clans",
            new CreateClanRequest("Ghost", "gh", null));

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateClan_DuplicateTag_ReturnsConflict()
    {
        AuthorizeAs();

        await PostAsync<ClanSummaryDto>("/clans", new CreateClanRequest("First", "dup", null));

        AuthorizeAs();

        HttpResponseMessage response = await PostRawAsync(
            "/clans",
            new CreateClanRequest("Second", "DUP", null));

        Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateClan_InvalidTag_ReturnsValidationError()
    {
        AuthorizeAs();

        HttpResponseMessage response = await PostRawAsync(
            "/clans",
            new CreateClanRequest("Bad Tag Clan", "a b!", null));

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateClan_TagIsNormalizedToUpperCase()
    {
        AuthorizeAs();

        Envelope<ClanSummaryDto> envelope = await PostAsync<ClanSummaryDto>(
            "/clans",
            new CreateClanRequest("Lower Tag", "abc", null));

        Assert.NotNull(envelope.Result);
        Assert.Equal("ABC", envelope.Result.Tag);
    }
}

using System.Net;
using ClanService.Contracts.Clans;
using ClanService.IntegrationTests.Infrastructure;
using Shared.SharedKernel;

namespace ClanService.IntegrationTests.Features.Clans;

public sealed class DeleteClanTests : IntegrationTestsBase
{
    public DeleteClanTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Delete_ByStrangerWithoutPermission_IsForbidden()
    {
        AuthorizeAs(roles: "User");
        Envelope<ClanSummaryDto> created = await PostAsync<ClanSummaryDto>(
            "/clans",
            new CreateClanRequest("Protected", "pr", null));

        Assert.NotNull(created.Result);

        AuthorizeAs(roles: "User");

        HttpResponseMessage response = await HttpClient.DeleteAsync($"/clans/{created.Result.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ByLeader_Succeeds()
    {
        Guid leaderId = Guid.NewGuid();
        AuthorizeAs(leaderId, roles: "User");
        Envelope<ClanSummaryDto> created = await PostAsync<ClanSummaryDto>(
            "/clans",
            new CreateClanRequest("Temporary", "tp", null));

        Assert.NotNull(created.Result);

        HttpResponseMessage response = await HttpClient.DeleteAsync($"/clans/{created.Result.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AuthorizeAs(roles: "User");
        HttpResponseMessage getResponse = await HttpClient.GetAsync($"/clans/{created.Result.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_ByModerator_Succeeds()
    {
        AuthorizeAs(roles: "User");
        Envelope<ClanSummaryDto> created = await PostAsync<ClanSummaryDto>(
            "/clans",
            new CreateClanRequest("Moderated", "md", null));

        Assert.NotNull(created.Result);

        // Moderator получает permission clans.manage.
        AuthorizeAs(roles: "Moderator");

        HttpResponseMessage response = await HttpClient.DeleteAsync($"/clans/{created.Result.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

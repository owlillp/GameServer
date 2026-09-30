using System.Net;
using ClanService.Contracts.Clans;
using ClanService.IntegrationTests.Infrastructure;
using Shared.SharedKernel;
using Shared.SharedKernel.Responses;

namespace ClanService.IntegrationTests.Features.Clans;

public sealed class ListClansTests : IntegrationTestsBase
{
    public ListClansTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task List_Anonymous_IsUnauthorized()
    {
        ClearAuthorization();

        HttpResponseMessage response = await HttpClient.GetAsync("/clans?page=1&pageSize=20");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task List_ReturnsPaginatedClans()
    {
        AuthorizeAs();
        await PostAsync<ClanSummaryDto>("/clans", new CreateClanRequest("Alpha", "al", null));
        await PostAsync<ClanSummaryDto>("/clans", new CreateClanRequest("Bravo", "br", null));
        await PostAsync<ClanSummaryDto>("/clans", new CreateClanRequest("Charlie", "ch", null));

        Envelope<PaginationResponse<ClanSummaryDto>> envelope =
            await GetAsync<PaginationResponse<ClanSummaryDto>>("/clans?page=1&pageSize=2");

        Assert.NotNull(envelope.Result);
        Assert.Equal(3, envelope.Result.TotalCount);
        Assert.Equal(2, envelope.Result.Items.Count);
        Assert.Equal(2, envelope.Result.TotalPages);
    }

    [Fact]
    public async Task List_SearchByName_IsCaseInsensitive()
    {
        AuthorizeAs();
        await PostAsync<ClanSummaryDto>("/clans", new CreateClanRequest("Alpha", "al", null));
        await PostAsync<ClanSummaryDto>("/clans", new CreateClanRequest("Bravo", "br", null));

        Envelope<PaginationResponse<ClanSummaryDto>> envelope =
            await GetAsync<PaginationResponse<ClanSummaryDto>>("/clans?page=1&pageSize=20&search=brav");

        Assert.NotNull(envelope.Result);
        Assert.Equal(1, envelope.Result.TotalCount);

        ClanSummaryDto clan = Assert.Single(envelope.Result.Items);
        Assert.Equal("Bravo", clan.Name);
    }

    [Fact]
    public async Task List_SearchByTag_MatchesUppercaseTag()
    {
        AuthorizeAs();
        await PostAsync<ClanSummaryDto>("/clans", new CreateClanRequest("Alpha", "xa", null));
        await PostAsync<ClanSummaryDto>("/clans", new CreateClanRequest("Bravo", "xb", null));

        Envelope<PaginationResponse<ClanSummaryDto>> envelope =
            await GetAsync<PaginationResponse<ClanSummaryDto>>("/clans?page=1&pageSize=20&search=XB");

        Assert.NotNull(envelope.Result);
        Assert.Equal(1, envelope.Result.TotalCount);

        ClanSummaryDto clan = Assert.Single(envelope.Result.Items);
        Assert.Equal("Bravo", clan.Name);
    }

    [Fact]
    public async Task Get_UnknownClan_ReturnsNotFound()
    {
        AuthorizeAs();

        HttpResponseMessage response = await HttpClient.GetAsync($"/clans/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuthService.Contracts.Admin;
using AuthService.Domain;
using AuthService.IntegrationTests.Infrastructure;
using Shared.SharedKernel;
using Shared.SharedKernel.Responses;

namespace AuthService.IntegrationTests.Features.Admin;

public sealed class AdminEndpointsTests : IntegrationTestsBase
{
    private const string PASSWORD = "TestPass123!";
    private const string SCOPE = "openid profile email offline_access auth";

    private readonly OidcTestHelper _oidc;

    public AdminEndpointsTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
        _oidc = new OidcTestHelper(factory);
    }

    [Fact]
    public async Task Users_Anonymous_IsUnauthorized()
    {
        await SeedPlatformConfigAsync();

        HttpResponseMessage response = await HttpClient.GetAsync("/auth/admin/users?page=1&pageSize=20");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Users_RegularUser_IsForbidden()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync("regular@test.com", PASSWORD);
        string accessToken = await GetUserAccessTokenAsync("regular@test.com");

        HttpResponseMessage response = await GetAuthorizedAsync("/auth/admin/users?page=1&pageSize=20", accessToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Users_ServiceToken_IsForbidden()
    {
        await SeedPlatformConfigAsync();

        OidcTokenResponse tokens = await _oidc.ExecuteClientCredentialsFlowAsync(
            IntegrationTestsWebFactory.SERVICE_CLIENT_ID,
            IntegrationTestsWebFactory.SERVICE_CLIENT_SECRET,
            "auth");

        Assert.True(tokens.IsSuccess, tokens.RawResponse);

        HttpResponseMessage response = await GetAuthorizedAsync("/auth/admin/users?page=1&pageSize=20", tokens.AccessToken!);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Users_Moderator_SeesRegisteredUsers()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync("moderator@test.com", PASSWORD, "moderatoruser");
        await RegisterAsync("player1@test.com", PASSWORD, "playerone");
        await RegisterAsync("player2@test.com", PASSWORD, "playertwo");
        await AddRoleAsync("moderator@test.com", AuthRoles.MODERATOR);

        string accessToken = await GetUserAccessTokenAsync("moderator@test.com");
        HttpResponseMessage response = await GetAuthorizedAsync("/auth/admin/users?page=1&pageSize=20", accessToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Envelope<PaginationResponse<AdminUserDto>>? envelope =
            await response.Content.ReadFromJsonAsync<Envelope<PaginationResponse<AdminUserDto>>>();

        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Result);
        Assert.Equal(3, envelope.Result.TotalCount);

        AdminUserDto moderator = envelope.Result.Items.Single(user => string.Equals(user.Email, "moderator@test.com", StringComparison.Ordinal));
        Assert.Contains(AuthRoles.MODERATOR, moderator.Roles, StringComparer.Ordinal);
        Assert.Contains(AuthRoles.USER, moderator.Roles, StringComparer.Ordinal);

        AdminUserDto player = envelope.Result.Items.Single(user => string.Equals(user.Email, "player1@test.com", StringComparison.Ordinal));
        Assert.Equal(new[] { AuthRoles.USER }, player.Roles);
        Assert.False(player.IsLockedOut);
    }

    [Fact]
    public async Task Users_Search_FiltersByEmail()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync("admin@test.com", PASSWORD, "adminuser");
        await RegisterAsync("needle@test.com", PASSWORD, "needleuser");
        await RegisterAsync("other@test.com", PASSWORD, "otheruser");
        await AddRoleAsync("admin@test.com", AuthRoles.ADMIN);

        string accessToken = await GetUserAccessTokenAsync("admin@test.com");
        HttpResponseMessage response = await GetAuthorizedAsync("/auth/admin/users?page=1&pageSize=20&search=needle", accessToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Envelope<PaginationResponse<AdminUserDto>>? envelope =
            await response.Content.ReadFromJsonAsync<Envelope<PaginationResponse<AdminUserDto>>>();

        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Result);
        Assert.Equal(1, envelope.Result.TotalCount);
        Assert.Equal("needle@test.com", envelope.Result.Items[0].Email);
    }

    [Fact]
    public async Task Users_Pagination_ReturnsRequestedPage()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync("page-admin@test.com", PASSWORD, "pageadmin");
        await RegisterAsync("page1@test.com", PASSWORD, "pageone");
        await RegisterAsync("page2@test.com", PASSWORD, "pagetwo");
        await AddRoleAsync("page-admin@test.com", AuthRoles.ADMIN);

        string accessToken = await GetUserAccessTokenAsync("page-admin@test.com");
        HttpResponseMessage response = await GetAuthorizedAsync("/auth/admin/users?page=2&pageSize=2", accessToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Envelope<PaginationResponse<AdminUserDto>>? envelope =
            await response.Content.ReadFromJsonAsync<Envelope<PaginationResponse<AdminUserDto>>>();

        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Result);
        Assert.Equal(3, envelope.Result.TotalCount);
        Assert.Equal(2, envelope.Result.Page);
        Assert.Equal(2, envelope.Result.PageSize);
        Assert.Equal(2, envelope.Result.TotalPages);
        Assert.Single(envelope.Result.Items);
    }

    [Fact]
    public async Task Stats_Admin_ReturnsCounters()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync("stats-admin@test.com", PASSWORD, "statsadmin");
        await RegisterAsync("stats-user@test.com", PASSWORD, "statsuser");
        await AddRoleAsync("stats-admin@test.com", AuthRoles.ADMIN);
        await AddRoleAsync("stats-user@test.com", AuthRoles.MODERATOR);

        string accessToken = await GetUserAccessTokenAsync("stats-admin@test.com");
        HttpResponseMessage response = await GetAuthorizedAsync("/auth/admin/stats", accessToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Envelope<AdminStatsDto>? envelope = await response.Content.ReadFromJsonAsync<Envelope<AdminStatsDto>>();

        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Result);
        Assert.Equal(2, envelope.Result.TotalUsers);
        Assert.Equal(1, envelope.Result.AdminCount);
        Assert.Equal(1, envelope.Result.ModeratorCount);
        Assert.Equal(0, envelope.Result.LockedOutCount);
    }

    private async Task<HttpResponseMessage> GetAuthorizedAsync(string url, string accessToken)
    {
        using HttpClient client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return await client.GetAsync(url);
    }

    private async Task<string> GetUserAccessTokenAsync(string email)
    {
        await _oidc.LoginAsync(email, PASSWORD);

        OidcTokenResponse tokens = await _oidc.ExecuteAuthorizationCodeFlowAsync(
            IntegrationTestsWebFactory.WEB_CLIENT_ID,
            clientSecret: null,
            SCOPE);

        Assert.True(tokens.IsSuccess, tokens.RawResponse);

        return tokens.AccessToken!;
    }
}

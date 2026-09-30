using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuthService.Contracts.Internal;
using AuthService.IntegrationTests.Infrastructure;
using Shared.SharedKernel;

namespace AuthService.IntegrationTests.Features.Internal;

public sealed class InternalUsersEndpointTests : IntegrationTestsBase
{
    private const string PASSWORD = "TestPass123!";
    private const string SCOPE = "openid profile email offline_access auth";

    private readonly OidcTestHelper _oidc;

    public InternalUsersEndpointTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
        _oidc = new OidcTestHelper(factory);
    }

    [Fact]
    public async Task Batch_ServiceToken_ReturnsUsers()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync("internal-1@test.com", PASSWORD, "internalone");
        await RegisterAsync("internal-2@test.com", PASSWORD, "internaltwo");

        Guid firstId = await GetAccountIdAsync("internal-1@test.com");
        Guid secondId = await GetAccountIdAsync("internal-2@test.com");

        OidcTokenResponse service = await GetServiceTokenAsync();
        HttpResponseMessage response = await PostBatchAsync(service.AccessToken!, [firstId, secondId]);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Envelope<IReadOnlyList<AuthUserLookupDto>>? envelope =
            await response.Content.ReadFromJsonAsync<Envelope<IReadOnlyList<AuthUserLookupDto>>>();

        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Result);
        Assert.Equal(2, envelope.Result.Count);
        Assert.Contains(
            envelope.Result,
            user => string.Equals(user.Email, "internal-1@test.com", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Batch_UserToken_IsForbidden()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync("internal-user@test.com", PASSWORD, "internaluser");
        Guid accountId = await GetAccountIdAsync("internal-user@test.com");

        await _oidc.LoginAsync("internal-user@test.com", PASSWORD);
        OidcTokenResponse tokens = await _oidc.ExecuteAuthorizationCodeFlowAsync(
            IntegrationTestsWebFactory.WEB_CLIENT_ID,
            clientSecret: null,
            SCOPE);

        Assert.True(tokens.IsSuccess, tokens.RawResponse);

        HttpResponseMessage response = await PostBatchAsync(tokens.AccessToken!, [accountId]);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Batch_Anonymous_IsUnauthorized()
    {
        await SeedPlatformConfigAsync();

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "/internal/users/batch",
            new InternalUsersBatchRequest([]));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Batch_TooManyIds_IsValidationError()
    {
        await SeedPlatformConfigAsync();

        OidcTokenResponse service = await GetServiceTokenAsync();
        Guid[] userIds = Enumerable.Range(0, 501).Select(_ => Guid.NewGuid()).ToArray();

        HttpResponseMessage response = await PostBatchAsync(service.AccessToken!, userIds);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<OidcTokenResponse> GetServiceTokenAsync()
    {
        OidcTokenResponse service = await _oidc.ExecuteClientCredentialsFlowAsync(
            IntegrationTestsWebFactory.SERVICE_CLIENT_ID,
            IntegrationTestsWebFactory.SERVICE_CLIENT_SECRET,
            "auth");

        Assert.True(service.IsSuccess, service.RawResponse);
        return service;
    }

    private async Task<HttpResponseMessage> PostBatchAsync(string accessToken, IReadOnlyList<Guid> userIds)
    {
        using HttpClient client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return await client.PostAsJsonAsync("/internal/users/batch", new InternalUsersBatchRequest(userIds));
    }
}

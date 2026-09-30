using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuthService.Contracts.Dtos;
using AuthService.IntegrationTests.Infrastructure;
using Shared.SharedKernel;

namespace AuthService.IntegrationTests.Features.Auth;

public sealed class ProfileEndpointTests : IntegrationTestsBase
{
    private const string EMAIL = "profile@test.com";
    private const string PASSWORD = "TestPass123!";
    private const string USER_NAME = "profileuser";
    private const string SCOPE = "openid profile email offline_access auth";

    private readonly OidcTestHelper _oidc;

    public ProfileEndpointTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
        _oidc = new OidcTestHelper(factory);
    }

    [Fact]
    public async Task WithUserAccessToken_ReturnsProfile()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync(EMAIL, PASSWORD, USER_NAME);
        await _oidc.LoginAsync(EMAIL, PASSWORD);

        OidcTokenResponse tokens = await _oidc.ExecuteAuthorizationCodeFlowAsync(
            IntegrationTestsWebFactory.WEB_CLIENT_ID,
            clientSecret: null,
            SCOPE);

        Assert.True(tokens.IsSuccess, tokens.RawResponse);

        using HttpClient client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        HttpResponseMessage response = await client.GetAsync("/auth/profile");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Envelope<ProfileDto>? envelope = await response.Content.ReadFromJsonAsync<Envelope<ProfileDto>>();

        Assert.NotNull(envelope);
        Assert.False(envelope.IsError);
        Assert.NotNull(envelope.Result);
        Assert.Equal(EMAIL, envelope.Result.Email);
        Assert.Equal(USER_NAME, envelope.Result.Username);
    }

    [Fact]
    public async Task WithoutToken_IsUnauthorized()
    {
        await SeedPlatformConfigAsync();

        HttpResponseMessage response = await HttpClient.GetAsync("/auth/profile");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WithServiceToken_IsUnauthorized()
    {
        await SeedPlatformConfigAsync();

        OidcTokenResponse tokens = await _oidc.ExecuteClientCredentialsFlowAsync(
            IntegrationTestsWebFactory.SERVICE_CLIENT_ID,
            IntegrationTestsWebFactory.SERVICE_CLIENT_SECRET,
            "auth");

        Assert.True(tokens.IsSuccess, tokens.RawResponse);

        using HttpClient client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        HttpResponseMessage response = await client.GetAsync("/auth/profile");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync("duplicate@test.com", PASSWORD);

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "/auth/register",
            new { email = "duplicate@test.com", userName = "Another", password = PASSWORD });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}

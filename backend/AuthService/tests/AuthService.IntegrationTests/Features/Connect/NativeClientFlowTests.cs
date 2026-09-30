using System.Net;
using AuthService.Domain;
using AuthService.IntegrationTests.Infrastructure;

namespace AuthService.IntegrationTests.Features.Connect;

// Флоу нативного клиента (Unity): public client + PKCE, loopback/custom scheme.
public sealed class NativeClientFlowTests : IntegrationTestsBase
{
    private const string EMAIL = "unity-user@test.com";
    private const string PASSWORD = "TestPass123!";
    private const string SCOPE = "openid profile email offline_access auth";

    private readonly OidcTestHelper _oidc;

    public NativeClientFlowTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
        _oidc = new OidcTestHelper(factory);
    }

    [Fact]
    public async Task UnityClient_LoopbackFlow_ReturnsTokens()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync(EMAIL, PASSWORD, "unityuser");
        await _oidc.LoginAsync(EMAIL, PASSWORD);

        OidcTokenResponse tokens = await _oidc.ExecuteAuthorizationCodeFlowAsync(
            IntegrationTestsWebFactory.GAME_CLIENT_ID,
            clientSecret: null,
            SCOPE,
            IntegrationTestsWebFactory.GAME_REDIRECT_URI);

        Assert.True(tokens.IsSuccess, tokens.RawResponse);
        Assert.NotNull(tokens.AccessToken);
        Assert.NotNull(tokens.RefreshToken);

        JwtClaims claims = OidcTestHelper.ParseJwt(tokens.AccessToken);
        Assert.Equal(EMAIL, claims.GetString("email"));
        Assert.Contains(AuthRoles.USER, claims.GetValues("role"), StringComparer.Ordinal);
    }

    [Fact]
    public async Task UnityClient_CustomSchemeRedirect_IsAccepted()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync(EMAIL, PASSWORD, "unityuser");
        await _oidc.LoginAsync(EMAIL, PASSWORD);

        OidcTokenResponse tokens = await _oidc.ExecuteAuthorizationCodeFlowAsync(
            IntegrationTestsWebFactory.GAME_CLIENT_ID,
            clientSecret: null,
            SCOPE,
            IntegrationTestsWebFactory.GAME_CUSTOM_SCHEME_REDIRECT_URI);

        Assert.True(tokens.IsSuccess, tokens.RawResponse);
        Assert.NotNull(tokens.AccessToken);
    }

    [Fact]
    public async Task UnityClient_UnregisteredRedirectUri_IsRejected()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync(EMAIL, PASSWORD, "unityuser");
        await _oidc.LoginAsync(EMAIL, PASSWORD);

        string codeChallenge = OidcTestHelper.GenerateCodeChallenge(OidcTestHelper.GenerateCodeVerifier());

        HttpResponseMessage response = await _oidc.AuthorizeAsync(
            IntegrationTestsWebFactory.GAME_CLIENT_ID,
            SCOPE,
            "http://127.0.0.1:9999/callback",
            codeChallenge);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UnityClient_ClientCredentials_IsRejected()
    {
        await SeedPlatformConfigAsync();

        OidcTokenResponse response = await _oidc.ExecuteClientCredentialsFlowAsync(
            IntegrationTestsWebFactory.GAME_CLIENT_ID,
            "any-secret",
            "auth");

        Assert.False(response.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

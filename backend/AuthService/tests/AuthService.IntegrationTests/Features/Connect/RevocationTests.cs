using System.Net;
using AuthService.IntegrationTests.Infrastructure;

namespace AuthService.IntegrationTests.Features.Connect;

public sealed class RevocationTests : IntegrationTestsBase
{
    private const string EMAIL = "revocation@test.com";
    private const string PASSWORD = "TestPass123!";
    private const string SCOPE = "openid profile email offline_access auth";

    private readonly OidcTestHelper _oidc;

    public RevocationTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
        _oidc = new OidcTestHelper(factory);
    }

    [Fact]
    public async Task RevokedRefreshToken_CannotBeUsed()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync(EMAIL, PASSWORD);
        await _oidc.LoginAsync(EMAIL, PASSWORD);

        OidcTokenResponse tokens = await _oidc.ExecuteAuthorizationCodeFlowAsync(
            IntegrationTestsWebFactory.WEB_CLIENT_ID,
            clientSecret: null,
            SCOPE);

        Assert.True(tokens.IsSuccess, tokens.RawResponse);
        Assert.NotNull(tokens.RefreshToken);

        HttpResponseMessage revocationResponse = await _oidc.RevokeTokenAsync(
            IntegrationTestsWebFactory.WEB_CLIENT_ID,
            tokens.RefreshToken,
            "refresh_token");

        Assert.Equal(HttpStatusCode.OK, revocationResponse.StatusCode);

        OidcTokenResponse refreshed = await _oidc.RefreshTokenAsync(
            IntegrationTestsWebFactory.WEB_CLIENT_ID,
            clientSecret: null,
            tokens.RefreshToken);

        Assert.False(refreshed.IsSuccess);
        Assert.Equal("invalid_grant", refreshed.Error);
    }
}

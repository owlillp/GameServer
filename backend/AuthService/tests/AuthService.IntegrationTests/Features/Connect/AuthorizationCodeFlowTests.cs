using System.Net;
using AuthService.Domain;
using AuthService.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.IntegrationTests.Features.Connect;

public sealed class AuthorizationCodeFlowTests : IntegrationTestsBase
{
    private const string EMAIL = "oidc-user@test.com";
    private const string PASSWORD = "TestPass123!";
    private const string USER_NAME = "oidcuser";
    private const string SCOPE = "openid profile email offline_access auth";

    public AuthorizationCodeFlowTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task FullFlow_ReturnsTokensWithUserClaims()
    {
        OidcTestHelper oidc = await SeedUserAndLoginAsync();

        OidcTokenResponse response = await oidc.ExecuteAuthorizationCodeFlowAsync(
            IntegrationTestsWebFactory.WEB_CLIENT_ID,
            clientSecret: null,
            SCOPE);

        Assert.True(response.IsSuccess, response.RawResponse);
        Assert.NotNull(response.AccessToken);
        Assert.NotNull(response.RefreshToken);
        Assert.NotNull(response.IdToken);
        Assert.Equal("Bearer", response.TokenType);

        JwtClaims accessClaims = OidcTestHelper.ParseJwt(response.AccessToken);

        Assert.False(string.IsNullOrEmpty(accessClaims.GetString("sub")));
        Assert.Equal(EMAIL, accessClaims.GetString("email"));
        Assert.Equal(USER_NAME, accessClaims.GetString("name"));
        Assert.Equal(USER_NAME, accessClaims.GetString("preferred_username"));
        Assert.Contains(AuthRoles.USER, accessClaims.GetValues("role"), StringComparer.Ordinal);
        Assert.Contains("auth-service", accessClaims.GetValues("aud"), StringComparer.Ordinal);

        JwtClaims idClaims = OidcTestHelper.ParseJwt(response.IdToken);
        Assert.Equal(EMAIL, idClaims.GetString("email"));
    }

    [Fact]
    public async Task WithoutAuthentication_RedirectsToFrontendLogin()
    {
        await SeedPlatformConfigAsync();
        var oidc = new OidcTestHelper(Factory);

        string codeChallenge = OidcTestHelper.GenerateCodeChallenge(OidcTestHelper.GenerateCodeVerifier());

        HttpResponseMessage response = await oidc.AuthorizeAsync(
            IntegrationTestsWebFactory.WEB_CLIENT_ID,
            SCOPE,
            IntegrationTestsWebFactory.WEB_REDIRECT_URI,
            codeChallenge);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        string location = response.Headers.Location?.ToString() ?? string.Empty;

        Assert.StartsWith(IntegrationTestsWebFactory.FRONTEND_LOGIN_URL, location, StringComparison.Ordinal);
        Assert.Contains("returnUrl=", location, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WithoutPkce_IsRejected()
    {
        OidcTestHelper oidc = await SeedUserAndLoginAsync();

        HttpResponseMessage response = await oidc.AuthorizeAsync(
            IntegrationTestsWebFactory.WEB_CLIENT_ID,
            SCOPE,
            IntegrationTestsWebFactory.WEB_REDIRECT_URI,
            codeChallenge: null);

        if (response.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.Found)
        {
            string? error = OidcTestHelper.GetQueryParameter(response.Headers.Location!, "error");
            Assert.Equal("invalid_request", error);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    [Fact]
    public async Task RefreshToken_ReturnsNewTokens()
    {
        OidcTestHelper oidc = await SeedUserAndLoginAsync();

        OidcTokenResponse initial = await oidc.ExecuteAuthorizationCodeFlowAsync(
            IntegrationTestsWebFactory.WEB_CLIENT_ID,
            clientSecret: null,
            SCOPE);

        Assert.True(initial.IsSuccess, initial.RawResponse);
        Assert.NotNull(initial.RefreshToken);

        OidcTokenResponse refreshed = await oidc.RefreshTokenAsync(
            IntegrationTestsWebFactory.WEB_CLIENT_ID,
            clientSecret: null,
            initial.RefreshToken);

        Assert.True(refreshed.IsSuccess, refreshed.RawResponse);
        Assert.NotNull(refreshed.AccessToken);

        JwtClaims claims = OidcTestHelper.ParseJwt(refreshed.AccessToken);

        Assert.Equal(EMAIL, claims.GetString("email"));
        Assert.Contains(AuthRoles.USER, claims.GetValues("role"), StringComparer.Ordinal);
    }

    [Fact]
    public async Task RefreshToken_AfterSecurityStampRotation_IsRejected()
    {
        OidcTestHelper oidc = await SeedUserAndLoginAsync();

        OidcTokenResponse initial = await oidc.ExecuteAuthorizationCodeFlowAsync(
            IntegrationTestsWebFactory.WEB_CLIENT_ID,
            clientSecret: null,
            SCOPE);

        Assert.True(initial.IsSuccess, initial.RawResponse);

        Guid accountId = await GetAccountIdAsync(EMAIL);
        await RotateSecurityStampAsync(accountId);

        OidcTokenResponse refreshed = await oidc.RefreshTokenAsync(
            IntegrationTestsWebFactory.WEB_CLIENT_ID,
            clientSecret: null,
            initial.RefreshToken!);

        Assert.False(refreshed.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, refreshed.StatusCode);
        Assert.Equal("invalid_grant", refreshed.Error);
    }

    [Fact]
    public async Task Authorize_AfterSecurityStampRotation_RedirectsToFrontendLogin()
    {
        OidcTestHelper oidc = await SeedUserAndLoginAsync();

        Guid accountId = await GetAccountIdAsync(EMAIL);
        await RotateSecurityStampAsync(accountId);

        string codeChallenge = OidcTestHelper.GenerateCodeChallenge(OidcTestHelper.GenerateCodeVerifier());

        HttpResponseMessage response = await oidc.AuthorizeAsync(
            IntegrationTestsWebFactory.WEB_CLIENT_ID,
            SCOPE,
            IntegrationTestsWebFactory.WEB_REDIRECT_URI,
            codeChallenge);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        string location = response.Headers.Location?.ToString() ?? string.Empty;
        Assert.StartsWith(IntegrationTestsWebFactory.FRONTEND_LOGIN_URL, location, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Authorize_LockedOutUser_RedirectsToFrontendLogin()
    {
        OidcTestHelper oidc = await SeedUserAndLoginAsync();

        Guid accountId = await GetAccountIdAsync(EMAIL);

        await using (AsyncServiceScope scope = Services.CreateAsyncScope())
        {
            UserManager<Account> userManager = scope.ServiceProvider.GetRequiredService<UserManager<Account>>();
            Account account = (await userManager.FindByIdAsync(accountId.ToString()))!;
            IdentityResult result = await userManager.SetLockoutEndDateAsync(account, DateTimeOffset.UtcNow.AddDays(1));
            Assert.True(result.Succeeded);
        }

        string codeChallenge = OidcTestHelper.GenerateCodeChallenge(OidcTestHelper.GenerateCodeVerifier());

        HttpResponseMessage response = await oidc.AuthorizeAsync(
            IntegrationTestsWebFactory.WEB_CLIENT_ID,
            SCOPE,
            IntegrationTestsWebFactory.WEB_REDIRECT_URI,
            codeChallenge);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        string location = response.Headers.Location?.ToString() ?? string.Empty;
        Assert.StartsWith(IntegrationTestsWebFactory.FRONTEND_LOGIN_URL, location, StringComparison.Ordinal);
    }

    private async Task<OidcTestHelper> SeedUserAndLoginAsync(string email = EMAIL)
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync(email, PASSWORD, USER_NAME);

        var oidc = new OidcTestHelper(Factory);
        await oidc.LoginAsync(email, PASSWORD);

        return oidc;
    }
}

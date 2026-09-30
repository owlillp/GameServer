using System.Net;
using System.Text.Json;
using AuthService.Domain;
using AuthService.IntegrationTests.Infrastructure;

namespace AuthService.IntegrationTests.Features.Connect;

public sealed class UserInfoTests : IntegrationTestsBase
{
    private const string EMAIL = "userinfo@test.com";
    private const string PASSWORD = "TestPass123!";
    private const string USER_NAME = "userinfo";
    private const string SCOPE = "openid profile email offline_access auth";

    private readonly OidcTestHelper _oidc;

    public UserInfoTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
        _oidc = new OidcTestHelper(factory);
    }

    [Fact]
    public async Task WithAccessToken_ReturnsUserClaims()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync(EMAIL, PASSWORD, USER_NAME);
        await _oidc.LoginAsync(EMAIL, PASSWORD);

        OidcTokenResponse tokens = await _oidc.ExecuteAuthorizationCodeFlowAsync(
            IntegrationTestsWebFactory.WEB_CLIENT_ID,
            clientSecret: null,
            SCOPE);

        Assert.True(tokens.IsSuccess, tokens.RawResponse);

        HttpResponseMessage response = await _oidc.GetUserInfoAsync(tokens.AccessToken!);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement root = document.RootElement;

        Assert.False(string.IsNullOrEmpty(root.GetProperty("sub").GetString()));
        Assert.Equal(EMAIL, root.GetProperty("email").GetString());
        Assert.Equal(USER_NAME, root.GetProperty("name").GetString());

        IReadOnlyList<string> roles = root.GetProperty("role")
            .EnumerateArray()
            .Select(role => role.GetString()!)
            .ToArray();

        Assert.Contains(AuthRoles.USER, roles, StringComparer.Ordinal);
    }

    [Fact]
    public async Task WithoutToken_IsUnauthorized()
    {
        await SeedPlatformConfigAsync();

        HttpResponseMessage response = await _oidc.GetUserInfoAsync(string.Empty);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

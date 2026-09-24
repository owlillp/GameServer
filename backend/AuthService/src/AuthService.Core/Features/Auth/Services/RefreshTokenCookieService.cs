using Microsoft.AspNetCore.Http;

namespace AuthService.Core.Features.Auth.Services;

public sealed class RefreshTokenCookieService(IHttpContextAccessor httpContextAccessor) : IRefreshTokenCookieService
{
    private const string COOKIE_NAME = "refresh_token";

    public string? GetRefreshToken()
    {
        HttpContext.Request.Cookies.TryGetValue(COOKIE_NAME, out string? refreshToken);
        return refreshToken;
    }

    public void Append(string refreshToken, DateTime expiresAt)
    {
        HttpContext.Response.Cookies.Append(
            COOKIE_NAME,
            refreshToken,
            CreateOptions(expiresAt));
    }

    public void Delete()
    {
        HttpContext.Response.Cookies.Delete(
            COOKIE_NAME,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = IsHttps(),
                SameSite = SameSiteMode.Lax,
                Path = GetCookiePath(),
            });
    }

    private HttpContext HttpContext =>
        httpContextAccessor.HttpContext
        ?? throw new InvalidOperationException("HTTP context is not available.");

    private CookieOptions CreateOptions(DateTime expiresAt) =>
        new()
        {
            HttpOnly = true,
            Secure = IsHttps(),
            SameSite = SameSiteMode.Lax,
            Expires = new DateTimeOffset(DateTime.SpecifyKind(expiresAt, DateTimeKind.Utc)),
            Path = GetCookiePath(),
        };

    private bool IsHttps() =>
        HttpContext.Request.IsHttps
        || string.Equals(
            HttpContext.Request.Headers["X-Forwarded-Proto"],
            "https",
            StringComparison.OrdinalIgnoreCase);

    private string GetCookiePath()
    {
        string prefix = HttpContext.Request.Headers["X-Forwarded-Prefix"].ToString().TrimEnd('/');
        return string.IsNullOrWhiteSpace(prefix) ? "/auth/jwt" : $"{prefix}/auth/jwt";
    }
}
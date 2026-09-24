using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Core.Configurations;
using AuthService.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Framework.Authentication;

namespace AuthService.Core.Features.Auth.Services;

public sealed class JwtTokenService(
    IOptions<JwtSettings> options,
    TimeProvider timeProvider) : IJwtTokenService
{
    private readonly JwtSettings _settings = options.Value;

    public JwtToken Generate(Account account, IReadOnlyCollection<string> roles)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var expiresAt = now.AddMinutes(_settings.AccessTokenLifetimeMinutes);

        var claims = new List<Claim>
        {
            new(AuthClaimTypes.SUB, account.Id.ToString()),
            new(AuthClaimTypes.NAME, account.UserName!),
            new(AuthClaimTypes.EMAIL, account.Email!),
            new(AuthClaimTypes.AUTH_METHOD, "jwt"),
        };

        claims.AddRange(roles.Select(role => new Claim(AuthClaimTypes.ROLE, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: credentials);

        string serialized = new JwtSecurityTokenHandler().WriteToken(token);
        return new JwtToken(serialized, expiresAt);
    }
}
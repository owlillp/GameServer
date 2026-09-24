using Shared.Core.Abstractions;

namespace AuthService.Core.Features.Auth.Queries.GetJwtSesionStatus;

public sealed record JwtSessionStatusQuery(string? RefreshToken) : IQuery;
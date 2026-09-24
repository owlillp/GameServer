using Shared.Core.Abstractions;

namespace AuthService.Core.Features.Auth.Commands.JwtLogout;

public sealed record JwtLogoutCommand(string? RefreshToken) : ICommand;
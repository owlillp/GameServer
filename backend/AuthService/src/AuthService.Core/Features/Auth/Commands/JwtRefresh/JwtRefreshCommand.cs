using Shared.Core.Abstractions;

namespace AuthService.Core.Features.Auth.Commands.JwtRefresh;

public sealed record JwtRefreshCommand(string? RefreshToken) : ICommand;
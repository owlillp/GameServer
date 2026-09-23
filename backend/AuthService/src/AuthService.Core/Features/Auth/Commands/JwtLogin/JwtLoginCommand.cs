using AuthService.Contracts.Requests;
using Shared.Core.Abstractions;

namespace AuthService.Core.Features.Auth.Commands.JwtLogin;

public sealed record JwtLoginCommand(LoginRequest Request) : ICommand;
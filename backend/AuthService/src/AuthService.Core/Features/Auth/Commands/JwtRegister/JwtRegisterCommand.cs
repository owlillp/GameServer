using AuthService.Contracts.Requests;
using Shared.Core.Abstractions;

namespace AuthService.Core.Features.Auth.Commands.JwtRegister;

public sealed record JwtRegisterCommand(RegisterRequest Request) : ICommand;
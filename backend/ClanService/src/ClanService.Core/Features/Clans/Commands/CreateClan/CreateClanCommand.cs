using ClanService.Contracts.Clans;
using Shared.Core.Abstractions;

namespace ClanService.Core.Features.Clans.Commands.CreateClan;

public sealed record CreateClanCommand(CreateClanRequest Request) : ICommand;

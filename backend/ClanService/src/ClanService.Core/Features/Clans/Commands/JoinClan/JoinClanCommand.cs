using Shared.Core.Abstractions;

namespace ClanService.Core.Features.Clans.Commands.JoinClan;

public sealed record JoinClanCommand(Guid ClanId) : ICommand;

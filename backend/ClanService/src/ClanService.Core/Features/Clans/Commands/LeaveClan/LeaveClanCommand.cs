using Shared.Core.Abstractions;

namespace ClanService.Core.Features.Clans.Commands.LeaveClan;

public sealed record LeaveClanCommand(Guid ClanId) : ICommand;

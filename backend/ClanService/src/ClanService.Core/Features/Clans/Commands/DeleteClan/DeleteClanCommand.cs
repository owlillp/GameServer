using Shared.Core.Abstractions;

namespace ClanService.Core.Features.Clans.Commands.DeleteClan;

public sealed record DeleteClanCommand(Guid ClanId) : ICommand;

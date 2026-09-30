using Shared.Core.Abstractions;

namespace ClanService.Core.Features.Clans.Commands.KickMember;

public sealed record KickMemberCommand(Guid ClanId, Guid UserId) : ICommand;

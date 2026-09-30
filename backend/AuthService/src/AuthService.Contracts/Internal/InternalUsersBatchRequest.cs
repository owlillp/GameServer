namespace AuthService.Contracts.Internal;

public sealed record InternalUsersBatchRequest(IReadOnlyList<Guid> UserIds);

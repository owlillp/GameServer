using Shared.Core.Abstractions;

namespace AuthService.Core.Features.InternalUsers.Queries.GetUsersBatch;

public sealed record GetUsersBatchQuery(IReadOnlyList<Guid> UserIds) : IQuery;

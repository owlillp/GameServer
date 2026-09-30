using AuthService.Contracts.Internal;
using AuthService.Core.Abstractions;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Abstractions;
using Shared.SharedKernel.Errors;

namespace AuthService.Core.Features.InternalUsers.Queries.GetUsersBatch;

public sealed class GetUsersBatchHandler(IReadDbContext readDbContext)
    : IQueryHandlerWithResult<IReadOnlyList<AuthUserLookupDto>, GetUsersBatchQuery>
{
    private const int MAX_USERS_PER_REQUEST = 500;

    public async Task<Result<IReadOnlyList<AuthUserLookupDto>, Error>> Handle(
        GetUsersBatchQuery query,
        CancellationToken cancellationToken = new ())
    {
        if (query.UserIds.Count == 0)
        {
            return Result.Success<IReadOnlyList<AuthUserLookupDto>, Error>([]);
        }

        if (query.UserIds.Count > MAX_USERS_PER_REQUEST)
        {
            return GeneralErrors.ValueIsInvalid("userIds");
        }

        List<AuthUserLookupDto> users = await readDbContext.AccountsRead
            .Where(account => query.UserIds.Contains(account.Id))
            .Select(account => new AuthUserLookupDto(
                account.Id,
                account.DisplayName,
                account.UserName,
                account.Email))
            .ToListAsync(cancellationToken);

        return users;
    }
}

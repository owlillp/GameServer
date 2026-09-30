using AuthService.Contracts.HttpCommunication;
using AuthService.Contracts.Internal;
using CSharpFunctionalExtensions;
using Shared.SharedKernel.Errors;

namespace ClanService.IntegrationTests.Infrastructure;

public sealed class FakeAuthServiceClient : IAuthServiceClient
{
    private readonly Dictionary<Guid, AuthUserLookupDto> _users = [];

    public FakeAuthServiceClient AddUser(Guid userId, string? name, string? userName, string? email)
    {
        _users[userId] = new AuthUserLookupDto(userId, name, userName, email);
        return this;
    }

    public Task<Result<IReadOnlyList<AuthUserLookupDto>, Error>> GetUsersByIdsAsync(
        IReadOnlyList<Guid> userIds,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<AuthUserLookupDto> users = userIds
            .Where(_users.ContainsKey)
            .Select(userId => _users[userId])
            .ToList();

        return Task.FromResult(Result.Success<IReadOnlyList<AuthUserLookupDto>, Error>(users));
    }
}

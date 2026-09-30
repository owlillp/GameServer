using AuthService.Contracts.Internal;
using CSharpFunctionalExtensions;
using Shared.SharedKernel.Errors;

namespace AuthService.Contracts.HttpCommunication;

public interface IAuthServiceClient
{
    Task<Result<IReadOnlyList<AuthUserLookupDto>, Error>> GetUsersByIdsAsync(
        IReadOnlyList<Guid> userIds,
        CancellationToken cancellationToken);
}

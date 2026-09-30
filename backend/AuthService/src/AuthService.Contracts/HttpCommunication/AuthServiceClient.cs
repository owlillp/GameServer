using AuthService.Contracts.Internal;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Shared.Core.HttpCommunication;
using Shared.SharedKernel.Errors;

namespace AuthService.Contracts.HttpCommunication;

internal sealed class AuthServiceClient(
    HttpClient httpClient,
    ILogger<AuthServiceClient> logger)
    : BaseHttpClient(httpClient, logger, "AuthService"), IAuthServiceClient
{
    public Task<Result<IReadOnlyList<AuthUserLookupDto>, Error>> GetUsersByIdsAsync(
        IReadOnlyList<Guid> userIds,
        CancellationToken cancellationToken)
        => PostAsync<InternalUsersBatchRequest, IReadOnlyList<AuthUserLookupDto>>(
            "/internal/users/batch",
            new InternalUsersBatchRequest(userIds),
            cancellationToken);
}

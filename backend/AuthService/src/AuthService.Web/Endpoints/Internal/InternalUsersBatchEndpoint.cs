using AuthService.Contracts.Internal;
using AuthService.Core.Features.InternalUsers.Queries.GetUsersBatch;
using AuthService.Domain;
using Microsoft.AspNetCore.Mvc;
using Shared.Framework.Authorization;
using Shared.Framework.Endpoints;

namespace AuthService.Web.Endpoints.Internal;

/// <summary>
/// Internal API для других сервисов (service-to-service). Наружу через nginx
/// не публикуется: доступен только из compose-сети, только под service/admin ролью.
/// </summary>
public sealed class InternalUsersBatchEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/internal/users/batch",
                async Task<EndpointResult<IReadOnlyList<AuthUserLookupDto>>> (
                        [FromBody] InternalUsersBatchRequest request,
                        [FromServices] GetUsersBatchHandler handler,
                        CancellationToken ct) =>
                    await handler.Handle(new GetUsersBatchQuery(request.UserIds), ct))
            .RequireAnyRole(AuthRoles.SERVICE, AuthRoles.ADMIN);
}

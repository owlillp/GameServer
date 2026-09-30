using AuthService.Contracts.Admin;
using AuthService.Core.Features.Admin.Queries.ListUsers;
using AuthService.Domain;
using Microsoft.AspNetCore.Mvc;
using Shared.Framework.Authorization;
using Shared.Framework.Endpoints;
using Shared.SharedKernel.Responses;

namespace AuthService.Web.Endpoints.Admin;

public sealed class AdminUsersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/auth/admin/users",
                async Task<EndpointResult<PaginationResponse<AdminUserDto>>> (
                        [FromQuery] int page,
                        [FromQuery] int pageSize,
                        [FromQuery] string? search,
                        [FromServices] ListUsersHandler handler,
                        CancellationToken ct) =>
                    await handler.Handle(new ListUsersQuery(page, pageSize, search), ct))
            .RequirePermissions(AuthPermissions.Users.VIEW);
}

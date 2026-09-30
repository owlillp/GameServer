using AuthService.Contracts.Dtos;
using AuthService.Core.Abstractions;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Abstractions;
using Shared.Framework.Authentication.UserScope;
using Shared.SharedKernel.Errors;

namespace AuthService.Core.Features.Auth.Queries.GetMyProfile;

public sealed class GetMyProfileHandler(
    IReadDbContext readDbContext,
    UserScopedData currentUser) : IQueryHandlerWithResult<ProfileDto, GetMyProfileQuery>
{
    public async Task<Result<ProfileDto, Error>> Handle(
        GetMyProfileQuery query,
        CancellationToken cancellationToken = new ())
    {
        Guid? userId = currentUser.Id;
        if (userId is null)
        {
            return Error.Authentication("authentication.required", "User identity is not available");
        }

        var account = await readDbContext
            .AccountsRead
            .FirstOrDefaultAsync(a => a.Id == userId, cancellationToken);

        if (account == null)
        {
            return GeneralErrors.NotFound(userId, entityName: "user");
        }

        var profileBodyDto = new ProfileBodyDto(
            account.Profile.Age,
            account.Profile.Bio,
            account.Profile.Location);

        return new ProfileDto(
            account.Id,
            account.Email,
            account.UserName,
            account.DisplayName,
            account.CreatedAt,
            account.UpdatedAt,
            profileBodyDto);
    }
}
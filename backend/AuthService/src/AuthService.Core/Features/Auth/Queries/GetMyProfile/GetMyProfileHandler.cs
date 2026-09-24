using AuthService.Contracts.Dtos;
using AuthService.Core.Abstractions;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Abstractions;
using Shared.Framework.Authentication;
using Shared.SharedKernel.Errors;

namespace AuthService.Core.Features.Auth.Queries.GetMyProfile;

public sealed class GetMyProfileHandler(
    IReadDbContext readDbContext,
    CurrentUser currentUser) : IQueryHandlerWithResult<ProfileDto, GetMyProfileQuery>
{
    public async Task<Result<ProfileDto, Error>> Handle(
        GetMyProfileQuery query,
        CancellationToken cancellationToken = new ())
    {
        var userId = currentUser.RequireId();

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
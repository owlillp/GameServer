using Shared.SharedKernel.Errors;

namespace AuthService.Domain;

public static class RefreshSessionErrors
{
    public static Error NotFound()
        => Error.NotFound("refresh_session.not_found", "Refresh-сессия не найдена");

    public static Error Invalid()
        => Error.Authentication("refresh_token.invalid", "Refresh-токен недействителен");
}
namespace AuthService.Core.Configurations;

public sealed class AuthServiceOptions
{
    public const string SECTION_NAME = "AuthService";

    public string? FrontendLoginUrl { get; init; }
}

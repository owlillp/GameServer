namespace AuthService.Core.Configurations;

public sealed class IdentitySettings
{
    public const string SECTION_NAME = "Identity";

    public PasswordSettings Password { get; set; } = new();

    public LockoutSettings Lockout { get; set; } = new();

    public UserSettings User { get; set; } = new();

    public SignInSettings SignIn { get; set; } = new();
}

public sealed class PasswordSettings
{
    public int RequiredLength { get; set; } = 8;

    public bool RequireDigit { get; set; } = true;

    public bool RequireLowercase { get; set; } = true;

    public bool RequireUppercase { get; set; } = true;

    public bool RequireNonAlphanumeric { get; set; }
}

public sealed class LockoutSettings
{
    public int MaxFailedAccessAttempts { get; set; } = 5;

    public TimeSpan DefaultLockoutTimeSpan { get; set; } = TimeSpan.FromMinutes(15);
}

public sealed class UserSettings
{
    public bool RequireUniqueEmail { get; set; } = true;
}

public sealed class SignInSettings
{
    public bool RequireConfirmedEmail { get; set; }
}
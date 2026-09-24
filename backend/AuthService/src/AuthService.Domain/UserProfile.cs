namespace AuthService.Domain;

public sealed record UserProfile(int? Age, string? Bio, string? Location)
{
    public static UserProfile Empty { get; } = new(null, null, null);
}

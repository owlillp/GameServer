namespace AuthService.Core.Configurations;

public static class OidcScopes
{
    public const string OPEN_ID = "openid";
    public const string PROFILE = "profile";
    public const string EMAIL = "email";
    public const string OFFLINE_ACCESS = "offline_access";
    public const string AUTH = "auth";

    public const string AUTH_RESOURCE = "auth-service";

    private static readonly Dictionary<string, string> ResourcesByScope =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [AUTH] = AUTH_RESOURCE,
        };

    public static readonly IReadOnlySet<string> All = new HashSet<string>(
        [
            OPEN_ID,
            PROFILE,
            EMAIL,
            OFFLINE_ACCESS,
            AUTH
        ],
        StringComparer.Ordinal);

    public static IReadOnlyList<string> GetResources(IEnumerable<string> scopes)
    {
        ArgumentNullException.ThrowIfNull(scopes);

        var resources = new List<string>();

        foreach (string scope in scopes)
        {
            if (ResourcesByScope.TryGetValue(scope, out string? resource))
            {
                resources.Add(resource);
            }
        }

        return resources;
    }
}

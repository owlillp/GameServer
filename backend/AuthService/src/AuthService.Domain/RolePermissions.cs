namespace AuthService.Domain;

public static class RolePermissions
{
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> Map =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
        {
            [AuthRoles.USER] = [],

            [AuthRoles.MODERATOR] = [AuthPermissions.Users.VIEW, AuthPermissions.Users.MANAGE, AuthPermissions.Platform.MANAGE],

            [AuthRoles.ADMIN] = AuthPermissions.All,

            //[PlatformRoles.SERVICE_ACCOUNT] = AuthPermissions.All,
        };

    public static IReadOnlyList<string> ForRole(string role)
        => Map.TryGetValue(role, out IReadOnlyList<string>? value) ? value : [];

    public static IReadOnlyCollection<string> ForRoles(IEnumerable<string> roles)
    {
        var permissions = new HashSet<string>();
        foreach (string role in roles)
        {
            foreach (string permission in ForRole(role))
            {
                permissions.Add(permission);
            }
        }
        return permissions;
    }
}
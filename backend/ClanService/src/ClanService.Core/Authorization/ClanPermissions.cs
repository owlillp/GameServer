namespace ClanService.Core.Authorization;

public static class ClanPermissions
{
    public const string CLANS_MANAGE = "clans.manage";

    // Только Admin: модерация состава кланов (исключение участников).
    public const string CLANS_ADMIN = "clans.admin";
}

// Имена ролей совпадают с claim "role" в access-токенах AuthService.
public static class ClanAuthRoles
{
    public const string USER = "User";
    public const string MODERATOR = "Moderator";
    public const string ADMIN = "Admin";
    public const string SERVICE = "Service";
}

public static class ClanRolePermissions
{
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> Map =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
        {
            [ClanAuthRoles.USER] = [],
            [ClanAuthRoles.MODERATOR] = [ClanPermissions.CLANS_MANAGE],
            [ClanAuthRoles.ADMIN] = [ClanPermissions.CLANS_MANAGE, ClanPermissions.CLANS_ADMIN],
            [ClanAuthRoles.SERVICE] = [],
        };
}

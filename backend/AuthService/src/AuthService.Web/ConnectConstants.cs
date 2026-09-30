namespace AuthService.Web;

public static class ConnectConstants
{
    public const string AUTHORIZE_ENDPOINT = "/connect/authorize";
    public const string TOKEN_ENDPOINT = "/connect/token";
    public const string REVOKE_ENDPOINT = "/connect/revoke";
    public const string USER_INFO_ENDPOINT = "/connect/userinfo";

    public static class Scopes
    {
        public const string OTHER_SCOPE = "";
    }
}
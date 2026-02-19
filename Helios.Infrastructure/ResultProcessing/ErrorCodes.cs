
namespace Helios.Infrastructure.ResultProcessing;

public static class ErrorCodes
{
    public static class Auth
    {
        public const string InvalidCredentials = "auth.invalid_credentials";
        public const string UserInactive = "auth.user_inactive";
    }

    public static class Tenants
    {
        public const string NotFound = "tenants.not_found";
    }

    public static class Common
    {
        public const string Unexpected = "common.unexpected";
        public const string ValidationFailed = "common.validation_failed";
        public const string Forbidden = "common.forbidden";
    }
}


namespace Helios.Infrastructure.ResultProcessing;

public static class ErrorCodes
{
    public static class Auth
    {
        public const string InvalidCredentials = "auth.invalid_credentials";
        public const string UserInactive = "auth.user_inactive";
        public const string Unauthorized = "auth.unauthorized"; 
        public const string TokenInvalid = "auth.token_invalid";
        public const string TokenExpired = "auth.token_expired";
    }

    public static class Tenants
    {
        public const string NotFound = "tenants.not_found";
        public const string Forbidden = "tenants.forbidden";
    }
    
    public static class Agents
    {
        public const string EnrollmentTokenInvalid = "agents.enrollment_token_invalid";
        public const string EnrollmentTokenExpired = "agents.enrollment_token_expired";
        public const string EnrollmentTokenConsumed = "agents.enrollment_token_consumed";
        public const string ServerNotFound = "agents.server_not_found";
        public const string AlreadyEnrolled = "agents.already_enrolled";
        public const string DuplicateAccessKey = "agents.duplicate_access_key";
    }

    public static class Common
    {
        public const string Unexpected = "common.unexpected";
        public const string ValidationFailed = "common.validation_failed";
        public const string Forbidden = "common.forbidden";
        public const string Unauthorized = "common.unauthorized"; 
        public const string NotFound = "common.not_found";         
        public const string Conflict = "common.conflict";         
        public const string Cancelled = "common.cancelled";       
        public const string Unavailable = "common.unavailable"; 
    }
}

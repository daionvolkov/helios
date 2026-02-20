namespace Helios.Platform.Common;

public static class DomainErrorCodes
{
    public static class PlatformServers
    {
        public const string NotFound = "platform.servers.not_found";
        public const string NameConflict = "platform.servers.name_conflict";
        public const string ValidationFailed = "platform.servers.validation_failed";
        public const string Unexpected = "platform.servers.unexpected";
    }


    public static class Agents
    {
        public const string TokenInvalid = "agents.enrollment.token_invalid";
        public const string TokenExpired = "agents.enrollment.token_expired";
        public const string TokenAlreadyUsed = "agents.enrollment.token_used";
        public const string ServerNotFound = "agents.server_not_found";
        public const string AgentNotFound = "agents.agent_not_found";
        public const string ValidationFailed = "agents.validation_failed";
        public const string Unexpected = "agents.unexpected";
    }
}

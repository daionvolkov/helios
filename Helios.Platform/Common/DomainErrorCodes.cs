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
}

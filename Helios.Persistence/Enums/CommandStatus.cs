namespace Helios.Persistence.Enums;

public enum CommandStatus
{
    Queued,
    Dispatched,
    Acked,
    Running,
    Succeeded,
    Failed,
    TimedOut,
    Canceled
}

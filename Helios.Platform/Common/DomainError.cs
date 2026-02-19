namespace Helios.Platform.Common;

public sealed record DomainError(
    string Code,
    string Message,
    DomainErrorKind Kind
);
    
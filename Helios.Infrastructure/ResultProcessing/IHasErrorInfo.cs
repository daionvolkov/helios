namespace Helios.Infrastructure.ResultProcessing;

public interface IHasErrorInfo
{
    string Code { get; }
    string Message { get; }
    string? Kind { get; } 
}
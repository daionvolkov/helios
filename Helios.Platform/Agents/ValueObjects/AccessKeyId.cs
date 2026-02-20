namespace Helios.Platform.Agents.ValueObjects;

public sealed class AccessKeyId
{
    public string Value { get; }
    private AccessKeyId(string value)
    {
        Value = value;
    }

    public static AccessKeyId Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("AccessKeyId cannot be empty");

        if (!value.StartsWith("agt_"))
            throw new ArgumentException("AccessKeyId must start with agt_");

        return new AccessKeyId(value);
    }

    public override string ToString() => Value;
}

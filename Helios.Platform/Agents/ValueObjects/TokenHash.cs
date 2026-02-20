namespace Helios.Platform.Agents.ValueObjects;

public sealed class TokenHash
{
    public string Value { get; }

    private TokenHash(string value)
    {
        Value = value;
    }

    public static TokenHash FromHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Token hash cannot be empty");

        return new TokenHash(hash);
    }

    public override string ToString() => Value;
}

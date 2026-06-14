namespace Blabber.Lib;

public sealed class BlabberException : Exception
{
    public BlabberException(string message)
        : base(message)
    {
    }

    public BlabberException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

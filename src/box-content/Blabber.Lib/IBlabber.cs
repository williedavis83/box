namespace Blabber.Lib;

public interface IBlabber
{
    Task<string> Bar(CancellationToken cancellationToken = default);

    Task<string> Baz(CancellationToken cancellationToken = default);
}

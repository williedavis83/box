using BoxBottom.Auth.Contract.Models;

namespace BoxBottom.Auth.Contract;

public interface IZeroAuthRequestValidator
{
    string? Validate(ZeroAuthLoginRequest request);
}

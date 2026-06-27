namespace BoxBottom.Auth.Contract;

public interface ICurrentUserAccessor
{
    Guid? UserId { get; }

    bool IsAuthenticated { get; }
}

namespace BoxBottom.Users.Contract;

public interface IExternalUserRepository<TExternalUser>
{
    Task<Guid> GetOrCreateInternalUserIdAsync(
        TExternalUser externalUser,
        CancellationToken cancellationToken = default);
}

namespace BoxBottom.Auth.Business;

public sealed class AuthProviderMismatchException(string message) : InvalidOperationException(message);

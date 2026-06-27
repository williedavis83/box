using BoxBottom.Auth.Business;
using BoxBottom.Auth.Contract;
using BoxBottom.Auth.Contract.Models;
using BoxBottom.Auth.Entra;
using BoxBottom.Auth.ZeroAuth;
using BoxBottom.Users.Contract.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BoxBottom.Auth.Controllers;

[ApiController]
[Route("api/Auth")]
public abstract class AuthControllerBase<TProfile> : ControllerBase
    where TProfile : BaseUserProfile
{
    [HttpGet("config")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetConfig() =>
        Ok(new { provider = AuthProviderGuard.ActiveProviderName });

    [HttpGet("login")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Login([FromQuery] string? returnUrl = null)
    {
        try
        {
            AuthProviderGuard.EnsureProviderActive(EntraAuthProvider.Name);
        }
        catch (AuthProviderMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }

        var properties = new AuthenticationProperties
        {
            RedirectUri = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl,
        };

        return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpPost("zero/login")]
    [ProducesResponseType(typeof(AuthSessionInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ZeroAuthLogin(
        [FromBody] ZeroAuthLoginRequest request,
        [FromServices] IAuthService<TProfile> authService,
        [FromServices] IZeroAuthRequestValidator validator,
        CancellationToken cancellationToken)
    {
        try
        {
            AuthProviderGuard.EnsureProviderActive(ZeroAuthAuthProvider.Name);
        }
        catch (AuthProviderMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }

        if (request is null)
        {
            return BadRequest(new { message = "Request body is required." });
        }

        var validationError = validator.Validate(request);
        if (validationError is not null)
        {
            return BadRequest(new { message = validationError });
        }

        var session = await authService.SignInAsync(
            new Users.Contract.Models.ExternalUser(request.Provider, request.ExternalId),
            new ProfileSeed(request.DisplayName, request.Email),
            cancellationToken);

        return Ok(session);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        [FromServices] IAuthService<TProfile> authService,
        CancellationToken cancellationToken)
    {
        await authService.SignOutAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(AuthSessionInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSession(
        [FromServices] IAuthService<TProfile> authService,
        CancellationToken cancellationToken)
    {
        var session = await authService.GetSessionAsync(cancellationToken);
        return session is null ? Unauthorized() : Ok(session);
    }

    protected abstract IAuthProviderGuard AuthProviderGuard { get; }
}

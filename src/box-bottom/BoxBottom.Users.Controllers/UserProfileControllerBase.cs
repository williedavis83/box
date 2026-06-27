using BoxBottom.Azure.Table;
using BoxBottom.Azure.Table.Models;
using BoxBottom.Auth.Contract;
using BoxBottom.Users.Contract;
using BoxBottom.Users.Contract.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BoxBottom.Users.Controllers;

[ApiController]
[Route("api/UserProfile")]
public abstract class UserProfileControllerBase<TProfile, TUpdateRequest>(
    IUserProfileService<TProfile, TUpdateRequest> userProfileService,
    IAzureTableService usersTableService,
    ICurrentUserAccessor currentUserAccessor) : ControllerBase
    where TProfile : BaseUserProfile
    where TUpdateRequest : BaseUpdateUserProfileRequest
{
    [HttpGet("status")]
    [ProducesResponseType<AzureTableConfigurationStatus>(StatusCodes.Status200OK)]
    public ActionResult<AzureTableConfigurationStatus> GetStatus() =>
        Ok(usersTableService.GetConfigurationStatus());

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(BaseUserProfile), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        if (!TryEnsureConfigured(out var notConfiguredResult))
        {
            return notConfiguredResult;
        }

        if (currentUserAccessor.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        var profile = await userProfileService.GetAsync(userId, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut("me")]
    [Authorize]
    [ProducesResponseType(typeof(BaseUserProfile), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UpdateMyProfile(
        [FromBody] TUpdateRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryEnsureConfigured(out var notConfiguredResult))
        {
            return notConfiguredResult;
        }

        if (currentUserAccessor.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        try
        {
            var profile = await userProfileService.UpdateAsync(userId, request, cancellationToken);
            return Ok(profile);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(BaseUserProfile), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetProfile(Guid userId, CancellationToken cancellationToken)
    {
        if (!TryEnsureConfigured(out var notConfiguredResult))
        {
            return notConfiguredResult;
        }

        var profile = await userProfileService.GetAsync(userId, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BaseUserProfile), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CreateProfile(
        [FromBody] CreateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryEnsureConfigured(out var notConfiguredResult))
        {
            return notConfiguredResult;
        }

        var profile = await userProfileService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetProfile), new { userId = profile.UserId }, profile);
    }

    [HttpPut("{userId:guid}")]
    [ProducesResponseType(typeof(BaseUserProfile), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UpdateProfile(
        Guid userId,
        [FromBody] TUpdateRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryEnsureConfigured(out var notConfiguredResult))
        {
            return notConfiguredResult;
        }

        try
        {
            var profile = await userProfileService.UpdateAsync(userId, request, cancellationToken);
            return Ok(profile);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private bool TryEnsureConfigured(out IActionResult notConfiguredResult)
    {
        var status = usersTableService.GetConfigurationStatus();
        if (status.IsConfigured)
        {
            notConfiguredResult = null!;
            return true;
        }

        notConfiguredResult = StatusCode(
            StatusCodes.Status503ServiceUnavailable,
            new
            {
                message = "Azure Table storage is not configured for user profiles.",
                status,
            });

        return false;
    }
}

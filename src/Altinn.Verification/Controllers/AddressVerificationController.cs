using System.ComponentModel.DataAnnotations;

using Altinn.Verification.Authorization;
using Altinn.Verification.Core;
using Altinn.Verification.Core.AddressVerifications;
using Altinn.Verification.Core.AddressVerifications.Models;
using Altinn.Verification.Models.AddressVerification;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Altinn.Verification.Controllers;

/// <summary>
/// Controller for logic concerning verification of notification addresses.
/// </summary>
[Authorize]
[Authorize(Policy = AuthConstants.PortalEndUserAccess)]
[Route("verification/api/v1/users/current/")]
[Consumes("application/json")]
[Produces("application/json")]
public class AddressVerificationController : ControllerBase
{
    private readonly IAddressVerificationService _addressVerificationService;
    private readonly int _verificationCodeCooldownPeriodInSeconds;
    private const string _verificationCodeNotSentMessage = "Verification code could not be sent";
    private const string _remainingSecondsBodyName = "retryAfterSeconds";
    private const string _retryAfterHeaderName = "Retry-After";

    /// <summary>
    /// Initializes a new instance of the <see cref="AddressVerificationController"/> class.
    /// </summary>
    /// <param name="addressVerificationService">The address verification service.</param>
    /// <param name="addressMaintenanceSettings">The address maintenance settings.</param>
    public AddressVerificationController(IAddressVerificationService addressVerificationService, IOptions<AddressMaintenanceSettings> addressMaintenanceSettings)
    {
        _addressVerificationService = addressVerificationService;
        _verificationCodeCooldownPeriodInSeconds = addressMaintenanceSettings.Value.VerificationCodeResendCooldownSeconds;
    }

    /// <summary>
    /// Get all verified addresses for the current user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A list of verified addresses.</returns>
    [HttpGet("verified-addresses")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<VerifiedAddressResponse>>> Get(CancellationToken cancellationToken)
    {
        var validationResult = ClaimsHelper.TryGetUserIdFromClaims(Request.HttpContext, out int userId);
        if (validationResult != null)
        {
            return validationResult;
        }

        var verifiedAddresses = await _addressVerificationService.GetVerifiedAddressesAsync(userId, cancellationToken);
        var response = verifiedAddresses.Select(va => new VerifiedAddressResponse { Type = va.AddressType, Value = va.Address });

        return Ok(response);
    }

    /// <summary>
    /// Verify an address for the current user by providing the verification code sent to the address.
    /// </summary>
    /// <param name="request">The code to verify, the address type, and the address value.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>No content on success, or a problem details object on failure.</returns>
    [HttpPost("verify")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Verify([FromBody][Required] AddressVerificationRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var validationResult = ClaimsHelper.TryGetUserIdFromClaims(Request.HttpContext, out int userId);
        if (validationResult != null)
        {
            return validationResult;
        }

        var verified = await _addressVerificationService.SubmitVerificationCodeAsync(userId, request.Value, (AddressType)request.Type!, request.VerificationCode, cancellationToken);

        if (!verified)
        {
            return UnprocessableEntity(new ProblemDetails { Title = "Address could not be verified", Detail = "The given verification code does not validate for the given address." });
        }

        return NoContent();
    }

    /// <summary>
    /// Starts the verification process for the current user and the given address by generating a code and sending it.
    /// If a code is already within its cooldown period, the remaining cooldown is returned in the response.
    /// </summary>
    /// <param name="request">The address type and value to send a code for.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>No content on success, or a problem details object describing the failure.</returns>
    /// <response code="204">Verification code was successfully generated and sent.</response>
    /// <response code="400">Request was malformed.</response>
    /// <response code="403">User is not authenticated.</response>
    /// <response code="422">The address is already verified for this user.</response>
    /// <response code="429">A code was recently sent and is still within its cooldown period.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost("send")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Send([FromBody][Required] AddressCodeSendRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var validationResult = ClaimsHelper.TryGetUserIdFromClaims(Request.HttpContext, out int userId);
        if (validationResult != null)
        {
            return validationResult;
        }

        var sendResult = await _addressVerificationService.SendVerificationCodeAsync(userId, request.Value, (AddressType)request.Type!, cancellationToken);

        return sendResult.Status switch
        {
            SendVerificationStatus.Success => new NoContentResult(),
            SendVerificationStatus.NotificationOrderFailed => InternalServerError(new ProblemDetails { Title = _verificationCodeNotSentMessage, Detail = "The verification process was created, but notification delivery failed." }),
            SendVerificationStatus.AddressAlreadyVerified => UnprocessableEntity(new ProblemDetails { Title = _verificationCodeNotSentMessage, Detail = "The address is already verified for this user." }),
            SendVerificationStatus.CodeCooldown => TooManyRequests(new ProblemDetails { Title = _verificationCodeNotSentMessage, Detail = $"Code resending attempts for an address are limited to 1 request per {_verificationCodeCooldownPeriodInSeconds} seconds. Please wait {sendResult.Cooldown}s before requesting a new code." }, sendResult.Cooldown),
            _ => InternalServerError(new ProblemDetails { Title = _verificationCodeNotSentMessage, Detail = "An unexpected error occurred." })
        };
    }

    private ObjectResult TooManyRequests(ProblemDetails problemDetails, int? retryAfter = null)
    {
        if (retryAfter.HasValue)
        {
            Response.Headers[_retryAfterHeaderName] = retryAfter.Value.ToString();
            problemDetails.Extensions[_remainingSecondsBodyName] = retryAfter.Value;
        }

        return StatusCode(StatusCodes.Status429TooManyRequests, problemDetails);
    }

    private ObjectResult InternalServerError(ProblemDetails problemDetails)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, problemDetails);
    }
}

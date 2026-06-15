using AltinnCore.Authentication.Constants;

using Microsoft.AspNetCore.Mvc;

namespace Altinn.Verification.Authorization;

/// <summary>
/// Helper class for working with claims in the HTTP context.
/// </summary>
public static class ClaimsHelper
{
    /// <summary>
    /// Retrieves the user identifier from the claims in the specified HTTP context as a string.
    /// </summary>
    /// <param name="context">The HTTP context containing the user claims.</param>
    /// <returns>A string representing the user identifier if present in the claims; otherwise, null.</returns>
    public static string? GetUserIdAsString(HttpContext context)
    {
        return context.User.Claims
            .Where(c => c.Type == AltinnCoreClaimTypes.UserId)
            .Select(c => c.Value).FirstOrDefault();
    }

    /// <summary>
    /// Attempts to retrieve the user ID from the claims in the provided HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context containing the claims.</param>
    /// <param name="userId">The user ID retrieved from the claims, if successful.</param>
    /// <returns>
    /// A <see cref="BadRequestObjectResult"/> if the user ID is missing or invalid; otherwise, null.
    /// </returns>
    public static BadRequestObjectResult? TryGetUserIdFromClaims(HttpContext context, out int userId)
    {
        userId = 0;
        string? userIdString = GetUserIdAsString(context);

        if (string.IsNullOrEmpty(userIdString))
        {
            return new BadRequestObjectResult("Invalid request context. UserId must be provided in claims.");
        }

        if (!int.TryParse(userIdString, out userId))
        {
            return new BadRequestObjectResult("Invalid user ID format in claims.");
        }

        return null;
    }
}

using System.Security.Claims;

using AltinnCore.Authentication.Constants;

namespace Altinn.Verification.Tests.IntegrationTests.Utils;

/// <summary>
/// Utility class for generating test principals and JWT tokens.
/// </summary>
public static class PrincipalUtil
{
    /// <summary>
    /// Generates a JWT token for a user with the portal end-user scope.
    /// </summary>
    /// <param name="userId">The user ID to include in the token claims.</param>
    /// <param name="authenticationLevel">The authentication level.</param>
    /// <param name="scope">The scope claim value.</param>
    /// <returns>A signed JWT token string.</returns>
    public static string GetToken(int userId, int authenticationLevel = 2, string scope = "altinn:portal/enduser")
    {
        List<Claim> claims = [];
        string issuer = "www.altinn.no";
        claims.Add(new Claim(AltinnCoreClaimTypes.UserId, userId.ToString(), ClaimValueTypes.String, issuer));
        claims.Add(new Claim(AltinnCoreClaimTypes.UserName, "UserOne", ClaimValueTypes.String, issuer));
        claims.Add(new Claim(AltinnCoreClaimTypes.PartyID, userId.ToString(), ClaimValueTypes.Integer32, issuer));
        claims.Add(new Claim(AltinnCoreClaimTypes.AuthenticateMethod, "Mock", ClaimValueTypes.String, issuer));
        claims.Add(new Claim(AltinnCoreClaimTypes.AuthenticationLevel, authenticationLevel.ToString(), ClaimValueTypes.Integer32, issuer));

        if (scope != null)
        {
            claims.Add(new Claim("scope", scope, ClaimValueTypes.String, "maskinporten"));
        }

        ClaimsIdentity identity = new("mock");
        identity.AddClaims(claims);

        ClaimsPrincipal principal = new(identity);
        return JwtGenerator.GenerateToken(principal, new TimeSpan(1, 1, 1));
    }

    /// <summary>
    /// Generates a JWT token for a system user (no userId, no scope — will be rejected by scope policy).
    /// </summary>
    /// <param name="systemUserId">The system user GUID.</param>
    /// <returns>A signed JWT token string.</returns>
    public static string GetSystemUserToken(Guid systemUserId)
    {
        string issuer = "www.altinn.no";
        string systemUser = $"{{\"systemuser_id\":[\"{systemUserId}\"],\"systemuser_org\":{{\"ID\":\"myOrg\"}},\"system_id\":\"the_matrix\"}}";

        List<Claim> claims = [];
        claims.Add(new Claim("authorization_details", systemUser, ClaimValueTypes.String, issuer));
        claims.Add(new Claim(AltinnCoreClaimTypes.AuthenticateMethod, "Mock", ClaimValueTypes.String, issuer));
        claims.Add(new Claim(AltinnCoreClaimTypes.AuthenticationLevel, "3", ClaimValueTypes.Integer32, issuer));

        ClaimsIdentity identity = new("mock");
        identity.AddClaims(claims);

        ClaimsPrincipal principal = new(identity);
        return JwtGenerator.GenerateToken(principal, new TimeSpan(1, 1, 1));
    }
}

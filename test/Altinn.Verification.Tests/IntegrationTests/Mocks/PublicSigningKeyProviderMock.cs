using System.Security.Cryptography.X509Certificates;

using Altinn.Common.AccessToken.Services;

using Microsoft.IdentityModel.Tokens;

namespace Altinn.Verification.Tests.IntegrationTests.Mocks;

/// <summary>
/// Mock for <see cref="IPublicSigningKeyProvider"/> used in integration tests.
/// Loads signing keys from PEM certificate files on disk.
/// </summary>
public class PublicSigningKeyProviderMock : IPublicSigningKeyProvider
{
    /// <inheritdoc />
    public Task<IEnumerable<SecurityKey>> GetSigningKeys(string issuer)
    {
        List<SecurityKey> signingKeys = [];

        X509Certificate2 cert = X509CertificateLoader.LoadCertificateFromFile($"{issuer}-org.pem");
        SecurityKey key = new X509SecurityKey(cert);

        signingKeys.Add(key);

        return Task.FromResult(signingKeys.AsEnumerable());
    }
}

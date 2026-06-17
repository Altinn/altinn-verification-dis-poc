#nullable enable

using Altinn.Common.AccessToken.Services;
using Altinn.Verification.Core;
using Altinn.Verification.Core.Integrations;
using Altinn.Verification.Tests.IntegrationTests.Mocks;
using Altinn.Verification.Tests.IntegrationTests.Mocks.Authentication;

using AltinnCore.Authentication.JwtCookie;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

namespace Altinn.Verification.Tests.IntegrationTests;

/// <summary>
/// Web application factory for integration tests of the Altinn Verification API.
/// </summary>
/// <typeparam name="TProgram">The entry point class of the application under test.</typeparam>
public sealed class VerificationWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    /// <summary>
    /// Gets or sets the mock for <see cref="IAddressVerificationRepository"/>.
    /// </summary>
    public Mock<IAddressVerificationRepository> AddressVerificationRepositoryMock { get; set; } = new();

    /// <summary>
    /// Gets or sets the mock for <see cref="INotificationsClient"/>.
    /// </summary>
    public Mock<INotificationsClient> NotificationsClientMock { get; set; } = new();

    /// <summary>
    /// Gets or sets the mock for <see cref="IUserLanguageService"/>.
    /// </summary>
    public Mock<IUserLanguageService> UserLanguageServiceMock { get; set; } = new();

    /// <summary>
    /// Gets or sets additional in-memory configuration entries.
    /// </summary>
    public Dictionary<string, string?> InMemoryConfigurationCollection { get; set; } = new();

    /// <inheritdoc/>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(LogLevel.Warning);
        });

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.SetBasePath(Directory.GetCurrentDirectory());
            config.AddJsonFile("appsettings.test.json");
            config.AddInMemoryCollection(InMemoryConfigurationCollection);
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
            services.AddSingleton<IPublicSigningKeyProvider, PublicSigningKeyProviderMock>();

            services.AddSingleton(AddressVerificationRepositoryMock.Object);
            services.AddSingleton(NotificationsClientMock.Object);
            services.AddSingleton(UserLanguageServiceMock.Object);

            services.AddSingleton(sp =>
            {
                var settings = new AddressMaintenanceSettings
                {
                    VerificationCodeResendCooldownSeconds = 60
                };
                var optionsMock = new Mock<IOptions<AddressMaintenanceSettings>>();
                optionsMock.Setup(o => o.Value).Returns(settings);
                return optionsMock.Object;
            });
        });
    }
}

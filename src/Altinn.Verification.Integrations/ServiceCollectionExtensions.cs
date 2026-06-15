using Altinn.Common.AccessTokenClient.Configuration;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.Verification.Core.Integrations;
using Altinn.Verification.Integrations.AddressVerification;
using Altinn.Verification.Integrations.Notifications;
using Altinn.Verification.Integrations.Persistence;
using Altinn.Verification.Integrations.Repositories;
using Altinn.Verification.Integrations.UserLanguage;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Verification.Integrations;

/// <summary>
/// Extension methods for registering Altinn.Verification infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all verification infrastructure services to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The application configuration.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddVerificationServices(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = GetConnectionString(config);

        services.AddDbContextFactory<VerificationDbContext>(options =>
        {
            options.UseNpgsql(connectionString)
                   .UseSnakeCaseNamingConvention();
        });

        services.Configure<NotificationsSettings>(config.GetSection(nameof(NotificationsSettings)));
        services.Configure<AccessTokenSettings>(config.GetSection("AccessTokenSettings"));
        services.AddTransient<IAccessTokenGenerator, AccessTokenGenerator>();

        services.AddHttpClient<INotificationsClient, NotificationsClient>();

        services.AddScoped<IAddressVerificationRepository, AddressVerificationRepository>();
        services.AddScoped<IVerificationCodeService, VerificationCodeService>();
        services.AddScoped<IUserNotifier, UserNotifier>();
        services.AddScoped<IUserLanguageService, DefaultUserLanguageService>();

        return services;
    }

    private static string GetConnectionString(IConfiguration config)
    {
        var settings = config.GetSection(nameof(PostgreSqlSettings)).Get<PostgreSqlSettings>()
            ?? throw new InvalidOperationException("PostgreSqlSettings is missing from configuration.");

        var connectionString = settings.ConnectionString;
        if (!string.IsNullOrEmpty(settings.VerificationDbPwd))
        {
            connectionString = connectionString.Replace("{verificationDbPwd}", settings.VerificationDbPwd, StringComparison.Ordinal);
        }

        return connectionString;
    }
}

using Altinn.Verification.Core.AddressVerifications;

using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Verification.Core.Extensions
{
    /// <summary>
    /// Extension methods for registering core verification services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the core verification services to the service collection.
        /// </summary>
        /// <param name="services">The service collection to add the services to.</param>
        /// <returns>The same service collection for chaining.</returns>
        public static IServiceCollection AddCoreVerificationServices(this IServiceCollection services)
        {
            services.AddScoped<IAddressVerificationService, AddressVerificationService>();
            services.AddSingleton<Telemetry.Telemetry>();

            return services;
        }
    }
}

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Altinn.Common.AccessToken.Services;
using Altinn.Common.PEP.Authorization;
using Altinn.Common.PEP.Implementation;
using Altinn.Verification.Authorization;
using Altinn.Verification.Configuration;
using Altinn.Verification.Core;
using Altinn.Verification.Core.Extensions;
using Altinn.Verification.Core.Telemetry;
using Altinn.Verification.Integrations;
using Altinn.Verification.Integrations.Persistence;

using AltinnCore.Authentication.JwtCookie;

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

ILogger logger;

var builder = WebApplication.CreateBuilder(args);

ConfigureWebHostCreationLogging();
ConfigureApplicationLogging(builder.Logging);
ConfigureServices(builder.Services, builder.Configuration);

WebApplication app = builder.Build();

if (args.Contains("--run-db-migrations"))
{
    await RunMigrationsAsync();
    return;
}

Configure();

await app.RunAsync();

void ConfigureWebHostCreationLogging()
{
    var logFactory = LoggerFactory.Create(b =>
    {
        b.AddFilter("Altinn.Verification.Program", LogLevel.Debug)
         .AddConsole();
    });

    logger = logFactory.CreateLogger<Program>();
}

void ConfigureApplicationLogging(ILoggingBuilder logging)
{
    logging.AddOpenTelemetry(b =>
    {
        b.IncludeFormattedMessage = true;
        b.IncludeScopes = true;
    });
}

void ConfigureServices(IServiceCollection services, IConfiguration config)
{
    logger.LogInformation("Program // ConfigureServices");

    services.AddOpenTelemetry()
        .ConfigureResource(r =>
            r.AddService(serviceName: Telemetry.AppName, serviceInstanceId: Environment.MachineName))
        .WithMetrics(metrics =>
        {
            metrics.AddAspNetCoreInstrumentation();
            metrics.AddMeter(
                "Microsoft.AspNetCore.Hosting",
                "Microsoft.AspNetCore.Server.Kestrel",
                "System.Net.Http",
                Telemetry.AppName);
        })
        .WithTracing(tracing =>
        {
            tracing.AddSource(Telemetry.AppName);
            tracing.AddAspNetCoreInstrumentation();
            tracing.AddHttpClientInstrumentation();
            tracing.AddEntityFrameworkCoreInstrumentation();

            if (builder.Environment.IsDevelopment())
            {
                tracing.SetSampler(new AlwaysOnSampler());
            }
        });

    services.AddControllers();
    services.AddMemoryCache();

    services.Configure<GeneralSettings>(config.GetSection("GeneralSettings"));
    services.Configure<AddressMaintenanceSettings>(config.GetSection("AddressMaintenanceSettings"));

    services.AddSingleton<IPublicSigningKeyProvider, PublicSigningKeyProvider>();

    services.AddAuthentication(JwtCookieDefaults.AuthenticationScheme)
        .AddJwtCookie(JwtCookieDefaults.AuthenticationScheme, options =>
        {
            var generalSettings = config.GetSection("GeneralSettings").Get<GeneralSettings>()
                ?? throw new InvalidOperationException("GeneralSettings is missing from configuration.");
            options.JwtCookieName = generalSettings.JwtCookieName;
            options.MetadataAddress = generalSettings.OpenIdWellKnownEndpoint;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            if (builder.Environment.IsDevelopment())
            {
                options.RequireHttpsMetadata = false;
            }
        });

    services.AddAuthorizationBuilder()
        .AddPolicy(AuthConstants.PortalEndUserAccess, policy =>
            policy.Requirements.Add(new ScopeAccessRequirement("altinn:portal/enduser")));

    services.AddScoped<IAuthorizationHandler, ScopeAccessHandler>();

    services.AddCoreVerificationServices();
    services.AddVerificationServices(config);
    services.AddProblemDetails();

    services.AddSwaggerGen(opts =>
    {
        opts.SwaggerDoc("v1", new OpenApiInfo { Title = "Altinn Verification", Version = "v1" });

        try
        {
            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            opts.IncludeXmlComments(xmlPath);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Program // Exception when attempting to include XML comments file.");
        }
    });
}

void Configure()
{
    logger.LogInformation("Program // Configure {AppName}", app.Environment.ApplicationName);

    if (app.Environment.IsDevelopment())
    {
        IdentityModelEventSource.ShowPII = true;
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
}

async Task RunMigrationsAsync()
{
    var migrationLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseMigration");

    var settings = builder.Configuration.GetSection("PostgreSqlSettings").Get<PostgreSqlSettings>()
        ?? throw new InvalidOperationException("PostgreSqlSettings is missing from configuration.");

    if (!settings.EnableDBConnection)
    {
        migrationLogger.LogWarning("Database connection is disabled, skipping migrations.");
        return;
    }

    try
    {
        migrationLogger.LogInformation("Database migration started.");

        var adminConnectionString = settings.AdminConnectionString;
        if (!string.IsNullOrEmpty(settings.VerificationDbAdminPwd))
        {
            adminConnectionString = adminConnectionString.Replace("{verificationDbAdminPwd}", settings.VerificationDbAdminPwd, StringComparison.Ordinal);
        }

        var options = new DbContextOptionsBuilder<VerificationDbContext>()
            .UseNpgsql(adminConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        using var context = new VerificationDbContext(options);
        var pendingCount = (await context.Database.GetPendingMigrationsAsync()).Count();

        if (pendingCount > 0)
        {
            migrationLogger.LogInformation("Applying {PendingMigrationCount} pending migrations.", pendingCount);
            await context.Database.MigrateAsync();
            migrationLogger.LogInformation("Database migrations applied successfully.");
        }
        else
        {
            migrationLogger.LogInformation("Database is up to date.");
        }
    }
    catch (Exception ex)
    {
        migrationLogger.LogError(ex, "Database migration failed.");
        Environment.Exit(1);
    }
}

/// <summary>
/// Startup class.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed partial class Program
{
    private Program()
    {
    }
}

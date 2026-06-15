using Altinn.Verification.Core.AddressVerifications.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Altinn.Verification.Integrations.Persistence;

/// <summary>
/// EF Core database context for the address verification schema.
/// </summary>
public class VerificationDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VerificationDbContext"/> class.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    public VerificationDbContext(DbContextOptions<VerificationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the verification codes table.
    /// </summary>
    public DbSet<VerificationCode> VerificationCodes { get; set; }

    /// <summary>
    /// Gets or sets the verified addresses table.
    /// </summary>
    public DbSet<VerifiedAddress> VerifiedAddresses { get; set; }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<VerificationCode>(entity =>
        {
            entity.ToTable("verification_codes", "address_verifications");
            entity.HasKey(e => e.VerificationCodeId).HasName("verification_code_id_pkey");
            entity.Property(e => e.VerificationCodeId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Address).IsRequired();
            entity.Property(e => e.AddressType).IsRequired().HasConversion(new EnumToStringConverter<AddressType>());
            entity.Property(e => e.VerificationCodeHash).IsRequired();
            entity.Property(e => e.Expires).IsRequired();
            entity.Property(e => e.Created).IsRequired();
            entity.Property(e => e.FailedAttempts).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.HasIndex(e => new { e.UserId, e.Address, e.AddressType }, "ix_user_id_address_address_type").IsUnique();
        });

        modelBuilder.Entity<VerifiedAddress>(entity =>
        {
            entity.ToTable("verified_addresses", "address_verifications");
            entity.HasKey(e => e.VerifiedAddressId);
            entity.Property(e => e.VerifiedAddressId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Address).IsRequired();
            entity.Property(e => e.AddressType).IsRequired().HasConversion(new EnumToStringConverter<AddressType>());
            entity.Property(e => e.VerifiedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).IsRequired();
            entity.Ignore(e => e.VerificationType);

            entity.HasIndex(e => new { e.UserId, e.Address, e.AddressType }, "ix_user_id_address_address_type").IsUnique();
        });
    }
}

using Auditor.Source;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;

namespace Auditor;

public static class AuditorConfig
{
    /// <summary>
    /// Configures the DbContext options builder with the AuditInterceptor.
    /// </summary>
    /// <param name="optionsBuilder">The DbContext options builder.</param>
    public static void Configure(DbContextOptionsBuilder optionsBuilder, ILogger? logger = null)
    {
        var auditEntries = new List<AuditEntry>();
        optionsBuilder.AddInterceptors(new AuditInterceptor(auditEntries, logger));
    }
}

public class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    /// <summary>
    /// Configures the entity type for AuditEntry.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("AuditEntries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Sequence).UseIdentityColumn();
        builder.Property(e => e.Metadata).IsRequired();
        builder.Property(e => e.StartTimeUtc).IsRequired();
        builder.Property(e => e.EndTimeUtc).IsRequired();
        builder.Property(e => e.Duration).IsRequired();
        builder.Property(e => e.Succeeded).IsRequired();
        builder.Property(e => e.ErrorMessage).IsRequired(false);
    }
}

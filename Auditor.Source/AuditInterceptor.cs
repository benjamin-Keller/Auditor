using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auditor.Source;

public class AuditorConfig
{
    /// <summary>
    /// Configures the DbContext options builder with the AuditInterceptor.
    /// </summary>
    /// <param name="optionsBuilder">The DbContext options builder.</param>
    public static void Configure(DbContextOptionsBuilder optionsBuilder)
    {
        var auditEntries = new List<AuditEntry>();
        optionsBuilder.AddInterceptors(new AuditInterceptor(auditEntries));
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
        builder.Property(e => e.Metadata).IsRequired();
        builder.Property(e => e.StartTimeUtc).IsRequired();
        builder.Property(e => e.EndTimeUtc).IsRequired();
        builder.Property(e => e.Succeeded).IsRequired();
        builder.Property(e => e.ErrorMessage).IsRequired(false);
    }
}

public class AuditInterceptor(List<AuditEntry> auditEntries) : SaveChangesInterceptor
{
    private readonly List<AuditEntry> _auditEntries = auditEntries ?? new();

    /// <summary>
    /// Overrides the SavingChangesAsync method to intercept and track audit entries before saving changes.
    /// </summary>
    /// <param name="eventData">The DbContext event data.</param>
    /// <param name="result">The interception result.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The interception result.</returns>
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var saveTask = base.SavingChangesAsync(eventData, result, cancellationToken);

        if (eventData.Context is null)
            return await saveTask;

        var startTime = DateTime.UtcNow;

        var auditEntries = eventData.Context.ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditEntry && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(entry => new AuditEntry
            {
                Id = Guid.NewGuid(), //To use Version7 Guid
                StartTimeUtc = startTime,
                Metadata = entry.DebugView.LongView,
            }).ToList();

        if (auditEntries.Count == 0)
            return await saveTask;

        _auditEntries.AddRange(auditEntries);

        return await saveTask;
    }

    /// <summary>
    /// Overrides the SavedChangesAsync method to update audit entries and save them to the database after changes are saved.
    /// </summary>
    /// <param name="eventData">The SaveChangesCompletedEventData.</param>
    /// <param name="result">The result of the saved changes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the saved changes.</returns>
    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        var saveTask = base.SavedChangesAsync(eventData, result, cancellationToken);

        if (eventData.Context is null)
            return await saveTask;

        var endTime = DateTime.UtcNow;

        foreach (var entry in _auditEntries)
        {
            entry.EndTimeUtc = endTime;
            entry.Succeeded = true;
        }

        if (_auditEntries.Count > 0)
        {
            eventData.Context.Set<AuditEntry>().AddRange(_auditEntries);
            _auditEntries.Clear();
            await eventData.Context.SaveChangesAsync(cancellationToken);
        }

        return await saveTask;
    }

    /// <summary>
    /// Overrides the SaveChangesFailedAsync method to handle save changes failure.
    /// </summary>
    /// <param name="eventData">The DbContext error event data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }
}

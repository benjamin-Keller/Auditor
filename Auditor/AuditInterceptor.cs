using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Auditor.Source;

/// <summary>
/// Represents an audit interceptor that tracks and saves audit entries before and after saving changes.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AuditInterceptor"/> class.
/// </remarks>
/// <param name="auditEntries">The list of audit entries.</param>
/// <param name="logger">The logger.</param>
public class AuditInterceptor(List<AuditEntry> auditEntries, ILogger? logger = null) : SaveChangesInterceptor
{
    private readonly List<AuditEntry> _auditEntries = auditEntries ?? new();
    private readonly ILogger? _logger = logger;

    /// <summary>
    /// Overrides the SavingChangesAsync method to intercept and track audit entries before saving changes.
    /// </summary>
    /// <param name="eventData">The DbContext event data.</param>
    /// <param name="result">The interception result.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The interception result.</returns>
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var startTime = DateTime.UtcNow;

        var auditEntries = eventData.Context.ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditEntry && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(entry => new AuditEntry
            {
                Id = Guid.NewGuid(), //To use Version7 Guid
                StartTimeUtc = startTime,
                Metadata = entry.DebugView.LongView,
                AuditType = entry.State.ToString()
            }).ToList();

        if (auditEntries.Count == 0)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        if (_logger is not null)
            _logger.LogDebug("[Auditor] {0} audit entries tracked at {1}.", auditEntries.Count, startTime);

        _auditEntries.AddRange(auditEntries);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
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
        if (eventData.Context is null)
            return await base.SavedChangesAsync(eventData, result, cancellationToken);

        var endTime = DateTime.UtcNow;

        foreach (var entry in _auditEntries)
        {
            entry.EndTimeUtc = endTime;
            entry.Duration = entry.EndTimeUtc - entry.StartTimeUtc;
            entry.Succeeded = true;
        }

        if (_auditEntries.Count > 0)
        {
            eventData.Context.Set<AuditEntry>().AddRange(_auditEntries);
            _auditEntries.Clear();
            await eventData.Context.SaveChangesAsync(cancellationToken);
            Console.WriteLine();
            if (_logger is not null)
                _logger.LogDebug("[Auditor] {0} changes saved at {1}.", result, endTime);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Overrides the SaveChangesFailedAsync method to handle save changes failure.
    /// </summary>
    /// <param name="eventData">The DbContext error event data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            await base.SaveChangesFailedAsync(eventData, cancellationToken);
            return;
        }

        var endTime = DateTime.UtcNow;

        foreach (var entry in _auditEntries)
        {
            entry.EndTimeUtc = endTime;
            entry.Duration = entry.EndTimeUtc - entry.StartTimeUtc;
            entry.Succeeded = true;
        }

        if (_auditEntries.Count > 0)
        {
            eventData.Context.Set<AuditEntry>().AddRange(_auditEntries);
            await eventData.Context.SaveChangesAsync(cancellationToken);
            Console.WriteLine();
            if (_logger is not null)
                _logger.LogDebug("[Auditor] {0} changes failed at {1}.", _auditEntries.Count, endTime);

            _auditEntries.Clear();
        }

        await base.SaveChangesFailedAsync(eventData, cancellationToken);
    }
}

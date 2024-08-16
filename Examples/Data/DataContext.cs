using Auditor.Models;
using Microsoft.EntityFrameworkCore;

namespace Auditor.Data;

public class DataContext : DbContext
{
    private readonly DbContextOptions<DataContext> _options;
    private readonly ILogger<DataContext> _logger;

    public DataContext(DbContextOptions<DataContext> options, ILogger<DataContext> logger) : base(options)
    {
        _options = options;
        _logger = logger;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        AuditorConfig.Configure(optionsBuilder, _logger);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new AuditEntryConfiguration());
    }

    public DbSet<SuperHero> SuperHeroes { get; set; }
}

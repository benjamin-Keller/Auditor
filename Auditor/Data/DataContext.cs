using Auditor.Models;
using Auditor.Source;
using Microsoft.EntityFrameworkCore;

namespace Auditor.Data;

public class DataContext : DbContext
{
    private readonly DbContextOptions<DataContext> _options;

    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        _options = options;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        AuditorConfig.Configure(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new AuditEntryConfiguration());
    }

    public DbSet<SuperHero> SuperHeroes { get; set; }
}

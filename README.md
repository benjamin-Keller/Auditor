# Auditor

Auditor is a simple tool to audit your codebase for common issues. It is designed to be fast and easy to use, and to provide a simple way to check for common issues in your codebase.

## Usage

To use Auditor, follow these steps:

1. Install Auditor package from NuGet. (Not yet on NuGet)
2. Call `builder.Services.SetupAudit()` in your `Program.cs`.
3. In your `DataContext`:
  - override `OnConfiguring` and call `AuditorConfig.Configure(optionsBuilder);`
  - override `OnModelCreating` and call `modelBuilder.ApplyConfiguration(new AuditEntryConfiguration());`
3. Run a database migration to create the `AuditEntry` table.
4. Review the audit results and take necessary actions to fix the issues.

## Example

Here is an example of how to use Auditor:
In your `Program.cs`:

```csharp
...
builder.Services.AddTransient<SuperHeroService>();
builder.Services.SetupAudit();

var config = builder.Configuration;
...
```

In your `DataContext.cs`:

```csharp
...
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    AuditorConfig.Configure(optionsBuilder);
}

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfiguration(new AuditEntryConfiguration());
}
...
```

## Features

- Automatic auditing of your database.
- Provides actionable insights to improve code quality.
- Easy to implement database auditing.

## Contributing

Contributions are welcome! If you find any issues or have suggestions for improvement, please open an issue or submit a pull request.

## License

Auditor is licensed under the MIT License. See the [LICENSE](https://github.com/your-username/auditor/blob/main/LICENSE) file for more details.

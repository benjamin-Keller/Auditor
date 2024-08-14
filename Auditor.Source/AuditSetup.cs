using Microsoft.Extensions.DependencyInjection;

namespace Auditor.Source;

public static class AuditSetup
{
    public static IServiceCollection SetupAudit(this IServiceCollection services)
    {
        return services.AddKeyedScoped<List<AuditEntry>>("Audit", (_, _) => new());
    }
}

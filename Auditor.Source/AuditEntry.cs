using System.ComponentModel.DataAnnotations;

namespace Auditor.Source;

public class AuditEntry
{
    [Key]
    public Guid Id { get; set; }
    public string Metadata { get; set; } = string.Empty;
    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
    public bool Succeeded { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

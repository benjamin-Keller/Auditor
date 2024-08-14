using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auditor.Source;

public class AuditEntry
{
    [Key]
    public Guid Id { get; set; }
    [NotMapped]
    public string AuditType { get; set; } = string.Empty;
    public string Metadata { get; set; } = string.Empty;
    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
    public TimeSpan Duration { get; set; }
    public bool Succeeded { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

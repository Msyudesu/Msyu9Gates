using System.ComponentModel.DataAnnotations;

namespace Msyu9Gates.Data.Models;

public class GateKey
{
    [Key] public int Id { get; set; }
    public int GateId { get; set; }
    public int ChapterId { get; set; }
    public int KeyNumber { get; set; }
    public string? KeyValue { get; set; } = string.Empty;
    public bool Discovered { get; set; } = false;
    public DateTimeOffset? DateDiscoveredUtc { get; set; } = null;
}

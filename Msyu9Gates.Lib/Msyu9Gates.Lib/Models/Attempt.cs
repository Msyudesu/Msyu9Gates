using System.ComponentModel.DataAnnotations;

namespace Msyu9Gates.Lib.Models;

public class Attempt
{
    [Key] public int Id { get; set; }
    public DateTimeOffset AttemptedAtUtc { get; set; }
    public int UserId { get; set; }
    public int GateId { get; set; }
    public int ChapterId { get; set; }
    public string? AttemptValue { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace Msyu9Gates.Data.Models;

public class Gate
{
    [Key] public int Id { get; set; }
    public int GateNumber { get; set; }
    public int GateOverallDifficultyLevel { get; set; }
    public bool IsCompleted { get; set; } = false;
    public bool IsLocked { get; set; } = true;
    public DateTimeOffset? DateUnlocked { get; set; }
    public DateTimeOffset? DateCompleted { get; set; }
    public string? Narrative { get; set; } = string.Empty;
    public string? Conclusion { get; set; } = string.Empty;

    public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
}
